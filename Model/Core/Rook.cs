using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{ 
            public class Rook : Figure
            {
                public Rook(string color, (int, int) position) : base(color, position)
                {
                    Name = "Rook";
                }

                public override List<(int, int)> GetRawMoves(Figure[,] board)
                {
                    var moves = new List<(int, int)>();

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

                    return moves;
                }
            }
}
