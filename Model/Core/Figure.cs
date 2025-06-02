using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public abstract class Figure : IFigure
    {
        public string Name { get; protected set; }
        public string Color { get; }
        public (int row, int col) Position { get; set; }
        public bool HasMoved { get; set; }

        protected Figure(string color, (int row, int col) position)
        {
            Color = color;
            Position = position;
        }

        public abstract List<(int row, int col)> GetRawMoves(Figure[,] board);

        public virtual List<(int row, int col)> GetValidMoves(Figure[,] board)
        {
            return Game.Instance.GetValidMoves(this);
        }

        protected bool IsInsideBoard(int row, int col)
        {
            return row >= 0 && row < 8 && col >= 0 && col < 8;
        }

        protected bool IsEnemy(Figure[,] board, int row, int col)
        {
            return board[row, col] != null && board[row, col].Color != Color;
        }
    }
}
