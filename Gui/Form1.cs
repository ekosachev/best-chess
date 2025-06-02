using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace Gui;

public partial class MainMenu : Form
{
    struct Format
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public MainMenu()
    {
        InitializeComponent();

        List<Format> formats = new List<Format>
        {
            new Format { Name = "JSON", Value = "json" },
            new Format { Name = "XML", Value = "xml" },
        };

        SaveFormatSelector.DataSource = formats;
        SaveFormatSelector.DisplayMember = "Name";
        SaveFormatSelector.ValueMember = "Value";
        SaveFormatSelector.SelectedIndex = 0;

        SelectGameSaveFile.Filter =
            //"JSON файлы (*.json)|*.json|" +
            //"XML файлы (*.xml)|*.xml|" +
            "Все поддерживаемые файлы (*.json, *.xml)|*.json;*.xml";

    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {
        OpenMainWindow(new Model.Core.Game());
    }

    private void button2_Click(object sender, EventArgs e)
    {
        if (SelectGameSaveFile.ShowDialog() == DialogResult.Cancel)
            return;

        string fileName = SelectGameSaveFile.FileName;
    }

    private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
    {

    }

    private void OpenMainWindow(Model.Core.Game gameInstance)
    {
        this.Hide();
        var form2 = new MainWindow(gameInstance);
        form2.Closed += (s, args) => this.Close();
        form2.Show();
    }
}
