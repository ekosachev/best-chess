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

        private Stack<(Figure[,] board, string player, int halfMove, int fullMove)> _history = new();
        private Dictionary<string, int> _positionCounts = new();

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
            Board[0, 0] = new Rook("Black", (0, 0));
            Board[0, 7] = new Rook("Black", (0, 7));
            Board[7, 0] = new Rook("White", (7, 0));
            Board[7, 7] = new Rook("White", (7, 7));

            // Кони
            Board[0, 1] = new Knight("Black", (0, 1));
            Board[0, 6] = new Knight("Black", (0, 6));
            Board[7, 1] = new Knight("White", (7, 1));
            Board[7, 6] = new Knight("White", (7, 6));

            // Слоны
            Board[0, 2] = new Bishop("Black", (0, 2));
            Board[0, 5] = new Bishop("Black", (0, 5));
            Board[7, 2] = new Bishop("White", (7, 2));
            Board[7, 5] = new Bishop("White", (7, 5));

            // Ферзи
            Board[0, 3] = new Queen("Black", (0, 3));
            Board[7, 3] = new Queen("White", (7, 3));

            // Короли
            Board[0, 4] = new King("Black", (0, 4));
            Board[7, 4] = new King("White", (7, 4));
        }

        public bool Move((int fromRow, int fromCol) from, (int toRow, int toCol) to)
        {
            var figure = Board[from.fromRow, from.fromCol];
            if (figure == null || figure.Color != CurrentPlayer) return false;

            // Рокировка
            if (figure is King && Math.Abs(from.fromCol - to.toCol) == 2)
                return PerformCastling(from, to);

            var validMoves = GetValidMoves(figure);
            if (!validMoves.Contains((to.toRow, to.toCol))) return false;

            SaveState();

            // Взятие на проходе
            if (figure is Pawn && to.toCol != from.fromCol && Board[to.toRow, to.toCol] == null)
            {
                Board[from.fromRow, to.toCol] = null;
                HalfMoveClock = 0;
            }

            // Обновляем счетчики ходов
            if (Board[to.toRow, to.toCol] != null || figure is Pawn)
                HalfMoveClock = 0;
            else
                HalfMoveClock++;

            // Выполняем ход
            Board[to.toRow, to.toCol] = figure;
            Board[from.fromRow, from.fromCol] = null;
            figure.Position = (to.toRow, to.toCol);
            figure.HasMoved = true;

            // Превращение пешки
            if (figure is Pawn && (to.toRow == 0 || to.toRow == 7))
            {
                OnPawnPromotion?.Invoke((to.toRow, to.toCol));
                return true;
            }

            // Запоминаем последний ход пешки
            if (figure is Pawn && Math.Abs(from.fromRow - to.toRow) == 2)
            {
                LastPawn = (Pawn)figure;
                LastMoveWasDoublePawnPush = true;
            }
            else
            {
                LastMoveWasDoublePawnPush = false;
            }

            if (CurrentPlayer == "Black") FullMoveNumber++;
            CurrentPlayer = CurrentPlayer == "White" ? "Black" : "White";

            UpdateGameStatus();
            CheckSpecialRules();

            return true;
        }

        private bool PerformCastling((int, int) from, (int, int) to)
        {
            var king = Board[from.Item1, from.Item2] as King;
            int rookCol = to.Item2 > from.Item2 ? 7 : 0;
            var rook = Board[from.Item1, rookCol] as Rook;

            if (king == null || rook == null || king.HasMoved || rook.HasMoved)
                return false;

            // Проверка пути
            int start = Math.Min(from.Item2, rookCol) + 1;
            int end = Math.Max(from.Item2, rookCol);
            for (int col = start; col < end; col++)
                if (Board[from.Item1, col] != null) return false;

            for (int col = from.Item2; col != rookCol; col += Math.Sign(rookCol - from.Item2))
                if (IsSquareUnderAttack(from.Item1, col, CurrentPlayer))
                    return false;

            SaveState();

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

            CurrentPlayer = CurrentPlayer == "White" ? "Black" : "White";
            HalfMoveClock++;
            if (CurrentPlayer == "Black") FullMoveNumber++;

            UpdateGameStatus();
            return true;
        }

        private List<(int, int)> GetValidMoves(Figure figure)
        {
            var moves = figure.GetAvailableMoves(Board);
            var validMoves = new List<(int, int)>();

            foreach (var move in moves)
            {
                var tempBoard = (Figure[,])Board.Clone();
                tempBoard[move.Item1, move.Item2] = figure;
                tempBoard[figure.Position.row, figure.Position.col] = null;

                if (!IsKingInCheck(figure.Color, tempBoard))
                    validMoves.Add(move);
            }

            return validMoves;
        }

        public bool IsKingInCheck(string color, Figure[,] board)
        {
            var kingPos = FindKing(color, board);
            foreach (var figure in board)
            {
                if (figure != null && figure.Color != color &&
                    figure.GetAvailableMoves(board).Contains(kingPos))
                    return true;
            }
            return false;
        }

        public bool IsSquareUnderAttack(int row, int col, string defenderColor)
        {
            foreach (var figure in Board)
            {
                if (figure != null && figure.Color != defenderColor &&
                    figure.GetAvailableMoves(Board).Contains((row, col)))
                    return true;
            }
            return false;
        }

        private (int, int) FindKing(string color, Figure[,] board)
        {
            for (int row = 0; row < 8; row++)
                for (int col = 0; col < 8; col++)
                    if (board[row, col] is King && board[row, col].Color == color)
                        return (row, col);
            throw new Exception("Король не найден!");
        }

        private void UpdateGameStatus()
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
                for (int col = 0; col < 8; col++)
                    if (Board[row, col] != null && Board[row, col].Color == player &&
                        GetValidMoves(Board[row, col]).Count > 0)
                        return true;
            return false;
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
            var pawn = Board[position.Item1, position.Item2] as Pawn;
            if (pawn == null) return;

            Figure newFigure = newFigureType switch
            {
                "Queen" => new Queen(pawn.Color, position),
                "Rook" => new Rook(pawn.Color, position),
                "Bishop" => new Bishop(pawn.Color, position),
                "Knight" => new Knight(pawn.Color, position),
                _ => throw new ArgumentException("Неизвестный тип фигуры")
            };

            Board[position.Item1, position.Item2] = newFigure;
            UpdateGameStatus();
        }

        private void SaveState()
        {
            var boardCopy = new Figure[8, 8];
            Array.Copy(Board, boardCopy, Board.Length);
            _history.Push((boardCopy, CurrentPlayer, HalfMoveClock, FullMoveNumber));
        }

        public bool UndoMove()
        {
            if (_history.Count <= 1) return false;

            _history.Pop();
            var (prevBoard, prevPlayer, prevHalfMove, prevFullMove) = _history.Peek();
            Board = prevBoard;
            CurrentPlayer = prevPlayer;
            HalfMoveClock = prevHalfMove;
            FullMoveNumber = prevFullMove;
            UpdateGameStatus();
            return true;
        }
    }
}
