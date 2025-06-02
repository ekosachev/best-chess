using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public class Rook : Figure
    {
        public Rook(string color, (int row, int col) position) : base(color, position)
        {
            Name = "Rook";
        }

        public override List<(int row, int col)> GetRawMoves(Figure[,] board)
        {
            var moves = new List<(int, int)>();
            var (row, col) = Position;

            // По вертикали и горизонтали
            int[][] directions = { new[] { 1, 0 }, new[] { -1, 0 }, new[] { 0, 1 }, new[] { 0, -1 } };

            foreach (var dir in directions)
            {
                int steps = 1;
                while (true)
                {
                    int newRow = row + dir[0] * steps;
                    int newCol = col + dir[1] * steps;

                    if (!IsInsideBoard(newRow, newCol)) break;

                    if (board[newRow, newCol] == null)
                    {
                        moves.Add((newRow, newCol));
                    }
                    else
                    {
                        if (board[newRow, newCol].Color != Color)
                            moves.Add((newRow, newCol));
                        break;
                    }
                    steps++;
                }
            }

            return moves;
        }
    }
}
