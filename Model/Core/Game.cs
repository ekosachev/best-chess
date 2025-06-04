using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public class Game
    {
        private static Game _instance;
        private Stack<Figure[,]> _boardHistory = new Stack<Figure[,]>();
        private Stack<string> _playerHistory = new Stack<string>();
        private Dictionary<string, int> _positionCounts = new Dictionary<string, int>();

        public static Game Instance => _instance ??= new Game();
        public Figure[,] Board { get; private set; } = new Figure[8, 8];
        public string CurrentPlayer { get; private set; } = "White";
        public Pawn LastPawn { get; private set; }
        public bool LastMoveWasDoublePawnPush { get; private set; }
        public bool IsCheck { get; private set; }
        public bool IsCheckmate { get; private set; }
        public bool IsStalemate { get; private set; }
        public bool IsDrawByRepetition { get; private set; }
        public (int row, int col)? EnPassantTarget { get; private set; }

        public Game()
        {
            InitializeBoard();
            SaveGameState();
        }

        private void InitializeBoard()
        {
            // Расстановка пешек
            for (int col = 0; col < 8; col++)
            {
                Board[1, col] = new Pawn("Black", (1, col));
                Board[6, col] = new Pawn("White", (6, col));
            }

            // Расстановка остальных фигур
            string[] backRowOrder = { "Rook", "Knight", "Bishop", "Queen", "King", "Bishop", "Knight", "Rook" };

            for (int col = 0; col < 8; col++)
            {
                Board[0, col] = CreateFigure(backRowOrder[col], "Black", (0, col));
                Board[7, col] = CreateFigure(backRowOrder[col], "White", (7, col));
            }
        }

        private Figure CreateFigure(string type, string color, (int, int) position)
        {
            return type switch
            {
                "Pawn" => new Pawn(color, position),
                "Rook" => new Rook(color, position),
                "Knight" => new Knight(color, position),
                "Bishop" => new Bishop(color, position),
                "Queen" => new Queen(color, position),
                "King" => new King(color, position),
                _ => throw new ArgumentException("Unknown figure type")
            };
        }

        private void SaveGameState()
        {
            var boardCopy = (Figure[,])Board.Clone();
            _boardHistory.Push(boardCopy);
            _playerHistory.Push(CurrentPlayer);

            var positionKey = GeneratePositionKey();
            _positionCounts.TryGetValue(positionKey, out int count);
            _positionCounts[positionKey] = count + 1;
            IsDrawByRepetition = _positionCounts[positionKey] >= 3;
        }

        private string GeneratePositionKey()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    sb.Append(Board[i, j]?.ToString() ?? ".");
                }
            }
            sb.Append(CurrentPlayer);
            sb.Append(EnPassantTarget?.ToString() ?? "null");
            return sb.ToString();
        }

        public void UndoLastMove()
        {
            if (_boardHistory.Count > 1)
            {
                var currentKey = GeneratePositionKey();
                if (_positionCounts.ContainsKey(currentKey))
                {
                    _positionCounts[currentKey]--;
                }

                _boardHistory.Pop();
                Board = _boardHistory.Peek();
                CurrentPlayer = _playerHistory.Pop();
                UpdateGameStatus();
            }
        }

        public bool MakeMove((int row, int col) from, (int row, int col) to)
        {
            var figure = Board[from.row, from.col];
            if (figure == null || figure.Color != CurrentPlayer)
                return false;

            if (!GetValidMoves(figure).Contains(to))
                return false;

            SaveGameState();
            HandleSpecialMoves(figure, from, to);

            Board[to.row, to.col] = figure;
            Board[from.row, from.col] = null;
            figure.Position = to;
            figure.HasMoved = true;

            HandlePawnPromotion(figure, to);
            CurrentPlayer = CurrentPlayer == "White" ? "Black" : "White";
            UpdateGameStatus();

            return true;
        }

        private void HandleSpecialMoves(Figure figure, (int row, int col) from, (int row, int col) to)
        {
            if (figure is Pawn && from.col != to.col && Board[to.row, to.col] == null)
            {
                Board[from.row, to.col] = null;
            }

            if (figure is King && Math.Abs(from.col - to.col) == 2)
            {
                HandleCastling(from, to);
            }

            if (figure is Pawn pawn && Math.Abs(from.row - to.row) == 2)
            {
                LastPawn = pawn;
                LastMoveWasDoublePawnPush = true;
                EnPassantTarget = ((from.row + to.row) / 2, from.col);
            }
            else
            {
                LastMoveWasDoublePawnPush = false;
                EnPassantTarget = null;
            }
        }

        private void HandleCastling((int row, int col) from, (int row, int col) to)
        {
            int rookCol = to.col > from.col ? 7 : 0;
            int newRookCol = to.col > from.col ? 5 : 3;

            var rook = Board[from.row, rookCol];
            Board[from.row, newRookCol] = rook;
            Board[from.row, rookCol] = null;
            rook.Position = (from.row, newRookCol);
            rook.HasMoved = true;
        }

        private void HandlePawnPromotion(Figure figure, (int row, int col) to)
        {
            if (figure is Pawn && (to.row == 0 || to.row == 7))
            {
                Board[to.row, to.col] = new Queen(figure.Color, to);
            }
        }

        public List<(int row, int col)> GetValidMoves(Figure figure)
        {
            return figure.GetRawMoves(Board)
                .Where(move => IsMoveValid(figure, move))
                .ToList();
        }

        private bool IsMoveValid(Figure figure, (int row, int col) move)
        {
            if (!IsInsideBoard(move.row, move.col))
                return false;

            if (Board[move.row, move.col]?.Color == figure.Color)
                return false;

            if (figure is King king)
                return IsKingMoveValid(king, move);

            return !WouldMoveExposeKing(figure, move);
        }

        private bool IsKingMoveValid(King king, (int row, int col) move)
        {
            // Проверяем, не окажется ли король под шахом после хода
            if (WouldPositionBeUnderAttack(move, king.Color))
                return false;

            // Дополнительная проверка для взятия фигур
            if (Board[move.row, move.col] != null)
            {
                // Временно выполняем взятие
                var target = Board[move.row, move.col];
                Board[move.row, move.col] = king;
                Board[king.Position.row, king.Position.col] = null;
                var originalPos = king.Position;
                king.Position = move;

                bool isCheck = IsKingInCheck(king.Color);

                // Отменяем временное взятие
                Board[move.row, move.col] = target;
                Board[originalPos.row, originalPos.col] = king;
                king.Position = originalPos;

                if (isCheck)
                    return false;
            }

            if (Math.Abs(move.col - king.Position.col) == 2)
                return IsCastlingValid(king, move);

            return true;
        }

        private bool IsCastlingValid(King king, (int row, int col) move)
        {
            if (WouldPositionBeUnderAttack(king.Position, king.Color))
                return false;

            int direction = move.col > king.Position.col ? 1 : -1;
            int rookCol = direction > 0 ? 7 : 0;

            var rook = Board[king.Position.row, rookCol] as Rook;
            if (rook == null || rook.Color != king.Color || rook.HasMoved)
                return false;

            int start = Math.Min(king.Position.col, rookCol) + 1;
            int end = Math.Max(king.Position.col, rookCol);
            for (int col = start; col < end; col++)
            {
                if (Board[king.Position.row, col] != null)
                    return false;
            }

            for (int col = king.Position.col; col != move.col; col += direction)
            {
                if (WouldPositionBeUnderAttack((king.Position.row, col), king.Color))
                    return false;
            }

            return true;
        }

        private bool WouldPositionBeUnderAttack((int row, int col) position, string playerColor)
        {
            return IsSquareUnderAttack(position, playerColor == "White" ? "Black" : "White");
        }

        private bool IsSquareUnderAttack((int row, int col) position, string byColor)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var figure = Board[i, j];
                    if (figure != null && figure.Color == byColor)
                    {
                        if (figure is Pawn pawn)
                        {
                            int direction = pawn.Color == "White" ? -1 : 1;
                            if ((position.col == j - 1 || position.col == j + 1) &&
                                position.row == i + direction)
                                return true;
                        }
                        else if (figure.GetRawMoves(Board).Contains(position))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void UpdateGameStatus()
        {
            IsCheck = IsKingInCheck(CurrentPlayer);
            IsCheckmate = IsCheck && !PlayerHasValidMoves(CurrentPlayer);
            IsStalemate = !IsCheck && !PlayerHasValidMoves(CurrentPlayer);
        }

        private bool IsKingInCheck(string playerColor)
        {
            var kingPos = FindKing(playerColor);
            return IsSquareUnderAttack(kingPos, playerColor == "White" ? "Black" : "White");
        }

        private bool PlayerHasValidMoves(string playerColor)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var figure = Board[i, j];
                    if (figure != null && figure.Color == playerColor)
                    {
                        foreach (var move in GetRawMoves(figure))
                        {
                            if (IsMoveValid(figure, move))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        private (int row, int col) FindKing(string color)
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if (Board[i, j] is King king && king.Color == color)
                        return (i, j);
            return (-1, -1);
        }

        private bool IsInsideBoard(int row, int col)
        {
            return row >= 0 && row < 8 && col >= 0 && col < 8;
        }

        private List<(int row, int col)> GetRawMoves(Figure figure)
        {
            return figure.GetRawMoves(Board);
        }

        private bool WouldMoveExposeKing(Figure figure, (int row, int col) move)
        {
            var originalPos = figure.Position;
            var target = Board[move.row, move.col];

            Board[move.row, move.col] = figure;
            Board[originalPos.row, originalPos.col] = null;
            figure.Position = move;

            bool isCheck = IsKingInCheck(figure.Color);

            Board[originalPos.row, originalPos.col] = figure;
            Board[move.row, move.col] = target;
            figure.Position = originalPos;

            return isCheck;
        }

        public void ResetGame()
        {
            Board = new Figure[8, 8];
            CurrentPlayer = "White";
            InitializeBoard();
            _boardHistory.Clear();
            _playerHistory.Clear();
            _positionCounts.Clear();
            SaveGameState();
            UpdateGameStatus();
        }
    }
}

