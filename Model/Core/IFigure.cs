using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public interface IFigure
    {
        string Name { get; }
        string Color { get; }
        (int row, int col) Position { get; set; }
        bool HasMoved { get; set; }

        List<(int row, int col)> GetRawMoves(Figure[][] board);
        List<(int row, int col)> GetValidMoves(Figure[][] board);
    }
}
