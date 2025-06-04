using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public class Knight : Figure
    {
        public Knight(string color, (int row, int col) position) : base(color, position)
        {
            Name = "Knight";
        }

        public override List<(int row, int col)> GetRawMoves(Figure[][] board)
        {
            var moves = new List<(int, int)>();
            var (row, col) = Position;

            // Все 8 возможных ходов буквой "Г"
            int[][] jumps = {
            new[] {2, 1}, new[] {2, -1},
            new[] {-2, 1}, new[] {-2, -1},
            new[] {1, 2}, new[] {1, -2},
            new[] {-1, 2}, new[] {-1, -2}
        };

            foreach (var jump in jumps)
            {
                int newRow = row + jump[0];
                int newCol = col + jump[1];

                if (IsInsideBoard(newRow, newCol) &&
                    (board[newRow][newCol] == null || board[newRow][newCol].Color != Color))
                {
                    moves.Add((newRow, newCol));
                }
            }

            return moves;
        }
    }
}
