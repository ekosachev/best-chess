using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public class Queen : Figure
    {
        public Queen(string color, (int, int) position) : base(color, position)
        {
            Name = "Queen";
        }
        public override List<(int, int)> GetRawMoves(Figure[,] board)
        {
            var moves = new List<(int, int)>();

            // Горизонтальные и вертикальные ходы (как ладья)
            int[] directions = { -1, 1 };
            foreach (int dir in directions)
            {
                // По горизонтали
                for (int col = Position.col + dir; IsInsideBoard(Position.row, col); col += dir)
                {
                    if (board[Position.row, col] == null)
                        moves.Add((Position.row, col));
                    else
                    {
                        if (IsEnemy(board, Position.row, col))
                            moves.Add((Position.row, col));
                        break;
                    }
                }

                // По вертикали
                for (int row = Position.row + dir; IsInsideBoard(row, Position.col); row += dir)
                {
                    if (board[row, Position.col] == null)
                        moves.Add((row, Position.col));
                    else
                    {
                        if (IsEnemy(board, row, Position.col))
                            moves.Add((row, Position.col));
                        break;
                    }
                }
            }

            // Диагональные ходы (как слон)
            foreach (int rowDir in new[] { -1, 1 })
            {
                foreach (int colDir in new[] { -1, 1 })
                {
                    for (int i = 1; i < 8; i++)
                    {
                        int newRow = Position.row + i * rowDir;
                        int newCol = Position.col + i * colDir;

                        if (!IsInsideBoard(newRow, newCol)) break;

                        if (board[newRow, newCol] == null)
                            moves.Add((newRow, newCol));
                        else
                        {
                            if (IsEnemy(board, newRow, newCol))
                                moves.Add((newRow, newCol));
                            break;
                        }
                    }
                }
            }
            return moves;
        }
    }
}