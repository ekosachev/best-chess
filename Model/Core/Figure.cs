using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
        public abstract class Figure : IFigure
        {
            public string Color { get; }
            public string Name { get; protected set; }
            public (int row, int col) Position { get; set; }
            public bool HasMoved { get; set; }

            protected Figure(string color, (int, int) position)
            {
                Color = color;
                Position = position;
                HasMoved = false;
            }

            public virtual List<(int, int)> GetAvailableMoves(Figure[,] board)
            {
                return GetRawMoves(board)
                    .Where(move => IsMoveSafe(board, move))
                    .ToList();
            }

            public abstract List<(int, int)> GetRawMoves(Figure[,] board);

            protected bool IsMoveSafe(Figure[,] board, (int row, int col) targetPos)
            {
                var tempBoard = (Figure[,])board.Clone();
                tempBoard[targetPos.row, targetPos.col] = this;
                tempBoard[Position.row, Position.col] = null;

                var kingPos = FindKing(Color, tempBoard);

                foreach (var figure in tempBoard)
                {
                    if (figure != null && figure.Color != Color)
                    {
                        if (figure.GetRawMoves(tempBoard).Contains(kingPos))
                            return false;
                    }
                }
                return true;
            }

            protected (int, int) FindKing(string color, Figure[,] board)
            {
                for (int row = 0; row < 8; row++)
                    for (int col = 0; col < 8; col++)
                        if (board[row, col] is King && board[row, col].Color == color)
                            return (row, col);
                throw new Exception("Король не найден!");
            }

            protected bool IsInsideBoard(int row, int col) =>
                row >= 0 && row < 8 && col >= 0 && col < 8;

            protected bool IsEnemy(Figure[,] board, int row, int col) =>
                board[row, col] != null && board[row, col].Color != Color;

            protected bool IsAlly(Figure[,] board, int row, int col) =>
                board[row, col] != null && board[row, col].Color == Color;
        }
}
