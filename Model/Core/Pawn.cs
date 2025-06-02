using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core 
{
        public class Pawn : Figure
        {
            public Pawn(string color, (int, int) position) : base(color, position)
            {
                Name = "Pawn";
            }

            public override List<(int, int)> GetRawMoves(Figure[,] board)
            {
                var moves = new List<(int, int)>();
                int direction = Color == "White" ? -1 : 1;

                // Ход вперед
                if (IsInsideBoard(Position.row + direction, Position.col) &&
                    board[Position.row + direction, Position.col] == null)
                {
                    moves.Add((Position.row + direction, Position.col));

                    // Двойной ход
                    if (!HasMoved && IsInsideBoard(Position.row + 2 * direction, Position.col) &&
                        board[Position.row + 2 * direction, Position.col] == null)
                    {
                        moves.Add((Position.row + 2 * direction, Position.col));
                    }
                }

                // Взятия
                int[] captureCols = { Position.col - 1, Position.col + 1 };
                foreach (int col in captureCols)
                {
                    if (IsInsideBoard(Position.row + direction, col))
                    {
                        // Обычное взятие
                        if (IsEnemy(board, Position.row + direction, col))
                            moves.Add((Position.row + direction, col));

                        // Взятие на проходе
                        else if (IsInsideBoard(Position.row, col) &&
                                 board[Position.row, col] is Pawn enemyPawn &&
                                 enemyPawn.Color != Color &&
                                 enemyPawn == Game.Instance.LastPawn &&
                                 Game.Instance.LastMoveWasDoublePawnPush)
                        {
                            moves.Add((Position.row + direction, col));
                        }
                    }
                }

                return moves;
            }
        }
}
