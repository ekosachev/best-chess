using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
        public class Bishop : Figure
        {
            public Bishop(string color, (int, int) position) : base(color, position)
            {
                Name = "Bishop";
            }

            public override List<(int, int)> GetRawMoves(Figure[,] board)
            {
                var moves = new List<(int, int)>();

                int[] directions = { -1, 1 };
                foreach (int rowDir in directions)
                {
                    foreach (int colDir in directions)
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
