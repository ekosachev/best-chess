﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gui
{
    public partial class Dialogue : Form
    {
        public Dialogue(string title, string text)
        {
            InitializeComponent();
            label1.Text = text;
            this.Text = title;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
