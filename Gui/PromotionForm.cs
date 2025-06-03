using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gui
{
    public partial class PromotionForm : Form
    {
        public string SelectedFigure { get; } = "Queen"; // Всегда выбираем ферзя

        public PromotionForm(string color)
        {
            // Заглушка - не требует реализации
        }

        public DialogResult ShowDialog()
        {
            // Автоматически "выбираем" ферзя без показа формы
            // А так еще можно выбирать ладью, коня или слона помимо ферзя
            return DialogResult.OK;
        }
    }
}
