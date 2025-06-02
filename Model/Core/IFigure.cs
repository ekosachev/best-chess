using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Core
{
    public interface IFigure
    {
        string Color { get; }
        string Name { get; }
        (int row, int col) Position { get; set; }
        bool HasMoved { get; set; }

        List<(int row, int col)> GetAvailableMoves(Figure[,] board);
        List<(int row, int col)> GetRawMoves(Figure[,] board); // Добавляем в интерфейс
    }
}
