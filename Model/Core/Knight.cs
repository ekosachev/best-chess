using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public class Knight : Figure //конь
    {
        public Knight(string color, (int, int) position) : base(color, position)
        {
            Name = "Knight";
        }

        public override List<(int, int)> GetAvailableMoves(Figure[,] board)
        {
            var moves = new List<(int, int)>();
            int[] rowOffsets = { 2, 1, -1, -2, -2, -1, 1, 2 };
            int[] colOffsets = { 1, 2, 2, 1, -1, -2, -2, -1 };

            for (int i = 0; i < 8; i++)
            {
                int newRow = Position.row + rowOffsets[i];
                int newCol = Position.col + colOffsets[i];

                if (IsInsideBoard(newRow, newCol) && !IsAlly(board, newRow, newCol))
                    moves.Add((newRow, newCol));
            }

            return moves;
        }
    }
}
