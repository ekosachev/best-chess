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

        public abstract List<(int, int)> GetAvailableMoves(Figure[,] board);

        protected bool IsInsideBoard(int row, int col) => row >= 0 && row < 8 && col >= 0 && col < 8;

        protected bool IsEnemy(Figure[,] board, int row, int col) =>
            board[row, col] != null && board[row, col].Color != Color;

        protected bool IsAlly(Figure[,] board, int row, int col) =>
            board[row, col] != null && board[row, col].Color == Color;
    }
}
