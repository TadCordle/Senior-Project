using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Windows.Forms;

namespace Senior_Project
{
    partial class frmMain : Form
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
		AutoResetEvent[] aiMove; // Signalling mechanism for AI processing.
		AutoResetEvent aiMoveComplete = new AutoResetEvent(false);

		Type ai1Type = typeof(AI1), // Default AI1 class.
			 ai2Type = typeof(Nolan_AI.ICanSeeForever); // Default AI2 class.

		public Type AI1Class
		{
			get { return this.ai1Type; }
			set
			{
				if (!(value.IsSubclassOf(typeof(AI))))
					throw new ArgumentException("AI does not subclass Senior_Project.AI!");

				try
				{
					value.GetConstructor(new[] { typeof(Board), typeof(int) });
				}
				catch (Exception e)
				{
					throw new ArgumentException("AI does not implement expected constructor .ctor(Board, int)", e);
				}

				this.ai1Type = value;
			}
		}
		public Type AI2Class
		{
			get { return this.ai2Type; }
			set
			{
				if (!(value.IsSubclassOf(typeof(AI))))
					throw new ArgumentException("AI does not subclass Senior_Project.AI!");

				try
				{
					value.GetConstructor(new[] { typeof(Board), typeof(int) });
				}
				catch (Exception e)
				{
					throw new ArgumentException("AI does not implement expected constructor .ctor(Board, int)", e);
				}

				this.ai2Type = value;
			}
		}

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
				using (StreamWriter w = new StreamWriter(file))
				{
					w.Write(dflt);
					w.Flush();
				}
            }

            // Set up a game
            stopai = false;
            SetGame(GameType.CPU);

            aiThread = new Thread[2];
            aiThreadStart = new ThreadStart[2];
			aiMove = new[] { new AutoResetEvent(false), new AutoResetEvent(false) };

            aiThread[0] = new Thread(aiThreadStart[0] = new ThreadStart(delegate
            {
				while(!(stopai || endgame))
				{
					aiMove[0].WaitOne(); // Wait for signal.

					Thread.Sleep(aiSleepTime); // Sleep before doing.
					
					try
					{
						ai1.MakeMove();
					}
					catch (SecurityException ex)
					{
						MessageBox.Show("AI sandboxing exception! Current AI turn execution aborted.\n\n" + ex,
							"Dynamic AI Loading",
							MessageBoxButtons.OK,
							MessageBoxIcon.Exclamation);
					}

					aiMoveComplete.Set(); // Tell main thread we've finished.
				}
            }));
			aiThread[0].IsBackground = true;

            aiThread[1] = new Thread(aiThreadStart[1] = new ThreadStart(delegate
            {
				while (!(stopai || endgame))
				{
					aiMove[1].WaitOne(); // Wait for signal.

					Thread.Sleep(aiSleepTime); // Sleep before doing.

					try
					{
						ai2.MakeMove();
					}
					catch (SecurityException ex)
					{
						MessageBox.Show("AI sandboxing exception! Current AI turn execution aborted.\n\n" + ex,
							"Dynamic AI Loading",
							MessageBoxButtons.OK,
							MessageBoxIcon.Exclamation);
					}
					
					aiMoveComplete.Set(); // Tell main thread we've finished.
				}
            }));
			aiThread[1].IsBackground = true;
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
			using (frmCreate c = new frmCreate())
			{
				c.ShowDialog();
			}
        }

        // Start a new game
        private void mnuNewGame_Click(object sender, EventArgs e)
        {
			using (frmOpponent f = new frmOpponent())
			{
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
                    if (!SetGame(GameType.Human))
                        return;
				}
				else if (result == DialogResult.Yes)
				{
					if (!endgame && (type == GameType.AIvsAI))
						stopai = true;
					SetGame(GameType.CPU);

                    aiThread[1] = new Thread(aiThreadStart[1]);
                    aiThread[1].IsBackground = true;
				}
				else if (result == DialogResult.Ignore)
				{
					stopai = false;
					if (!SetGame(GameType.AIvsAI))
						return;

					aiThread[0] = new Thread(aiThreadStart[0]);
					aiThread[0].IsBackground = true;
					aiThread[1] = new Thread(aiThreadStart[1]);
					aiThread[1].IsBackground = true;

					int c = 1;
					while (!endgame && type == GameType.AIvsAI)
					{
						DoAI(c);
						c = (c == 1 ? 2 : 1);
					}
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
				using (Pen p = new Pen(Color.Gray))
				{
					for (int i = 1; i < 10; i++)
					{
						g.DrawLine(p, i * 48, 0, i * 48, picBoard.Height);
						g.DrawLine(p, 0, i * 48, picBoard.Width, i * 48);
					}
				}
            }

            // Draw a box around the selected piece
            GamePiece gp = board.SelectedPiece();
            if (gp != null)
            {
				using (Pen p = new Pen(Color.Black))
				{
					g.DrawRectangle(p, Board.IndexToCoord(gp.x) + 1, Board.IndexToCoord(gp.y) + 1, 46, 46);
				}
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
                    GamePiece clicked = board.GetPieceAtPos(Board.CoordToIndex(mousex), Board.CoordToIndex(mousey));
					foreach (GamePiece gp in board)
					{
						if (gp == clicked)
							continue;
						gp.Selected = false;
					}
                    if (clicked.Selected)
                        clicked.Selected = false;
                    else
                        clicked.Selected = true;
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
                board.SelectedPiece().Selected = false;
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
			if (!aiThread[code - 1].IsAlive)
				aiThread[code - 1].Start();
			aiMove[code - 1].Set();
			while (!aiMoveComplete.WaitOne(10))
				Application.DoEvents();

			Application.DoEvents();

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
		// returns false if setup fails, true otherwise.
        private bool SetGame(GameType mode)
        {
            // Set up new game
            type = mode;
            board = new Board(file);
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

			// Catch attempts to violate sandboxing.
			try
			{
				ai1 = Activator.CreateInstance(ai1Type, board, 1) as AI;
				ai2 = Activator.CreateInstance(ai2Type, board, 2) as AI;
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException is SecurityException)
				{
					MessageBox.Show("AI sandboxing exception! Game execution aborted.\n\n" + ex.InnerException,
						"Dynamic AI Loading",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);

					return false;
				}
				else throw;
			}

            picBoard.Invalidate();
			return true;
        }

        // Show extra forms
        private void mnuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Program by Tad Cordle and Nolan Lum\n" +
                            "Programmed in C# .NET", "About");
        }
        private void mnuHTP_Click(object sender, EventArgs e)
        {
			using (frmPlayHelp f = new frmPlayHelp())
			{
				f.ShowDialog();
			}
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

		// Display AI chooser.
		private void mnuChooseAI_Click(object sender, EventArgs e)
		{
			using (var aier = new frmAIChooser())
			{
				aier.AI1Type = this.ai1Type;
				aier.AI2Type = this.ai2Type;

				if (aier.ShowDialog() == DialogResult.OK)
				{
					if (aier.AI1Type != this.ai1Type)
						this.ai1Type = aier.AI1Type;
					if (aier.AI2Type != this.ai2Type)
						this.ai2Type = aier.AI2Type;
				}
			}
		}
    }
}