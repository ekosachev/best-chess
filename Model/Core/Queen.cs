using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public class Queen : Figure //ферзь
    {
        public Queen(string color, (int, int) position) : base(color, position)
        {
            Name = "Queen";
        }

        public override List<(int, int)> GetAvailableMoves(Figure[,] board)
        {
            var moves = new List<(int, int)>();

            // Комбинация ходов ладьи и слона
            var rookMoves = new Rook(Color, Position).GetAvailableMoves(board);
            var bishopMoves = new Bishop(Color, Position).GetAvailableMoves(board);

            moves.AddRange(rookMoves);
            moves.AddRange(bishopMoves);

            return moves;
        }
    }
}
