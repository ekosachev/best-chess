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
        SuspendLayout();
        // 
        // NewGame
        // 
        NewGame.Location = new Point(252, 176);
        NewGame.Name = "NewGame";
        NewGame.Size = new Size(131, 23);
        NewGame.TabIndex = 0;
        NewGame.Text = "Начать новую игру";
        NewGame.UseVisualStyleBackColor = true;
        NewGame.Click += button1_Click;
        // 
        // ResumeGame
        // 
        ResumeGame.Location = new Point(320, 205);
        ResumeGame.Name = "ResumeGame";
        ResumeGame.Size = new Size(131, 23);
        ResumeGame.TabIndex = 1;
        ResumeGame.Text = "Продолжить игру";
        ResumeGame.UseVisualStyleBackColor = true;
        ResumeGame.Click += button2_Click;
        // 
        // SaveFormatSelector
        // 
        SaveFormatSelector.FormattingEnabled = true;
        SaveFormatSelector.Location = new Point(389, 176);
        SaveFormatSelector.Name = "SaveFormatSelector";
        SaveFormatSelector.Size = new Size(121, 23);
        SaveFormatSelector.TabIndex = 2;
        // 
        // SelectGameSaveFile
        // 
        SelectGameSaveFile.FileName = "openFileDialog1";
        SelectGameSaveFile.FileOk += openFileDialog1_FileOk;
        // 
        // MainMenu
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(SaveFormatSelector);
        Controls.Add(ResumeGame);
        Controls.Add(NewGame);
        Name = "MainMenu";
        Text = "best-chess Main Menu";
        Load += Form1_Load;
        ResumeLayout(false);
    }

    #endregion

    private Button NewGame;
    private Button ResumeGame;
    private ComboBox SaveFormatSelector;
    private OpenFileDialog SelectGameSaveFile;
}
