using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public partial class Game
    {
        private void UpdateDrawByRepetition(string positionKey)
        {
            IsDrawByRepetition = _positionCounts[positionKey] >= 3;
        }
    }
}
