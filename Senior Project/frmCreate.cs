using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Senior_Project
{
    public partial class frmCreate : Form
    {
        #region Variables

        Board board; // The canvas
        int createCode; // Determines which piece to create when you left click
        string file, prevFile; // Current file you're editing
        bool saved = true; // Controls the "save changes" dialog
        bool showGrid; // Whether or not the grid is drawn

        #endregion

        // Set up variables
        public frmCreate()
        {
            InitializeComponent();
            file = "";
            prevFile = file;
            saved = true;
            showGrid = false;
        }

        // Create a blank board
        private void frmCreate_Load(object sender, EventArgs e)
        {
            createCode = 0;
            board = new Board(10, 10);
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                    board[i, j] = new GamePiece(i, j, 0);
            file = "";
        }

        // Draw the game board
        private void picBoard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            board.Draw(g);

            if (showGrid)
            {
                Pen p = new Pen(Color.Gray);
                for (int i = 1; i < 10; i++)
                {
                    g.DrawLine(p, i * 48, 0, i * 48, picBoard.Height);
                    g.DrawLine(p, 0, i * 48, picBoard.Width, i * 48);
                }
            }
        }
        
        // Determine whether or not the draw the grid
        private void cbGrid_CheckedChanged(object sender, EventArgs e)
        {
            showGrid = !showGrid;
            picBoard.Invalidate();
        }

        // Choose which piece to create on the form
        private void rad_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton button = (RadioButton)sender;
            createCode = int.Parse(button.Tag.ToString());
        }

        // Create/delete pieces if the board is left/right clicked
        private void picBoard_MouseClick(object sender, MouseEventArgs e)
        {
            int mousex = (int)(picBoard.PointToClient(Cursor.Position).X);
            int mousey = (int)(picBoard.PointToClient(Cursor.Position).Y);

            if (e.Button == MouseButtons.Left)
                board[Board.CoordToIndex(mousex), Board.CoordToIndex(mousey)].code = createCode;
            else if (e.Button == MouseButtons.Right)
                board[Board.CoordToIndex(mousex), Board.CoordToIndex(mousey)].code = 0;
            saved = false;

            picBoard.Invalidate();
        }

        // Open a saved board
        private void mnuOpen_Click(object sender, EventArgs e)
        {
            // Ask for user to save board if unsaved
            if (!saved)
            {
                DialogResult d = MessageBox.Show("Would you like to save your changes?", "Create a Board", MessageBoxButtons.YesNoCancel);
                if (d == DialogResult.Yes)
                    Save();
                else if (d == DialogResult.Cancel)
                    return;
            }

            // Open a board
            ofd.Filter = "Text Files (*.txt)|*.txt";
            ofd.FileName = "";

            string f;
            if (ofd.ShowDialog() == DialogResult.OK)
                f = ofd.FileName;
            else
                return;

            file = f;
            board = new Board(10, 10, file);
            picBoard.Invalidate();
        }

        // Save the current board
        private void mnuSave_Click(object sender, EventArgs e)
        {
            Save();
        }
        private void mnuSaveAs_Click(object sender, EventArgs e)
        {
            // Save as a new file
            prevFile = file;
            file = "";
            Save();
        }

        // Close the form
        private void mnuDone_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
        private void frmCreate_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ask for user to save board if unsaved
            if (!saved)
            {
                DialogResult d = MessageBox.Show("Would you like to save your changes?", "Create a Board", MessageBoxButtons.YesNoCancel);
                if (d == DialogResult.Yes)
                    Save();
                else if (d == DialogResult.Cancel)
                    e.Cancel = true;
            }
        }

        // Save the board
        private void Save()
        {
            // "Save as" if no file is saved
            if (file == "")
                GetFile();
            if (file == "")
                return;
            
            StreamWriter writer = new StreamWriter(file, false);
            for (int i = 0; i < board.sizex; i++)
            {
                for (int j = 0; j < board.sizey; j++)
                    writer.Write(board[i, j].code);
                if (i != board.sizex - 1)
                    writer.WriteLine();
            }
            writer.Flush();
            writer.Dispose();
            saved = true;
        }

        // Show the save file dialog and set the file
        private void GetFile()
        {
            // Show save dialog
            sfd.Filter = "Text Files (*.txt)|*.txt";
            sfd.FileName = "";

            DialogResult d = sfd.ShowDialog();
            if (d == DialogResult.OK)
            {
                if (sfd.FileName == "")
                    MessageBox.Show("Please specify a file name.");
                file = sfd.FileName;
            }
            else
                file = prevFile;
        }
    }
}