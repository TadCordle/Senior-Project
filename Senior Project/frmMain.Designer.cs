namespace Senior_Project
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

			if (disposing)
				aiMoveComplete.Close();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.HUD1 = new System.Windows.Forms.Label();
			this.HUD2 = new System.Windows.Forms.Label();
			this.HUD3 = new System.Windows.Forms.Label();
			this.HUD4 = new System.Windows.Forms.Label();
			this.lblTurn = new System.Windows.Forms.Label();
			this.lblStatus = new System.Windows.Forms.Label();
			this.lblPlayerCount = new System.Windows.Forms.Label();
			this.lblAICount = new System.Windows.Forms.Label();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.mnuGame = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuNewGame = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuLoad = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuCreate = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuHTP = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
			this.ofd = new System.Windows.Forms.OpenFileDialog();
			this.picBoard = new System.Windows.Forms.PictureBox();
			this.loffleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.feefToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cbGrid = new System.Windows.Forms.CheckBox();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuChooseAI = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.picBoard)).BeginInit();
			this.SuspendLayout();
			// 
			// HUD1
			// 
			this.HUD1.AutoSize = true;
			this.HUD1.Location = new System.Drawing.Point(12, 522);
			this.HUD1.Name = "HUD1";
			this.HUD1.Size = new System.Drawing.Size(32, 13);
			this.HUD1.TabIndex = 1;
			this.HUD1.Text = "Turn:";
			// 
			// HUD2
			// 
			this.HUD2.AutoSize = true;
			this.HUD2.Location = new System.Drawing.Point(12, 535);
			this.HUD2.Name = "HUD2";
			this.HUD2.Size = new System.Drawing.Size(53, 13);
			this.HUD2.TabIndex = 1;
			this.HUD2.Text = "AI Status:";
			// 
			// HUD3
			// 
			this.HUD3.AutoSize = true;
			this.HUD3.Location = new System.Drawing.Point(211, 522);
			this.HUD3.Name = "HUD3";
			this.HUD3.Size = new System.Drawing.Size(74, 13);
			this.HUD3.TabIndex = 1;
			this.HUD3.Text = "Player Pieces:";
			// 
			// HUD4
			// 
			this.HUD4.AutoSize = true;
			this.HUD4.Location = new System.Drawing.Point(211, 535);
			this.HUD4.Name = "HUD4";
			this.HUD4.Size = new System.Drawing.Size(55, 13);
			this.HUD4.TabIndex = 1;
			this.HUD4.Text = "AI Pieces:";
			// 
			// lblTurn
			// 
			this.lblTurn.AutoSize = true;
			this.lblTurn.Location = new System.Drawing.Point(71, 522);
			this.lblTurn.Name = "lblTurn";
			this.lblTurn.Size = new System.Drawing.Size(36, 13);
			this.lblTurn.TabIndex = 1;
			this.lblTurn.Text = "Player";
			// 
			// lblStatus
			// 
			this.lblStatus.AutoSize = true;
			this.lblStatus.Location = new System.Drawing.Point(71, 535);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(89, 13);
			this.lblStatus.TabIndex = 1;
			this.lblStatus.Text = "Waiting for player";
			// 
			// lblPlayerCount
			// 
			this.lblPlayerCount.AutoSize = true;
			this.lblPlayerCount.Location = new System.Drawing.Point(292, 522);
			this.lblPlayerCount.Name = "lblPlayerCount";
			this.lblPlayerCount.Size = new System.Drawing.Size(13, 13);
			this.lblPlayerCount.TabIndex = 1;
			this.lblPlayerCount.Text = "0";
			// 
			// lblAICount
			// 
			this.lblAICount.AutoSize = true;
			this.lblAICount.Location = new System.Drawing.Point(292, 535);
			this.lblAICount.Name = "lblAICount";
			this.lblAICount.Size = new System.Drawing.Size(13, 13);
			this.lblAICount.TabIndex = 1;
			this.lblAICount.Text = "0";
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuGame,
            this.mnuHelp});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(504, 24);
			this.menuStrip.TabIndex = 2;
			this.menuStrip.Text = "menuStrip1";
			// 
			// mnuGame
			// 
			this.mnuGame.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuNewGame,
            this.toolStripSeparator1,
            this.mnuChooseAI,
            this.toolStripMenuItem2,
            this.mnuLoad,
            this.mnuCreate,
            this.toolStripMenuItem1,
            this.mnuExit});
			this.mnuGame.Name = "mnuGame";
			this.mnuGame.Size = new System.Drawing.Size(50, 20);
			this.mnuGame.Text = "&Game";
			// 
			// mnuNewGame
			// 
			this.mnuNewGame.Name = "mnuNewGame";
			this.mnuNewGame.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.mnuNewGame.Size = new System.Drawing.Size(184, 22);
			this.mnuNewGame.Text = "&New Game";
			this.mnuNewGame.Click += new System.EventHandler(this.mnuNewGame_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(181, 6);
			// 
			// mnuLoad
			// 
			this.mnuLoad.Name = "mnuLoad";
			this.mnuLoad.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.mnuLoad.Size = new System.Drawing.Size(184, 22);
			this.mnuLoad.Text = "&Open Board";
			this.mnuLoad.Click += new System.EventHandler(this.mnuLoad_Click);
			// 
			// mnuCreate
			// 
			this.mnuCreate.Name = "mnuCreate";
			this.mnuCreate.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.mnuCreate.Size = new System.Drawing.Size(184, 22);
			this.mnuCreate.Text = "&Create Board";
			this.mnuCreate.Click += new System.EventHandler(this.mnuCreate_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(181, 6);
			// 
			// mnuExit
			// 
			this.mnuExit.Name = "mnuExit";
			this.mnuExit.Size = new System.Drawing.Size(184, 22);
			this.mnuExit.Text = "E&xit";
			this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
			// 
			// mnuHelp
			// 
			this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuHTP,
            this.mnuAbout});
			this.mnuHelp.Name = "mnuHelp";
			this.mnuHelp.Size = new System.Drawing.Size(44, 20);
			this.mnuHelp.Text = "&Help";
			// 
			// mnuHTP
			// 
			this.mnuHTP.Name = "mnuHTP";
			this.mnuHTP.ShortcutKeys = System.Windows.Forms.Keys.F1;
			this.mnuHTP.Size = new System.Drawing.Size(157, 22);
			this.mnuHTP.Text = "How to &Play";
			this.mnuHTP.Click += new System.EventHandler(this.mnuHTP_Click);
			// 
			// mnuAbout
			// 
			this.mnuAbout.Name = "mnuAbout";
			this.mnuAbout.Size = new System.Drawing.Size(157, 22);
			this.mnuAbout.Text = "&About";
			this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
			// 
			// picBoard
			// 
			this.picBoard.BackColor = System.Drawing.Color.White;
			this.picBoard.Location = new System.Drawing.Point(12, 27);
			this.picBoard.Name = "picBoard";
			this.picBoard.Size = new System.Drawing.Size(480, 480);
			this.picBoard.TabIndex = 0;
			this.picBoard.TabStop = false;
			this.picBoard.Paint += new System.Windows.Forms.PaintEventHandler(this.picBoard_Paint);
			this.picBoard.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picBoard_MouseClick);
			// 
			// loffleToolStripMenuItem
			// 
			this.loffleToolStripMenuItem.Name = "loffleToolStripMenuItem";
			this.loffleToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
			// 
			// feefToolStripMenuItem
			// 
			this.feefToolStripMenuItem.Name = "feefToolStripMenuItem";
			this.feefToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
			// 
			// cbGrid
			// 
			this.cbGrid.AutoSize = true;
			this.cbGrid.Location = new System.Drawing.Point(384, 521);
			this.cbGrid.Name = "cbGrid";
			this.cbGrid.Size = new System.Drawing.Size(73, 17);
			this.cbGrid.TabIndex = 3;
			this.cbGrid.Text = "Show grid";
			this.cbGrid.UseVisualStyleBackColor = true;
			this.cbGrid.CheckedChanged += new System.EventHandler(this.cbGrid_CheckedChanged);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(181, 6);
			// 
			// mnuChooseAI
			// 
			this.mnuChooseAI.Name = "mnuChooseAI";
			this.mnuChooseAI.Size = new System.Drawing.Size(184, 22);
			this.mnuChooseAI.Text = "Choose &AI";
			this.mnuChooseAI.Click += new System.EventHandler(this.mnuChooseAI_Click);
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(504, 565);
			this.Controls.Add(this.cbGrid);
			this.Controls.Add(this.HUD4);
			this.Controls.Add(this.HUD3);
			this.Controls.Add(this.HUD2);
			this.Controls.Add(this.lblAICount);
			this.Controls.Add(this.lblPlayerCount);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.lblTurn);
			this.Controls.Add(this.HUD1);
			this.Controls.Add(this.picBoard);
			this.Controls.Add(this.menuStrip);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip;
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.Text = "Main";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.picBoard)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picBoard;
        private System.Windows.Forms.Label HUD1;
        private System.Windows.Forms.Label HUD2;
        private System.Windows.Forms.Label HUD3;
        private System.Windows.Forms.Label HUD4;
        private System.Windows.Forms.Label lblTurn;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblPlayerCount;
        private System.Windows.Forms.Label lblAICount;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuGame;
        private System.Windows.Forms.ToolStripMenuItem mnuLoad;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
        private System.Windows.Forms.OpenFileDialog ofd;
        private System.Windows.Forms.ToolStripMenuItem mnuCreate;
        private System.Windows.Forms.ToolStripMenuItem mnuNewGame;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem mnuHTP;
        private System.Windows.Forms.ToolStripMenuItem mnuAbout;
        private System.Windows.Forms.ToolStripMenuItem loffleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem feefToolStripMenuItem;
        private System.Windows.Forms.CheckBox cbGrid;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem mnuChooseAI;

    }
}

