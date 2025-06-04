using Model.Core;
using Model.Data;
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
            //"JSON ����� (*.json)|*.json|" +
            //"XML ����� (*.xml)|*.xml|" +
            "��� �������������� ����� (*.json, *.xml)|*.json;*.xml";

    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {
        var game = new Game();
        game.ResetGame();
        game.Extension = $".{SaveFormatSelector.SelectedValue}";
        OpenMainWindow(game);
    }

    private void button2_Click(object sender, EventArgs e)
    {
        if (SelectGameSaveFile.ShowDialog() == DialogResult.Cancel)
            return;

        string fileName = SelectGameSaveFile.FileName;
        string ext = Path.GetExtension(fileName).Substring(1);
        GameSerializer serializer;
        if (ext == "json")
        {
            serializer = new JsonGameSerializer();
        }
        else if (ext == "xml")
        {
            serializer = new XmlGameSerializer();
        }
        else
        {
            MessageBox.Show(
                "�������������� ������ json � xml �����",
                "������ ���� ��������� �������",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }
        Game game = null;
        try
        {
            game = serializer.Deserialize(fileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "������ ������ �����", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        if (game != null)
        {

            OpenMainWindow(game);
        }
    }

    private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
    {
        
    }

    private void OpenMainWindow(Game gameInstance)
    {
        Hide();
        var form2 = new MainWindow(gameInstance);
        form2.Closed += (s, args) => Close();
        form2.Show();
    }
}
