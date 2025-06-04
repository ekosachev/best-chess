using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public class Queen : Figure
    {
        public Queen(string color, (int row, int col) position) : base(color, position)
        {
            Name = "Queen";
        }

        public override List<(int row, int col)> GetRawMoves(Figure[][] board)
        {
            var moves = new List<(int, int)>();
            var (row, col) = Position;

            // Комбинация ладьи и слона
            int[] directions = { -1, 0, 1 };

            foreach (int i in directions)
            {
                foreach (int j in directions)
                {
                    if (i == 0 && j == 0) continue;

                    int steps = 1;
                    while (true)
                    {
                        int newRow = row + i * steps;
                        int newCol = col + j * steps;

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
            }

            return moves;
        }
    }
}