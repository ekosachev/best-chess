using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Model;
using Model.Core;
using Newtonsoft.Json.Bson;

namespace Gui
{
    public partial class MainWindow : Form
    {
        private Model.Core.Game GameState { get; set; }

        private Panel[][] BoardCells { get; set; }
        public MainWindow(Model.Core.Game game)
        {
            InitializeComponent();
            GameState = game;
            BoardCells = new Panel[][]
            {
                new Panel[] { A8, B8, C8, D8, E8, F8, G8, H8 },
                new Panel[] { A7, B7, C7, D7, E7, F7, G7, H7 },
                new Panel[] { A6, B6, C6, D6, E6, F6, G6, H6 },
                new Panel[] { A5, B5, C5, D5, E5, F5, G5, H5 },
                new Panel[] { A4, B4, C4, D4, E4, F4, G4, H4 },
                new Panel[] { A3, B3, C3, D3, E3, F3, G3, H3 },
                new Panel[] { A2, B2, C2, D2, E2, F2, G2, H2 },
                new Panel[] { A1, B1, C1, D1, E1, F1, G1, H1 },
            };

            updateBoard();
        }

        private void updateBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    placePiece(row, col, GameState.Board[row, col]);
                }
            }
        }

        private void placePiece(int row, int col, IFigure figure)
        {
            var image = getPieceImage(figure);
            if (image == null) return;
            BoardCells[row][col].BackgroundImage = image;
        }

        private Bitmap getPieceImage(IFigure figure)
        {
            Bitmap image = null;
            if (figure is Pawn _ ) _ = (figure.Color == "White") ? image = Properties.Resources.wp : image = Properties.Resources.bp;
            if (figure is Rook _ ) _ = (figure.Color == "White") ? image = Properties.Resources.wr : image = Properties.Resources.br;
            if (figure is Knight _ ) _ = (figure.Color == "White") ? image = Properties.Resources.wn : image = Properties.Resources.bn;
            if (figure is Bishop _) _ = (figure.Color == "White") ? image = Properties.Resources.wb : image = Properties.Resources.bb;
            if (figure is Queen _) _ = (figure.Color == "White") ? image = Properties.Resources.wq : image = Properties.Resources.bq;
            if (figure is King _ ) _ = (figure.Color == "White") ? image = Properties.Resources.wk : image = Properties.Resources.bk;
     
            return image;
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
     
        }
    }
}
