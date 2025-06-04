using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public partial class Game
    {
        private void UpdateCheck()
        {
            IsCheck = IsKingInCheck(CurrentPlayer);
            IsCheckmate = (IsCheck && !PlayerHasValidMoves(CurrentPlayer)) || !Board.Any(r => r.Any(f => f is King _ && f.Color != CurrentPlayer));
            IsStalemate = !IsCheck && !PlayerHasValidMoves(CurrentPlayer);
        }
    }
}
