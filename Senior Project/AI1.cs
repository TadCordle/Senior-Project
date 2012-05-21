using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;

namespace Senior_Project
{
    sealed class AI1 : AI
    {
        int notaicode;

        public AI1(Board board, int aicode) : base (board, aicode)
        {
            notaicode = (aicode == 2 ? 1 : 2);
        }

        // Controls AI's Decision Making
        public override void MakeMove()
        {
            #region Variables

            //int aCount = board.Count(aicode); // AI count
            int naCount = board.Count(notaicode); // Opponent count
            
            int greatest = -1; // Count for best move
            int movex = 0, movey = 0, curx = 0, cury = 0; // Coordinates for move position and current position
			int[][] positioncheck = new int[][] {
									new[] { 0, 1 },  new[] { 1, 1 },   new[] { 1, 0 },  new[] { 1, -1 }, 
                                    new[] { 0, -1 }, new[] { -1, -1 }, new[] { -1,0 }, new[] { -1, 1 }, 
                                    new[] { 0, 2 },  new[] { 2, 2 },   new[] { 2, 0 },  new[] { 2, -2 }, 
                                    new[] { 0, -2 }, new[] { -2, -2 }, new[] { -2, 0 }, new[] {-2, 2 } }; // Array of possible relative move locations
            bool jumped = false; // Whether or not the current best move is a jump
            int gain = 0; // Conversion count from simulated move
            int defence = 0; // Number of pieces "saved" by simulated move
            Random r = new Random(); // To add variety to games
            bool killedEnemy = false; // True when a move is found that wipes out all enemy pieces

			int maxX = Board.SIZE_X - 1, maxY = Board.SIZE_Y - 1; // Bounds of the board

            #endregion

            #region Offensive Moves

            foreach (GamePiece gp in board)
            {
                if (gp.Code == aicode)
                {
                    for (int i = 0; i < 16; i++) // i>=8 means simulated move is a jump
                    {
                        // Make sure adjacent spot is a valid position
                        if (gp.x + positioncheck[i][0] <= maxX && gp.x + positioncheck[i][0] >= 0 &&
                            gp.y + positioncheck[i][1] <= maxY && gp.y + positioncheck[i][1] >= 0)
                        {
                            if (board[gp.x + positioncheck[i][0], gp.y + positioncheck[i][1]] != 0)
                                continue;
                        }
                        else
                            continue;
                        
                        // Count the gain from each possible move
                        gain = board.Convert(gp.x + positioncheck[i][0], gp.y + positioncheck[i][1], notaicode, notaicode);

                        // Break from search if move that wipes out all pieces is found
                        if (gain - (i >= 8 ? 0 : 1) == naCount)
                            killedEnemy = true;

                        if (i < 8 && !killedEnemy)
                        {
                            gain += 1; // Take duplication into account

                            #region Defend Vulnerable Spots

                            // Fill vulnerable holes
                            if (gain <= 2)
                            {
                                defence = board.Convert(gp.x + positioncheck[i][0], gp.y + positioncheck[i][1], aicode, aicode);
                                if (defence >= 6)
                                {
                                    // If spot is valuable, check if it's in range of enemy pieces
                                    for (int j = 8; j < 16; j++)
                                    {
                                        // gp.x + positioncheck(i) = checked spot, + positioncheck(j) = area around checked spot
                                        int newspotx = gp.x + positioncheck[i][0] + positioncheck[j][0];
                                        int newspoty = gp.y + positioncheck[i][1] + positioncheck[j][1];
                                        if (newspotx <= maxX && newspotx >= 0 && 
                                            newspoty <= maxY && newspoty >= 0)
                                        {
                                            // If it finds a green piece in range
                                            if (board[newspotx, newspoty] == notaicode)
                                            {
                                                // Set gain to the number of pieces saved
                                                gain = defence;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion
                        }
                        else if (i >= 8 && !killedEnemy)
                        {
                            #region Check For Vulnerable Jumping

                            // If jumping, check if leaving a vulnerable spot
                            if (gain >= greatest && gain != naCount)
                            {
                                int adjacentPlayers = board.Convert(gp.x, gp.y, aicode, aicode);
                                if (adjacentPlayers >= 5)
                                {
                                    // If spot is valuable, check if it's in range of enemy pieces
                                    for (int j = 0; j < 16; j++)
                                    {
                                        if (gp.x + positioncheck[j][0] <= maxX && gp.x + positioncheck[j][0] >= 0 &&
                                            gp.y + positioncheck[j][1] <= maxY && gp.y + positioncheck[j][1] >= 0)
                                        {
                                            // If it finds a green piece in range and the simulated move does not consume it
                                            if (board[gp.x + positioncheck[j][0], gp.y + positioncheck[j][1]] == notaicode &&
                                                !(Math.Abs(positioncheck[i][0] - positioncheck[j][0]) == 1 ||
                                                  Math.Abs(positioncheck[i][1] - positioncheck[j][1]) == 1))
                                            {
                                                // Penalize gain
                                                gain -= (adjacentPlayers - 6);
                                                if (j < 8)
                                                    gain -= 1;
                                                if (gain < 1)
                                                    gain = 1;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion
                        }

                        if (gain >= greatest || killedEnemy)
                        {
                            // Duplication is better than jumping if resulting count is equal
                            if (gain == greatest)
                            {
                                if (!jumped && i >= 8 && !killedEnemy)
                                    continue;
                                if (r.Next(2) == 0 && !killedEnemy) // So AI doesn't do the same thing every game
                                    continue;
                            }

                            // Save best move
                            jumped = i >= 8;
                            greatest = gain;
                            curx = gp.x;
                            cury = gp.y;
                            movex = gp.x + positioncheck[i][0];
                            movey = gp.y + positioncheck[i][1];

                            if (killedEnemy)
                                break;
                        }
                    }

                    if (killedEnemy)
                        break;
                }
            }

            #endregion

            #region Block Enemy Moves

            // If found no good moves
            if (greatest == 1 && !killedEnemy)
            {
                jumped = false;
                greatest = 0;

                // Block enemy movement instead of capturing
                foreach (GamePiece gp in board)
                {
                    if (gp.Code == notaicode)
                    {
                        // Check for empty spaces within jumping distance of enemy
                        for (int i = 8; i < 16; i++)
                        {
                            // Make sure checked spot is in bounds
                            if (!(gp.x + positioncheck[i][0] <= maxX && gp.x + positioncheck[i][0] >= 0 &&
                                  gp.y + positioncheck[i][1] <= maxY && gp.y + positioncheck[i][1] >= 0))
                                continue;
                            
                            // If spot is found
                            if (board[gp.x + positioncheck[i][0], gp.y + positioncheck[i][1]] == 0)
                            {
                                // Search for a piece that can block the spot
                                int newx = gp.x + positioncheck[i][0];
                                int newy = gp.y + positioncheck[i][1];
                                GamePiece blocker = null;

                                for (int j = 0; j < 8; j++)
                                {
                                    if (!(newx + positioncheck[j][0] <= maxX && newx + positioncheck[j][0] >= 0 &&
                                          newy + positioncheck[j][1] <= maxY && newy + positioncheck[j][1] >= 0))
                                        continue;
                                    
                                    // Save piece if found
                                    if (board[newx + positioncheck[j][0], newy + positioncheck[j][1]] == aicode)
                                    {
                                        blocker = board.GetPieceAtPos(newx + positioncheck[j][0], newy + positioncheck[j][1]);
                                        break;
                                    }
                                }

                                // If space can't be blocked, look for more
                                if (blocker == null)
                                    continue;

                                int loss = board.Convert(newx, newy, aicode, aicode);

                                // Use move that blocks the most valuable spot
                                if (loss >= greatest)
                                {
                                    greatest = loss;
                                    curx = blocker.x;
                                    cury = blocker.y;
                                    movex = newx;
                                    movey = newy;
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            // Execute the saved move
            if (jumped)
                board[curx, cury] = 0;
            board[movex, movey] = aicode;
            board.Convert(movex, movey, aicode, notaicode);
        }
    }
}