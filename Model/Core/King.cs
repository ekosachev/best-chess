
using Model.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public class King : Figure
    {
        public King(string color, (int row, int col) position) : base(color, position)
        {
            Name = "King";
        }

        public override List<(int row, int col)> GetRawMoves(Figure[][] board)
        {
            var moves = new List<(int, int)>();
            var (row, col) = Position;

            // Все 8 возможных направлений
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int newRow = row + i;
                    int newCol = col + j;

                    if (IsInsideBoard(newRow, newCol) &&
                        (board[newRow][newCol] == null || board[newRow][newCol].Color != Color))
                    {
                        moves.Add((newRow, newCol));
                    }
                }
            }

            // Рокировка (только если не было ходов)
            if (!HasMoved)
            {
                // Короткая рокировка
                if (board[row][7] is Rook rook1 && !rook1.HasMoved)
                {
                    if (board[row][5] == null && board[row][6] == null)
                        moves.Add((row, 6));
                }
                // Длинная рокировка
                if (board[row][0] is Rook rook2 && !rook2.HasMoved)
                {
                    if (board[row][1] == null && board[row][2] == null && board[row][3] == null)
                        moves.Add((row, 2));
                }
            }

            return moves;
        }
    }
}
