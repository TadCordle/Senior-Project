using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Senior_Project
{
    public partial class frmMain : Form
    {
        #region Variables

        Board board; // The game board
        Graphics g; // For drawing
        bool showGrid; // Whether or not to draw the grid
        bool playerTurn; // True if currently player 1's turn
        GameType type; // Represents the type of game
        bool endgame; // True if one of the players cannot move
        string file; // Current board's filepath

        // AI controls
        AI ai1, ai2;
        Thread[] aiThread;
        ThreadStart[] aiThreadStart;
        bool stopai; // Used to break out of AI thread when a new game is selected mid-turn
        const int aiSleepTime = 1; // Milliseconds AI pauses between turns

        // Game types
        private enum GameType
        {
            CPU,
            Human,
            AIvsAI
        };

        #endregion

        // Initialize variables and start a Player vs. CPU game
        public frmMain()
        {
            InitializeComponent();

            // Read in a board from the default file or create a new file if "default.txt" doesn't exist
            file = Application.StartupPath + "/boards/default.txt";
            if (!File.Exists(file))
            {
                string dflt = "2000000001\n" +
                              "0000000000\n" +
                              "0000000000\n" +
                              "0000000000\n" +
                              "0000330000\n" +
                              "0000330000\n" +
                              "0000000000\n" +
                              "0000000000\n" +
                              "0000000000\n" +
                              "2000000001";
                StreamWriter w = new StreamWriter(file);
                w.Write(dflt);
                w.Flush();
                w.Close();
            }

            // Set up a game
            stopai = false;
            SetGame(GameType.CPU);

            aiThread = new Thread[2];
            aiThreadStart = new ThreadStart[2];

            aiThread[0] = new Thread(aiThreadStart[0] = new ThreadStart(delegate
            {
                Thread.Sleep(aiSleepTime);
                ai1.MakeMove();
            }));

            aiThread[1] = new Thread(aiThreadStart[1] = new ThreadStart(delegate
            {
                Thread.Sleep(aiSleepTime);
                ai2.MakeMove();
            }));
        }

        // Load a level from a text file
        private void mnuLoad_Click(object sender, EventArgs e)
        {
            // Show open file dialog
            ofd.Filter = "Text Files (*.txt)|*.txt";
            ofd.FileName = "";

            string f;
            if (ofd.ShowDialog() == DialogResult.OK)
                f = ofd.FileName;
            else
                return;

            file = f;
            SetGame(type);

            // Start game if AI vs. AI game mode
            if (type == GameType.AIvsAI)
            {
                stopai = false;

                aiThread[0] = new Thread(aiThreadStart[0]);
                aiThread[1] = new Thread(aiThreadStart[1]);

                int c = 1;
                while (!endgame && type == GameType.AIvsAI)
                {
                    DoAI(c);
                    c = (c == 1 ? 2 : 1);
                }
            }
        }

        // Show board creation form
        private void mnuCreate_Click(object sender, EventArgs e)
        {
            frmCreate c = new frmCreate();
            c.ShowDialog();
        }

        // Start a new game
        private void mnuNewGame_Click(object sender, EventArgs e)
        {
            frmOpponent f = new frmOpponent();
            DialogResult result = f.ShowDialog();

            if (result != DialogResult.Cancel)
            {
                // Make sure board's file still exists if a new game is chosen
                if (!File.Exists(file))
                {
                    MessageBox.Show("New game failed to load because board's file was not found.");
                    return;
                }

                // Stop the AI if a new game is chosen while the AI is thinking
                if (aiThread[0].IsAlive)
                    aiThread[0].Abort();
                if (aiThread[1].IsAlive)
                    aiThread[1].Abort();

                if (!endgame && (type == GameType.AIvsAI || (type == GameType.CPU && !playerTurn)))
                    stopai = true;
            }

            // Set new game type based on the dialog's chosen item
            if (result == DialogResult.OK)
            {
                if (!endgame && (type == GameType.AIvsAI))
                    stopai = true;
                SetGame(GameType.Human);
            }
            else if (result == DialogResult.Yes)
            {
                if (!endgame && (type == GameType.AIvsAI))
                    stopai = true;
                SetGame(GameType.CPU);
            }
            else if (result == DialogResult.Ignore)
            {
                stopai = false;
                SetGame(GameType.AIvsAI);

                aiThread[0] = new Thread(aiThreadStart[0]);
                aiThread[1] = new Thread(aiThreadStart[1]);

                int c = 1;
                while (!endgame && type == GameType.AIvsAI)
                {
                    DoAI(c);
                    c = (c == 1 ? 2 : 1);
                }
            }
        }

        // Redraw the game board
        private void picBoard_Paint(object sender, PaintEventArgs e)
        {
            // Update the board's image
            g = e.Graphics;
            board.Draw(g);

            // Draw the grid if the checkbox is checked
            if (showGrid)
            {
                Pen p = new Pen(Color.Gray);
                for (int i = 1; i < 10; i++)
                {
                    g.DrawLine(p, i * 48, 0, i * 48, picBoard.Height);
                    g.DrawLine(p, 0, i * 48, picBoard.Width, i * 48);
                }
            }

            // Draw a box around the selected piece
            GamePiece gp = board.SelectedPiece();
            if (gp != null)
            {
                Pen p = new Pen(Color.Black);
                g.DrawRectangle(p, Board.IndexToCoord(gp.x)+1, Board.IndexToCoord(gp.y)+1, 46, 46);
            }
        }

        // Choose whether or not to show the grid
        private void cbGrid_CheckedChanged(object sender, EventArgs e)
        {
            showGrid = !showGrid;
            picBoard.Invalidate();
        }

        // Handle player selection and movement
        private void picBoard_MouseClick(object sender, MouseEventArgs e)
        {
            if (!picBoard.Enabled) return;

            stopai = false;

            // Get mouse position relative to the game board
            int mousex = (int)(picBoard.PointToClient(Cursor.Position).X);
            int mousey = (int)(picBoard.PointToClient(Cursor.Position).Y);

            // Select or deselect a piece
            if (e.Button == MouseButtons.Left)
            {
                if (playerTurn || type == GameType.Human)
                {
                    GamePiece clicked = board[Board.CoordToIndex(mousex), Board.CoordToIndex(mousey)];
                    foreach (GamePiece g in board.board)
                    {
                        if (g == clicked)
                            continue;
                        g.selected = false;
                    }
                    if (clicked.selected)
                        clicked.selected = false;
                    else
                        clicked.selected = true;
                }
            }
            // Move selected piece
            else if (e.Button == MouseButtons.Right)
            {
                // Represents whose turn it is
                int pcode;
                if (type == GameType.Human && !playerTurn)
                    pcode = 2;
                else
                    pcode = 1;

                bool moved = board.MoveSelectedPiece(Board.CoordToIndex(mousex), Board.CoordToIndex(mousey), pcode);
                if (!moved)
                    return;

                // Set up next player's turn
                playerTurn = !playerTurn;
                board.SelectedPiece().selected = false;
                picBoard.Invalidate();

                // If the next player can't move, fill in all reachable empty spaces
                if (board.Count(pcode == 1 ? 2 : 1) != 0 && !board.HasMovesLeft((pcode == 1 ? 2 : 1)))
                {
                    MessageBox.Show("The next player has no moves left.");
                    board.FillEmptySpaces(pcode);
                    endgame = true;
                }

                // If player 2 is not human, tell AI to move
                UpdateGameStats(playerTurn, "Thinking...");
                if (picBoard.Enabled && type != GameType.Human)
                    DoAI(2);
            }

            picBoard.Invalidate();
        }

        // Make the computer make a move ("code" is the code of the ai)
        private void DoAI(int code)
        {
            // Make AI execute its move
            try
            {
                if (aiThread[code - 1].ThreadState != ThreadState.Running)
                    aiThread[code - 1].Start();
                while (aiThread[code - 1].IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                }
                aiThread[code - 1] = new Thread(aiThreadStart[code - 1]);
            }
            catch (ThreadStateException) { }

            // If a new game was chosen mid-AIvsAI game, reset the board
            if (stopai)
            {
                SetGame(type);
                return;
            }

            // Check if game is finished
            int other = (code == 1 ? 2 : 1);
            if (!endgame)
            {
                picBoard.Invalidate();
                if (board.Count(other) != 0 && !board.HasMovesLeft(other))
                {
                    if (type == GameType.AIvsAI)
                        MessageBox.Show("The next AI has no moves left.");
                    else
                        MessageBox.Show("You have no moves left.");
                    board.FillEmptySpaces(code);
                    picBoard.Invalidate();
                    endgame = true;
                }
                playerTurn = !playerTurn;
                UpdateGameStats(playerTurn, "Waiting for player " + other.ToString());
            }

            picBoard.Invalidate();
        }

        // Update the side text and check if the game is over
        private void UpdateGameStats(bool turn, string status)
        {
            // Update piece counters
            int pcount = board.Count(1);
            int aicount = board.Count(2);
            lblAICount.Text = aicount.ToString();
            lblPlayerCount.Text = pcount.ToString();
            
            // Update AI's status
            lblStatus.Text = status;

            // Update player turn display
            string PType;
            if (type == GameType.AIvsAI)
                PType = "AI";
            else
                PType = "Player";
            
            if (turn)
                if (type == GameType.Human || type == GameType.AIvsAI)
                    lblTurn.Text = PType + " 1";
                else
                    lblTurn.Text = "Player";
            else
                if (type == GameType.Human || type == GameType.AIvsAI)
                    lblTurn.Text = PType + " 2";
                else
                    lblTurn.Text = "CPU";

            // Game ender
            if (board.Count(0) == 0 || aicount == 0 || pcount == 0 || endgame)
            {
                if (pcount > aicount)
                {
                    if (type == GameType.Human || type == GameType.AIvsAI)
                        MessageBox.Show(PType + " 1 wins!");
                    else
                        MessageBox.Show("You win!");
                    lblStatus.Text = "Raging";
                }
                else if (aicount > pcount)
                {
                    if (type == GameType.Human || type == GameType.AIvsAI)    
                        MessageBox.Show(PType + " 2 wins!");
                    else
                        MessageBox.Show("You lose!");
                    lblStatus.Text = "Bragging to its friends";
                }
                else
                {
                    MessageBox.Show("It was a tie!");
                    lblStatus.Text = "Being indifferent";
                }
                picBoard.Enabled = false;
                endgame = true;
            }
        }

        // Set up a new game with a type of "mode"
        private void SetGame(GameType mode)
        {
            // Set up new game
            type = mode;
            board = new Board(10, 10, file);
            playerTurn = true;
            endgame = false;
            picBoard.Enabled = mode != GameType.AIvsAI;
            
            // Set up HUD
            HUD2.Visible = lblStatus.Visible = mode != GameType.Human;
            UpdateGameStats(playerTurn, "Waiting for player" + (mode != GameType.CPU ? "1" : ""));
            if (mode == GameType.CPU)
            {
                HUD3.Text = "Player 1 Pieces:";
                HUD4.Text = "CPU Pieces:";
            }
            else if (mode == GameType.Human)
            {
                HUD3.Text = "Player 1 Pieces:";
                HUD4.Text = "Player 2 Pieces:";
            }
            else if (mode == GameType.AIvsAI)
            {
                HUD3.Text = "AI 1 Pieces:";
                HUD4.Text = "AI 2 Pieces:";
            }

            ai1 = new AI1(board, 1);
            ai2 = new AI2(board, 2);

            picBoard.Invalidate();
        }

        // Show extra forms
        private void mnuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Program by Tad Cordle\n" +
                            "Programmed in C# .NET", "About");
        }
        private void mnuHTP_Click(object sender, EventArgs e)
        {
            frmPlayHelp f = new frmPlayHelp();
            f.ShowDialog();
        }

        // Exit the game
        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (aiThread[0].ThreadState == ThreadState.Running)
                aiThread[0].Abort();
            if (aiThread[1].ThreadState == ThreadState.Running)
                aiThread[1].Abort();
            endgame = true;
            Application.Exit();
        }
        private void mnuExit_Click(object sender, EventArgs e)
        {
            if (aiThread[0].ThreadState == ThreadState.Running)
                aiThread[0].Abort();
            if (aiThread[1].ThreadState == ThreadState.Running)
                aiThread[1].Abort();
            endgame = true;
            Application.Exit();
        }
    }
}