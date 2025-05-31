using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public class King : Figure //король
    {
        public King(string color, (int, int) position) : base(color, position)
        {
            Name = "King";
        }

        public override List<(int, int)> GetAvailableMoves(Figure[,] board)
        {
            var moves = new List<(int, int)>();
            int[] rowOffsets = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] colOffsets = { -1, 0, 1, -1, 1, -1, 0, 1 };

            // Обычные ходы
            for (int i = 0; i < 8; i++)
            {
                int newRow = Position.row + rowOffsets[i];
                int newCol = Position.col + colOffsets[i];

                if (IsInsideBoard(newRow, newCol) && !IsAlly(board, newRow, newCol))
                    moves.Add((newRow, newCol));
            }

            // Рокировка
            if (!HasMoved && !Game.Instance.IsKingInCheck(Color, board))
            {
                // Короткая (вправо)
                if (CanCastle(board, 7))
                    moves.Add((Position.row, 6));

                // Длинная (влево)
                if (CanCastle(board, 0))
                    moves.Add((Position.row, 2));
            }

            return moves;
        }

        private bool CanCastle(Figure[,] board, int rookCol)
        {
            var rook = board[Position.row, rookCol] as Rook;
            if (rook == null || rook.HasMoved) return false;

            int start = Math.Min(Position.col, rookCol) + 1;
            int end = Math.Max(Position.col, rookCol);
            for (int col = start; col < end; col++)
                if (board[Position.row, col] != null) return false;

            for (int col = Position.col; col != rookCol; col += Math.Sign(rookCol - Position.col))
                if (Game.Instance.IsSquareUnderAttack(Position.row, col, Color))
                    return false;

            return true;
        }
    }
}
