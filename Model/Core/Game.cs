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

        public static Game Instance => _instance ??= new Game();
        public Figure[,] Board { get; private set; } = new Figure[8, 8];
        public string CurrentPlayer { get; private set; } = "White";
        public Pawn LastPawn { get; private set; }
        public bool LastMoveWasDoublePawnPush { get; private set; }
        public bool IsCheck { get; private set; }
        public bool IsCheckmate { get; private set; }
        public bool IsStalemate { get; private set; }
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
        }
        public void UndoLastMove()
        {
            if (_boardHistory.Count > 1)
            {
                _boardHistory.Pop(); // Удаляем текущее состояние
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

            // Проверяем, что ход допустим
            if (!GetValidMoves(figure).Contains(to))
                return false;

            // Сохраняем состояние до хода
            SaveGameState();

            // Обработка специальных ходов
            HandleSpecialMoves(figure, from, to);

            // Выполняем перемещение
            Board[to.row, to.col] = figure;
            Board[from.row, from.col] = null;
            figure.Position = to;
            figure.HasMoved = true;

            // Обработка превращения пешки
            HandlePawnPromotion(figure, to);

            // Смена игрока и обновление статуса
            CurrentPlayer = CurrentPlayer == "White" ? "Black" : "White";
            UpdateGameStatus();

            return true;
        }

        private void HandleSpecialMoves(Figure figure, (int row, int col) from, (int row, int col) to)
        {
            // Взятие на проходе
            if (figure is Pawn && from.col != to.col && Board[to.row, to.col] == null)
            {
                Board[from.row, to.col] = null; // Удаляем взятую пешку
            }

            // Рокировка
            if (figure is King && Math.Abs(from.col - to.col) == 2)
            {
                HandleCastling(from, to);
            }

            // Обновление состояния для взятия на проходе
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
                // Создаем форму для выбора фигуры
                /*var promotionForm = new PromotionForm(figure.Color);
                promotionForm.ShowDialog();

                // Получаем выбранную фигуру
                Figure promotedFigure = promotionForm.SelectedFigure switch
                {
                    "Queen" => new Queen(figure.Color, to),
                    "Rook" => new Rook(figure.Color, to),
                    "Bishop" => new Bishop(figure.Color, to),
                    "Knight" => new Knight(figure.Color, to),
                    _ => new Queen(figure.Color, to) // По умолчанию - ферзь
                };

                // Заменяем пешку на выбранную фигуру
                Board[to.row, to.col] = promotedFigure;*/

                // Временная заглушка - всегда превращаем в ферзя
                Board[to.row, to.col] = new Queen(figure.Color, to);

                // Логируем превращение
                //Debug.WriteLine($"Пешка превращена в {promotionForm.SelectedFigure} на {to}");
            }
        }

        public void UpdateGameStatus()
        {
            IsCheck = IsKingInCheck(CurrentPlayer);
            IsCheckmate = IsCheck && !HasAnyValidMoves();
            IsStalemate = !IsCheck && !HasAnyValidMoves();
        }

        private bool IsKingInCheck(string playerColor)
        {
            var kingPos = FindKing(playerColor);
            return IsSquareUnderAttack(kingPos, playerColor == "White" ? "Black" : "White");
        }
        private bool HasAnyValidMoves()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var figure = Board[i, j];
                    if (figure != null && figure.Color == CurrentPlayer)
                    {
                        if (GetValidMoves(figure).Count > 0)
                            return true;
                    }
                }
            }
            return false;
        }

        public List<(int row, int col)> GetValidMoves(Figure figure)
        {
            var rawMoves = figure.GetRawMoves(Board);
            var validMoves = new List<(int, int)>();

            foreach (var move in rawMoves)
            {
                if (IsMoveValid(figure, move))
                    validMoves.Add(move);
            }

            return validMoves;
        }

        private bool IsMoveValid(Figure figure, (int row, int col) move)
        {
            // Базовые проверки
            if (!IsInsideBoard(move.row, move.col)) return false;
            if (Board[move.row, move.col]?.Color == figure.Color) return false;

            // Специальные проверки для каждого типа фигур
            if (figure is Pawn)
                return IsPawnMoveValid((Pawn)figure, move);

            if (figure is King)
                return IsKingMoveValid((King)figure, move);

            // Для остальных фигур - проверка, не откроют ли короля
            return !WouldMoveExposeKing(figure, move);
        }

        private bool IsPawnMoveValid(Pawn pawn, (int row, int col) move)
        {
            int direction = pawn.Color == "White" ? -1 : 1;
            int colDiff = Math.Abs(move.col - pawn.Position.col);

            // Обычный ход вперед
            if (colDiff == 0)
            {
                if (Board[move.row, move.col] != null) return false;

                // Двойной ход
                if (Math.Abs(move.row - pawn.Position.row) == 2)
                {
                    int middleRow = (move.row + pawn.Position.row) / 2;
                    return !pawn.HasMoved && Board[middleRow, move.col] == null;
                }
                return true;
            }
            // Взятие
            else if (colDiff == 1)
            {
                // Обычное взятие
                if (Board[move.row, move.col] != null)
                    return true;

                // Взятие на проходе
                return move == EnPassantTarget;
            }

            return false;
        }

        private bool IsKingMoveValid(King king, (int row, int col) move)
        {
            // Рокировка
            if (Math.Abs(move.col - king.Position.col) == 2)
                return IsCastlingValid(king, move);

            // Обычный ход короля
            return !WouldPositionBeUnderAttack(move, king.Color);
        }

        private bool IsCastlingValid(King king, (int row, int col) move)
        {
            if (king.HasMoved || IsKingInCheck(king.Color))
                return false;

            int rookCol = move.col > king.Position.col ? 7 : 0;
            var rook = Board[king.Position.row, rookCol] as Rook;

            if (rook == null || rook.HasMoved)
                return false;

            // Проверка, что путь свободен
            int start = Math.Min(king.Position.col, rookCol) + 1;
            int end = Math.Max(king.Position.col, rookCol);

            for (int col = start; col < end; col++)
            {
                if (Board[king.Position.row, col] != null)
                    return false;

                if (col != king.Position.col && col != rookCol &&
                    WouldPositionBeUnderAttack((king.Position.row, col), king.Color))
                    return false;
            }

            return true;
        }

        public bool WouldPositionBeUnderAttack((int row, int col) position, string playerColor)
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
                        // Для пешек особый случай - они бьют не так, как ходят
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

        private bool WouldMoveExposeKing(Figure figure, (int row, int col) move)
        {
            // Сохраняем текущее состояние
            var originalPosition = figure.Position;
            var originalFigure = Board[move.row, move.col];

            // Делаем временный ход
            Board[move.row, move.col] = figure;
            Board[originalPosition.row, originalPosition.col] = null;
            figure.Position = move;

            // Проверяем шах
            var kingPos = FindKing(figure.Color);
            var isCheck = IsSquareUnderAttack(kingPos, figure.Color == "White" ? "Black" : "White");

            // Возвращаем всё как было
            Board[originalPosition.row, originalPosition.col] = figure;
            Board[move.row, move.col] = originalFigure;
            figure.Position = originalPosition;

            return isCheck;
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

        public void ResetGame()
        {
            Board = new Figure[8, 8];
            CurrentPlayer = "White";
            InitializeBoard();
            _boardHistory.Clear();
            _playerHistory.Clear();
            SaveGameState();
            UpdateGameStatus();
        }
    }
}

