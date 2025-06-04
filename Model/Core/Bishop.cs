using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public class Bishop : Figure
    {
        public Bishop(string color, (int row, int col) position) : base(color, position)
        {
            Name = "Bishop";
        }

        public override List<(int row, int col)> GetRawMoves(Figure[][] board)
        {
            var moves = new List<(int, int)>();
            var (row, col) = Position;

            // По диагоналям
            int[][] directions = { new[] { 1, 1 }, new[] { 1, -1 }, new[] { -1, 1 }, new[] { -1, -1 } };

            foreach (var dir in directions)
            {
                int steps = 1;
                while (true)
                {
                    int newRow = row + dir[0] * steps;
                    int newCol = col + dir[1] * steps;

                    if (!IsInsideBoard(newRow, newCol)) break;

                    if (board[newRow][newCol] == null)
                    {
                        moves.Add((newRow, newCol));
                    }
                    else
                    {
                        if (board[newRow][newCol].Color != Color)
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
