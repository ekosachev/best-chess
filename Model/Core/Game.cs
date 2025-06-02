using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public partial class Game
    {
        public static Game Instance { get; private set; }
        public Figure[,] Board { get; private set; } = new Figure[8, 8];
        public string CurrentPlayer { get; private set; } = "White";
        public bool IsCheck { get; private set; }
        public bool IsCheckmate { get; private set; }
        public bool IsStalemate { get; private set; }
        public Pawn LastPawn { get; private set; }
        public bool LastMoveWasDoublePawnPush { get; private set; }
        public int HalfMoveClock { get; private set; }
        public int FullMoveNumber { get; private set; } = 1;

        private readonly Stack<GameState> _history = new();
        private readonly Dictionary<string, int> _positionCounts = new();

        public event Action<string> OnCheck;
        public event Action<string> OnCheckmate;
        public event Action OnStalemate;
        public event Action OnThreefoldRepetition;
        public event Action OnFiftyMoveRuleDraw;
        public event Action<(int, int)> OnPawnPromotion;

        public Game()
        {
            Instance = this;
            InitializeBoard();
            SaveState();
        }

        private void InitializeBoard()
        {
            // Пешки
            for (int col = 0; col < 8; col++)
            {
                Board[1, col] = new Pawn("Black", (1, col));
                Board[6, col] = new Pawn("White", (6, col));
            }

            // Ладьи
            PlacePiece(0, 0, new Rook("Black", (0, 0)));
            PlacePiece(0, 7, new Rook("Black", (0, 7)));
            PlacePiece(7, 0, new Rook("White", (7, 0)));
            PlacePiece(7, 7, new Rook("White", (7, 7)));

            // Кони
            PlacePiece(0, 1, new Knight("Black", (0, 1)));
            PlacePiece(0, 6, new Knight("Black", (0, 6)));
            PlacePiece(7, 1, new Knight("White", (7, 1)));
            PlacePiece(7, 6, new Knight("White", (7, 6)));

            // Слоны
            PlacePiece(0, 2, new Bishop("Black", (0, 2)));
            PlacePiece(0, 5, new Bishop("Black", (0, 5)));
            PlacePiece(7, 2, new Bishop("White", (7, 2)));
            PlacePiece(7, 5, new Bishop("White", (7, 5)));

            // Ферзи
            PlacePiece(0, 3, new Queen("Black", (0, 3)));
            PlacePiece(7, 3, new Queen("White", (7, 3)));

            // Короли
            PlacePiece(0, 4, new King("Black", (0, 4)));
            PlacePiece(7, 4, new King("White", (7, 4)));
        }

        private void PlacePiece(int row, int col, Figure piece)
        {
            Board[row, col] = piece;
            piece.Position = (row, col);
        }

        public bool Move((int fromRow, int fromCol) from, (int toRow, int toCol) to)
        {
            if (!IsValidPosition(from) || !IsValidPosition(to))
                return false;

            var figure = Board[from.fromRow, from.fromCol];
            if (figure == null || figure.Color != CurrentPlayer)
                return false;

            // Специальные ходы
            if (figure is King && IsCastlingMove(from, to))
                return PerformCastling(from, to);

            if (!IsMoveAllowed(figure, from, to))
                return false;

            SaveState();
            ExecuteMove(figure, from, to);
            HandlePostMoveActions(figure, to);
            SwitchPlayer();
            UpdateGameState();

            return true;
        }

        private bool IsValidPosition((int row, int col) position)
            => position.row >= 0 && position.row < 8 && position.col >= 0 && position.col < 8;

        private bool IsCastlingMove((int, int) from, (int, int) to)
            => Math.Abs(from.Item2 - to.Item2) == 2;

        private bool IsMoveAllowed(Figure figure, (int, int) from, (int, int) to)
        {
            var availableMoves = figure.GetAvailableMoves(Board);
            return availableMoves.Contains((to.Item1, to.Item2)) &&
                   !WouldKingBeInCheck(figure, (to.Item1, to.Item2));
        }

        private bool WouldKingBeInCheck(Figure figure, (int, int) targetPos)
        {
            var tempBoard = CloneBoard();
            tempBoard[targetPos.Item1, targetPos.Item2] = figure;
            tempBoard[figure.Position.row, figure.Position.col] = null;
            return IsKingInCheck(figure.Color, tempBoard);
        }

        private Figure[,] CloneBoard()
        {
            var clone = new Figure[8, 8];
            Array.Copy(Board, clone, Board.Length);
            return clone;
        }

        private void ExecuteMove(Figure figure, (int fromRow, int fromCol) from, (int toRow, int toCol) to)
        {
            // Взятие на проходе
            if (figure is Pawn && to.toCol != from.fromCol && Board[to.toRow, to.toCol] == null)
            {
                Board[from.fromRow, to.toCol] = null;
                HalfMoveClock = 0;
            }

            // Обычное перемещение
            Board[to.toRow, to.toCol] = figure;
            Board[from.fromRow, from.fromCol] = null;
            figure.Position = (to.toRow, to.toCol);
            figure.HasMoved = true;

            // Обновление счетчиков
            UpdateMoveCounters(figure, Board[to.toRow, to.toCol] != null);
        }

        private void UpdateMoveCounters(Figure figure, bool captureOccurred)
        {
            if (captureOccurred || figure is Pawn)
                HalfMoveClock = 0;
            else
                HalfMoveClock++;
        }

        private void HandlePostMoveActions(Figure figure, (int toRow, int toCol) to)
        {
            if (figure is Pawn pawn)
            {
                HandlePawnSpecialCases(pawn, to);
                CheckPawnPromotion(pawn, to.toRow);
            }
        }

        private void HandlePawnSpecialCases(Pawn pawn, (int, int) to)
        {
            if (Math.Abs(pawn.Position.row - to.Item1) == 2)
            {
                LastPawn = pawn;
                LastMoveWasDoublePawnPush = true;
            }
            else
            {
                LastMoveWasDoublePawnPush = false;
            }
        }

        private void CheckPawnPromotion(Pawn pawn, int toRow)
        {
            if (toRow == 0 || toRow == 7)
                OnPawnPromotion?.Invoke((toRow, pawn.Position.col));
        }

        private bool PerformCastling((int, int) from, (int, int) to)
        {
            var king = Board[from.Item1, from.Item2] as King;
            int rookCol = to.Item2 > from.Item2 ? 7 : 0;
            var rook = Board[from.Item1, rookCol] as Rook;

            if (king == null || rook == null || king.HasMoved || rook.HasMoved)
                return false;

            if (!IsCastlingPathClear(from.Item1, from.Item2, rookCol))
                return false;

            SaveState();
            ExecuteCastling(king, rook, from, to, rookCol);
            return true;
        }

        private bool IsCastlingPathClear(int row, int kingCol, int rookCol)
        {
            int start = Math.Min(kingCol, rookCol) + 1;
            int end = Math.Max(kingCol, rookCol);

            for (int col = start; col < end; col++)
            {
                if (Board[row, col] != null || IsSquareUnderAttack(row, col, CurrentPlayer))
                    return false;
            }
            return true;
        }

        private void ExecuteCastling(King king, Rook rook, (int, int) from, (int, int) to, int rookCol)
        {
            // Перемещаем короля
            Board[to.Item1, to.Item2] = king;
            Board[from.Item1, from.Item2] = null;
            king.Position = (to.Item1, to.Item2);
            king.HasMoved = true;

            // Перемещаем ладью
            int newRookCol = to.Item2 > from.Item2 ? 5 : 3;
            Board[from.Item1, newRookCol] = rook;
            Board[from.Item1, rookCol] = null;
            rook.Position = (from.Item1, newRookCol);
            rook.HasMoved = true;

            UpdateMoveCounters(king, false);
        }

        private void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == "White" ? "Black" : "White";
            if (CurrentPlayer == "Black") FullMoveNumber++;
        }

        public bool IsKingInCheck(string color, Figure[,] board)
        {
            var kingPos = FindKing(color, board);
            return board.Cast<Figure>()
                .Where(f => f != null && f.Color != color)
                .Any(f => f.GetAvailableMoves(board).Contains(kingPos));
        }

        public bool IsSquareUnderAttack(int row, int col, string defenderColor)
        {
            return Board.Cast<Figure>()
                .Where(f => f != null && f.Color != defenderColor)
                .Any(f => f.GetAvailableMoves(Board).Contains((row, col)));
        }

        private (int, int) FindKing(string color, Figure[,] board)
        {
            for (int row = 0; row < 8; row++)
                for (int col = 0; col < 8; col++)
                    if (board[row, col] is King && board[row, col].Color == color)
                        return (row, col);
            throw new Exception("Король не найден!");
        }

        private void UpdateGameState()
        {
            IsCheck = IsKingInCheck(CurrentPlayer, Board);
            IsCheckmate = IsCheck && !HasAnyValidMove(CurrentPlayer);
            IsStalemate = !IsCheck && !HasAnyValidMove(CurrentPlayer);

            if (IsCheckmate) OnCheckmate?.Invoke(CurrentPlayer == "White" ? "Black" : "White");
            else if (IsCheck) OnCheck?.Invoke(CurrentPlayer);
            else if (IsStalemate) OnStalemate?.Invoke();
        }

        private bool HasAnyValidMove(string player)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var figure = Board[row, col];
                    if (figure != null && figure.Color == player &&
                        GetValidMoves(figure).Count > 0)
                        return true;
                }
            }
            return false;
        }

        private List<(int, int)> GetValidMoves(Figure figure)
        {
            return figure.GetAvailableMoves(Board)
                .Where(move => !WouldKingBeInCheck(figure, move))
                .ToList();
        }

        private void CheckSpecialRules()
        {
            string positionKey = GetBoardHash();
            _positionCounts[positionKey] = _positionCounts.GetValueOrDefault(positionKey) + 1;

            if (_positionCounts[positionKey] >= 3)
                OnThreefoldRepetition?.Invoke();
            if (HalfMoveClock >= 50)
                OnFiftyMoveRuleDraw?.Invoke();
        }

        private string GetBoardHash()
        {
            var hash = new System.Text.StringBuilder();
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var figure = Board[row, col];
                    hash.Append(figure?.Name[0] ?? '-');
                    hash.Append(figure?.Color[0] ?? '-');
                    hash.Append(figure?.HasMoved ?? false ? '1' : '0');
                }
            }
            hash.Append(CurrentPlayer);
            return hash.ToString();
        }

        public void PromotePawn((int, int) position, string newFigureType)
        {
            if (!(Board[position.Item1, position.Item2] is Pawn pawn))
                return;

            Figure newFigure = newFigureType switch
            {
                "Queen" => new Queen(pawn.Color, position),
                "Rook" => new Rook(pawn.Color, position),
                "Bishop" => new Bishop(pawn.Color, position),
                "Knight" => new Knight(pawn.Color, position),
                _ => throw new ArgumentException("Неизвестный тип фигуры")
            };

            Board[position.Item1, position.Item2] = newFigure;
            UpdateGameState();
        }

        private void SaveState()
        {
            _history.Push(new GameState
            {
                Board = CloneBoard(),
                CurrentPlayer = CurrentPlayer,
                HalfMoveClock = HalfMoveClock,
                FullMoveNumber = FullMoveNumber,
                LastPawn = LastPawn,
                LastMoveWasDoublePawnPush = LastMoveWasDoublePawnPush
            });
        }

        public bool UndoMove()
        {
            if (_history.Count <= 1) return false;

            var state = _history.Pop();
            Board = state.Board;
            CurrentPlayer = state.CurrentPlayer;
            HalfMoveClock = state.HalfMoveClock;
            FullMoveNumber = state.FullMoveNumber;
            LastPawn = state.LastPawn;
            LastMoveWasDoublePawnPush = state.LastMoveWasDoublePawnPush;

            UpdateGameState();
            return true;
        }

        private class GameState
        {
            public Figure[,] Board { get; set; }
            public string CurrentPlayer { get; set; }
            public int HalfMoveClock { get; set; }
            public int FullMoveNumber { get; set; }
            public Pawn LastPawn { get; set; }
            public bool LastMoveWasDoublePawnPush { get; set; }
        }
    }
}

