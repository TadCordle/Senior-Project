namespace Senior_Project
{
    partial class frmCreate
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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCreate));
			this.picBoard = new System.Windows.Forms.PictureBox();
			this.menu = new System.Windows.Forms.MenuStrip();
			this.mnuBoard = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSave = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuDone = new System.Windows.Forms.ToolStripMenuItem();
			this.radGreen = new System.Windows.Forms.RadioButton();
			this.radRed = new System.Windows.Forms.RadioButton();
			this.radGray = new System.Windows.Forms.RadioButton();
			this.grpChoice = new System.Windows.Forms.GroupBox();
			this.ofd = new System.Windows.Forms.OpenFileDialog();
			this.sfd = new System.Windows.Forms.SaveFileDialog();
			this.cbGrid = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.picBoard)).BeginInit();
			this.menu.SuspendLayout();
			this.grpChoice.SuspendLayout();
			this.SuspendLayout();
			// 
			// picBoard
			// 
			this.picBoard.BackColor = System.Drawing.SystemColors.ButtonHighlight;
			this.picBoard.Location = new System.Drawing.Point(60, 84);
			this.picBoard.Name = "picBoard";
			this.picBoard.Size = new System.Drawing.Size(384, 384);
			this.picBoard.TabIndex = 1;
			this.picBoard.TabStop = false;
			this.picBoard.Paint += new System.Windows.Forms.PaintEventHandler(this.picBoard_Paint);
			this.picBoard.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picBoard_MouseClick);
			// 
			// menu
			// 
			this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuBoard});
			this.menu.Location = new System.Drawing.Point(0, 0);
			this.menu.Name = "menu";
			this.menu.Size = new System.Drawing.Size(502, 24);
			this.menu.TabIndex = 2;
			this.menu.Text = "menuStrip1";
			// 
			// mnuBoard
			// 
			this.mnuBoard.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOpen,
            this.mnuSave,
            this.mnuSaveAs,
            this.toolStripMenuItem1,
            this.mnuDone});
			this.mnuBoard.Name = "mnuBoard";
			this.mnuBoard.Size = new System.Drawing.Size(50, 20);
			this.mnuBoard.Text = "Board";
			// 
			// mnuOpen
			// 
			this.mnuOpen.Name = "mnuOpen";
			this.mnuOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.mnuOpen.Size = new System.Drawing.Size(146, 22);
			this.mnuOpen.Text = "&Open";
			this.mnuOpen.Click += new System.EventHandler(this.mnuOpen_Click);
			// 
			// mnuSave
			// 
			this.mnuSave.Name = "mnuSave";
			this.mnuSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.mnuSave.Size = new System.Drawing.Size(146, 22);
			this.mnuSave.Text = "&Save";
			this.mnuSave.Click += new System.EventHandler(this.mnuSave_Click);
			// 
			// mnuSaveAs
			// 
			this.mnuSaveAs.Name = "mnuSaveAs";
			this.mnuSaveAs.Size = new System.Drawing.Size(146, 22);
			this.mnuSaveAs.Text = "Save &As...";
			this.mnuSaveAs.Click += new System.EventHandler(this.mnuSaveAs_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(143, 6);
			// 
			// mnuDone
			// 
			this.mnuDone.Name = "mnuDone";
			this.mnuDone.Size = new System.Drawing.Size(146, 22);
			this.mnuDone.Text = "&Done";
			this.mnuDone.Click += new System.EventHandler(this.mnuDone_Click);
			// 
			// radGreen
			// 
			this.radGreen.AutoSize = true;
			this.radGreen.Location = new System.Drawing.Point(6, 19);
			this.radGreen.Name = "radGreen";
			this.radGreen.Size = new System.Drawing.Size(93, 17);
			this.radGreen.TabIndex = 0;
			this.radGreen.TabStop = true;
			this.radGreen.Tag = "1";
			this.radGreen.Text = "Player 1 Piece";
			this.radGreen.UseVisualStyleBackColor = true;
			this.radGreen.CheckedChanged += new System.EventHandler(this.rad_CheckedChanged);
			// 
			// radRed
			// 
			this.radRed.AutoSize = true;
			this.radRed.Location = new System.Drawing.Point(105, 19);
			this.radRed.Name = "radRed";
			this.radRed.Size = new System.Drawing.Size(93, 17);
			this.radRed.TabIndex = 1;
			this.radRed.TabStop = true;
			this.radRed.Tag = "2";
			this.radRed.Text = "Player 2 Piece";
			this.radRed.UseVisualStyleBackColor = true;
			this.radRed.CheckedChanged += new System.EventHandler(this.rad_CheckedChanged);
			// 
			// radGray
			// 
			this.radGray.AutoSize = true;
			this.radGray.Location = new System.Drawing.Point(204, 19);
			this.radGray.Name = "radGray";
			this.radGray.Size = new System.Drawing.Size(89, 17);
			this.radGray.TabIndex = 2;
			this.radGray.TabStop = true;
			this.radGray.Tag = "3";
			this.radGray.Text = "Neutral Piece";
			this.radGray.UseVisualStyleBackColor = true;
			this.radGray.CheckedChanged += new System.EventHandler(this.rad_CheckedChanged);
			// 
			// grpChoice
			// 
			this.grpChoice.Controls.Add(this.radGray);
			this.grpChoice.Controls.Add(this.radGreen);
			this.grpChoice.Controls.Add(this.radRed);
			this.grpChoice.Location = new System.Drawing.Point(12, 525);
			this.grpChoice.Name = "grpChoice";
			this.grpChoice.Size = new System.Drawing.Size(299, 44);
			this.grpChoice.TabIndex = 4;
			this.grpChoice.TabStop = false;
			this.grpChoice.Text = "Piece Selection";
			// 
			// ofd
			// 
			this.ofd.FileName = "openFileDialog1";
			// 
			// cbGrid
			// 
			this.cbGrid.AutoSize = true;
			this.cbGrid.Location = new System.Drawing.Point(347, 544);
			this.cbGrid.Name = "cbGrid";
			this.cbGrid.Size = new System.Drawing.Size(73, 17);
			this.cbGrid.TabIndex = 5;
			this.cbGrid.Text = "Show grid";
			this.cbGrid.UseVisualStyleBackColor = true;
			this.cbGrid.CheckedChanged += new System.EventHandler(this.cbGrid_CheckedChanged);
			// 
			// frmCreate
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(502, 582);
			this.Controls.Add(this.cbGrid);
			this.Controls.Add(this.grpChoice);
			this.Controls.Add(this.picBoard);
			this.Controls.Add(this.menu);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menu;
			this.MaximizeBox = false;
			this.Name = "frmCreate";
			this.Text = "Create Board";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCreate_FormClosing);
			this.Load += new System.EventHandler(this.frmCreate_Load);
			((System.ComponentModel.ISupportInitialize)(this.picBoard)).EndInit();
			this.menu.ResumeLayout(false);
			this.menu.PerformLayout();
			this.grpChoice.ResumeLayout(false);
			this.grpChoice.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picBoard;
        private System.Windows.Forms.MenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem mnuBoard;
        private System.Windows.Forms.ToolStripMenuItem mnuOpen;
        private System.Windows.Forms.ToolStripMenuItem mnuSave;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuDone;
        private System.Windows.Forms.RadioButton radGreen;
        private System.Windows.Forms.RadioButton radRed;
        private System.Windows.Forms.RadioButton radGray;
        private System.Windows.Forms.GroupBox grpChoice;
        private System.Windows.Forms.ToolStripMenuItem mnuSaveAs;
        private System.Windows.Forms.OpenFileDialog ofd;
        private System.Windows.Forms.SaveFileDialog sfd;
        private System.Windows.Forms.CheckBox cbGrid;
    }
}