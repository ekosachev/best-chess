namespace Gui;

partial class MainMenu
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        NewGame = new Button();
        ResumeGame = new Button();
        SaveFormatSelector = new ComboBox();
        SelectGameSaveFile = new OpenFileDialog();
        flowLayoutPanel1 = new FlowLayoutPanel();
        label1 = new Label();
        flowLayoutPanel1.SuspendLayout();
        SuspendLayout();
        // 
        // NewGame
        // 
        NewGame.AutoSize = true;
        NewGame.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        NewGame.Location = new Point(3, 3);
        NewGame.Name = "NewGame";
        NewGame.Size = new Size(123, 25);
        NewGame.TabIndex = 0;
        NewGame.Text = "Начать новую игру";
        NewGame.UseVisualStyleBackColor = true;
        NewGame.Click += button1_Click;
        // 
        // ResumeGame
        // 
        ResumeGame.AutoSize = true;
        ResumeGame.Location = new Point(3, 34);
        ResumeGame.Name = "ResumeGame";
        ResumeGame.Size = new Size(123, 25);
        ResumeGame.TabIndex = 1;
        ResumeGame.Text = "Продолжить игру";
        ResumeGame.UseVisualStyleBackColor = true;
        ResumeGame.Click += button2_Click;
        // 
        // SaveFormatSelector
        // 
        SaveFormatSelector.FormattingEnabled = true;
        SaveFormatSelector.Location = new Point(3, 80);
        SaveFormatSelector.Name = "SaveFormatSelector";
        SaveFormatSelector.Size = new Size(123, 23);
        SaveFormatSelector.TabIndex = 2;
        // 
        // SelectGameSaveFile
        // 
        SelectGameSaveFile.FileName = "openFileDialog1";
        SelectGameSaveFile.FileOk += openFileDialog1_FileOk;
        // 
        // flowLayoutPanel1
        // 
        flowLayoutPanel1.AutoSize = true;
        flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        flowLayoutPanel1.Controls.Add(NewGame);
        flowLayoutPanel1.Controls.Add(ResumeGame);
        flowLayoutPanel1.Controls.Add(label1);
        flowLayoutPanel1.Controls.Add(SaveFormatSelector);
        flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
        flowLayoutPanel1.Location = new Point(12, 12);
        flowLayoutPanel1.Name = "flowLayoutPanel1";
        flowLayoutPanel1.Size = new Size(129, 106);
        flowLayoutPanel1.TabIndex = 3;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(3, 62);
        label1.Name = "label1";
        label1.Size = new Size(121, 15);
        label1.TabIndex = 3;
        label1.Text = "Формат сохранения:";
        // 
        // MainMenu
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ClientSize = new Size(724, 454);
        Controls.Add(flowLayoutPanel1);
        Name = "MainMenu";
        Text = "best-chess Main Menu";
        Load += Form1_Load;
        flowLayoutPanel1.ResumeLayout(false);
        flowLayoutPanel1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button NewGame;
    private Button ResumeGame;
    private ComboBox SaveFormatSelector;
    private OpenFileDialog SelectGameSaveFile;
    private FlowLayoutPanel flowLayoutPanel1;
    private Label label1;
}
