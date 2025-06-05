using Model.Core;
using Model.Data;


namespace Gui
{
    public partial class MainWindow : Form
    {
        private static Color WHITE_CELL = Color.AntiqueWhite;
        private static Color BLACK_CELL = Color.SandyBrown;
        private static Color WHITE_CELL_SELECTED = Color.LawnGreen;
        private static Color BLACK_CELL_SELECTED = Color.DarkGreen;
        private static Color WHITE_CELL_HIGHLIGHTED = Color.DeepSkyBlue;
        private static Color BLACK_CELL_HIGHLIGHTED = Color.MediumBlue;
        private static Color WHITE_CELL_ATTACK = Color.DeepPink;
        private static Color BLACK_CELL_ATTACK = Color.MediumVioletRed;

        private struct Position
        {
            public int row { get; set; }
            public int col { get; set; }
        }

        private Model.Core.Game GameState { get; set; }
        private Position? SelectedCell { get; set; }
        private List<Position> HighlightedCells { get; set; } = new List<Position>();
        private bool ControlsEnabled { get; set; } = true;

        private Panel[][] BoardCells { get; set; }
        public MainWindow(Game game, string? saveFormat = null)
        {
            InitializeComponent();

            saveFileSelector.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


            GameState = game;
            BoardCells =
            [
                [A8, B8, C8, D8, E8, F8, G8, H8],
                [A7, B7, C7, D7, E7, F7, G7, H7],
                [A6, B6, C6, D6, E6, F6, G6, H6],
                [A5, B5, C5, D5, E5, F5, G5, H5],
                [A4, B4, C4, D4, E4, F4, G4, H4],
                [A3, B3, C3, D3, E3, F3, G3, H3],
                [A2, B2, C2, D2, E2, F2, G2, H2],
                [A1, B1, C1, D1, E1, F1, G1, H1],
            ];

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++) BoardCells[row][col].Click += Cell_Click;
            }

            FormClosing += MainWindow_FormClosing;
            UpdatePathLabel();
            setNewSavePath.Click += SetNewSavePath_Click;
            saveButton.Click += SaveButton_Click;
            currentTurnLabel.Text = GameState.CurrentPlayer == "White" ? "Белые" : "Черные";

            updateBoard();

            if (saveFormat != GameState.Extension.Substring(1) && saveFormat != null)
            {
                GameState.Extension = $".{saveFormat}";
                GameState.FilePath = Path.ChangeExtension(GameState.FilePath, GameState.Extension);
                UpdatePathLabel();
                MessageBox.Show($"Формат изменен на {saveFormat}", "Сохранение и загрузка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SaveGame();
            }

            if (string.IsNullOrEmpty(GameState.FilePath) || string.IsNullOrEmpty(GameState.Extension))
            {
                saveButton.Enabled = false;
            }
        }

        private void UpdatePathLabel()
        {
            pathToSave.Text = string.IsNullOrEmpty(GameState.FilePath) ? "N/A" : GameState.FilePath;
        }

        private void SetNewSavePath_Click(object? sender, EventArgs e)
        {
            ChooseSaveFile();
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            SaveGame();
        }

        private void MainWindow_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(GameState.FilePath))
            {
                if (!ChooseSaveFile()) e.Cancel = true;
            }
            else
            {
                SaveGame();
                
            }
        }

        private void UpdateCurrentPlayer()
        {
            currentTurnLabel.Text = GameState.CurrentPlayer == "White" ? "Белые" : "Черные";
        }

        private void updateBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    placePiece(row, col, GameState.Board[row][col]);
                }
            }
        }

        private void placePiece(int row, int col, IFigure figure)
        {
            var image = getPieceImage(figure);
            BoardCells[row][col].BackgroundImage = image;
        }

        private Bitmap getPieceImage(IFigure figure)
        {
            Bitmap image = null;
            if (figure is Pawn _) _ = (figure.Color == "White") ? image = Properties.Resources.wp : image = Properties.Resources.bp;
            if (figure is Rook _) _ = (figure.Color == "White") ? image = Properties.Resources.wr : image = Properties.Resources.br;
            if (figure is Knight _) _ = (figure.Color == "White") ? image = Properties.Resources.wn : image = Properties.Resources.bn;
            if (figure is Bishop _) _ = (figure.Color == "White") ? image = Properties.Resources.wb : image = Properties.Resources.bb;
            if (figure is Queen _) _ = (figure.Color == "White") ? image = Properties.Resources.wq : image = Properties.Resources.bq;
            if (figure is King _) _ = (figure.Color == "White") ? image = Properties.Resources.wk : image = Properties.Resources.bk;

            return image;
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            if (GameState.IsCheckmate || GameState.IsStalemate)
            {
                return;
            }
            Panel panel;

            if (sender is Panel p)
            {
                panel = (Panel)sender;
            }
            else
            {
                return;
            }
            Console.WriteLine("Clicked a panel");

            string cellName = panel.Name;
            int row = 8 - int.Parse(cellName.Substring(1));
            int col = (int)cellName[0] - 65;

            if (SelectedCell != null)
            {
                if (SelectedCell.Value.row == row && SelectedCell.Value.col == col)
                {
                    DeselectCell();
                    UnhighlightAllCells();
                    return;
                }
                Console.WriteLine("Making a move");
                (int, int) from = (SelectedCell.Value.row, SelectedCell.Value.col);
                (int, int) to = (row, col);
                if (HighlightedCells.Contains(new Position { row = row, col = col }))
                {
                    GameState.MakeMove(from, to);

                    updateBoard();
                    DeselectCell();
                    UnhighlightAllCells();
                    if (GameState.IsCheck)
                    {
                        MessageBox.Show($"{currentTurnLabel.Text} поставили шах", "Шах", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    if (GameState.IsCheckmate)
                    {
                        MessageBox.Show($"{currentTurnLabel.Text} поставили шах и мат\nПобеда {(currentTurnLabel.Text == "Белые" ? "Белых" : "Черных" )}", "Шах и мат", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    }
                    if (GameState.IsStalemate)
                    {
                        MessageBox.Show($"Партия завершилась патом", "Пат", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    if (GameState.IsDrawByRepetition)
                    {
                        MessageBox.Show($"Партия завершилась ничьей после троекратного повторения ходов", "Ничья", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    UpdateCurrentPlayer();
                }
            }

            Figure? figure = GameState.Board[row][col];
            if (figure == null) return;

            if (figure.Color == GameState.CurrentPlayer)
            {

                SelectCell(row, col);
                UnhighlightAllCells();
                foreach (var pos in GameState.GetValidMoves(figure))
                {
                    HighlightCell(pos.Item1, pos.Item2);
                }
            }
        }

        private void SelectCell(int row, int col)
        {
            DeselectCell();
            SelectedCell = new Position { row = row, col = col };
            BoardCells[row][col].BackColor = (row + col) % 2 == 0 ? WHITE_CELL_SELECTED : BLACK_CELL_SELECTED;
        }

        private void DeselectCell()
        {
            if (SelectedCell == null) return;
            var cell = SelectedCell.Value;
            BoardCells[cell.row][cell.col].BackColor = (cell.row + cell.col) % 2 == 0 ? WHITE_CELL : BLACK_CELL;
            SelectedCell = null;
        }

        private void HighlightCell(int row, int col)
        {
            BoardCells[row][col].BackColor = (row + col) % 2 == 0 ? WHITE_CELL_HIGHLIGHTED : BLACK_CELL_HIGHLIGHTED;
            HighlightedCells.Add(new Position { row = row, col = col });
        }

        private void UnhighlightAllCells()
        {
            foreach (var pos in HighlightedCells)
            {
                BoardCells[pos.row][pos.col].BackColor = (pos.row + pos.col) % 2 == 0 ? WHITE_CELL : BLACK_CELL;
            }

            HighlightedCells.Clear();
        }

        private bool ChooseSaveFile()
        {
            saveFileSelector.DefaultExt = GameState.Extension;
            var dt = DateTime.Now;
            saveFileSelector.FileName = $"game-{dt.Year}-{dt.Month}-{dt.Day}T{dt.Hour}{dt.Minute}{dt.Second}";
            var result = saveFileSelector.ShowDialog();
            if (result != DialogResult.OK) return false;
            GameState.FilePath = saveFileSelector.FileName;
            pathToSave.Text = GameState.FilePath;

            saveButton.Enabled = true;
            SaveGame();
            MessageBox.Show("Игра сохранена в новый файл", "Сохранение и загрузка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        private void SaveGame()
        {
            string filePath = GameState.FilePath;
            if (string.IsNullOrEmpty(filePath)) return;
            if (string.IsNullOrEmpty(GameState.Extension)) return;
            string ext = Path.GetExtension(filePath);
            GameSerializer serializer;
            if (GameState.Extension == ".json")
            {
                serializer = new JsonGameSerializer();
                serializer.Serialize(GameState, filePath);
                
            }
            else
            {
                serializer = new XmlGameSerializer();
                serializer.Serialize(GameState, filePath);

            }
            MessageBox.Show("Игра сохранена", "Сохранение и загрузка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void currentTurnLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
