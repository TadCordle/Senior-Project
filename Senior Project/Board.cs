using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Senior_Project
{
    class Board
    {
        public GamePiece[,] board;
        public int sizex, sizey;

        // Constructors
        public Board(int sizex, int sizey, string file)
        {
            this.sizex = sizex;
            this.sizey = sizey;

            board = new GamePiece[sizey, sizex];

            // Load the board from a text file
            StreamReader load;
            if(File.Exists(file))
                load = new StreamReader(file);
            else
            {
                MessageBox.Show("Load failed because the file " + file + " was not found.");
                return;
            }

            for (int i = 0; i < sizex; i++)
            {
                string line = load.ReadLine();
                for (int j = 0; j < sizey; j++)
                    board[i, j] = new GamePiece(i, j, int.Parse(line[j].ToString()));
            }
            load.Dispose();
        }
        public Board(int sizex, int sizey)
        {
            this.sizex = sizex;
            this.sizey = sizey;
            board = new GamePiece[sizex, sizey];
        }

        public GamePiece this[int posr, int posc]
        {
            get { return board[posr, posc]; }
            set { board[posr, posc] = value; }
        }

        // Draw the game board
        public void Draw(Graphics g)
        {
            for (int i = 0; i < sizey; i++)
                for (int j = 0; j < sizex; j++)
                    if (board[j, i].Sprite != null)
                        g.DrawImage(board[j, i].Sprite, IndexToCoord(j)+1, IndexToCoord(i)+1, 47, 47);
        }

        // Return the board's selected piece
        public GamePiece SelectedPiece()
        {
            foreach (GamePiece g in board)
                if (g.selected)
                    return g;

            return null;
        }

        // Change all spaces adjacent to space (x, y) with code c2 to code c1
        public int Convert(int x, int y, int c1, int c2)
        {
            int count = 0;

            if (c1 == c2)
            {
                for (int i = -1; i <= 1; i++)
                    for (int j = -1; j <= 1; j++)
                        if (x + i < sizex && x + i >= 0 && y + j < sizey && y + j >= 0)
                            if (board[x + i, y + j].code == c2)
                                count++;
            }
            else
            {
                for (int i = -1; i <= 1; i++)
                    for (int j = -1; j <= 1; j++)
                        if (x + i < sizex && x + i >= 0 && y + j < sizey && y + j >= 0)
                            if (board[x + i, y + j].code == c2)
                            {
                                board[x + i, y + j].code = c1;
                                count++;
                            }
            }

            return count;
        }

        // Return a count of all pieces on the board with code c
        public int Count(int c)
        {
            int count = 0;
            foreach (GamePiece gp in board)
                if (gp.code == c)
                    count++;
            return count;
        }

        // Checks if there are any pieces with code c that can move
        public bool HasMovesLeft(int c)
        {
            foreach (GamePiece gp in board)
            {
                if (gp.code != c) continue;

                for(int i = -2; i <= 2; i++)
                    for (int j = -2; j <= 2; j++)
                        if (gp.MoveIsInRange(gp.x + i, gp.y + j))
                            if (gp.x + i < sizex && gp.x + i >= 0 && gp.y + j < sizey && gp.y + j >= 0)
                                if (board[gp.x + i, gp.y + j].code == 0)
                                    return true;
            }
            return false;
        }

        // Moves the selected piece and returns a value representing whether or not the move failed
        public bool MoveSelectedPiece(int xto, int yto, int pcode)
        {
            GamePiece gp = this.SelectedPiece();
            if (gp == null || gp.code != pcode)
                return false;

            // Move or duplicate the selected piece
            if (!gp.MoveIsInRange(xto, yto) || board[xto, yto].code != 0)
                return false;
            else
            {
                board[xto, yto].code = pcode;
                // If position is far enough away, jump instead of duplicating
                if (!(Math.Abs(xto - gp.x) <= 1 && Math.Abs(yto - gp.y) <= 1))
                    gp.code = 0;
                this.Convert(xto, yto, pcode, (pcode == 1 ? 2 : 1));
            }

            return true;
        }

        // Fills all of the empty spaces in a board with the pieces that can move there
        public void FillEmptySpaces(int code)
        {
            foreach (GamePiece g in board)
                if (g.code == code)
                    DuplicatePiece(g);
        }
        private void DuplicatePiece(GamePiece gp)
        {                                                   
            for (int i = -2; i <= 2; i++)                   
                for (int j = -2; j <= 2; j++)
                {
                    if (i == 0 && j == 0 || !gp.MoveIsInRange(gp.x + i, gp.y + j))
                        continue;

                    if (gp.x + i < sizex && gp.x + i >= 0 && gp.y + j < sizey && gp.y + j >= 0)
                        if (board[gp.x + i, gp.y + j].code == 0)
                        {
                            board[gp.x + i, gp.y + j].code = gp.code;
                            DuplicatePiece(board[gp.x + i, gp.y + j]);
                        }
                }
        }
        
        // Conversions used for drawing purposes
        #region Conversions

        // Converts array coordinates to pixel coordinates
        public static int IndexToCoord(int n)
        {
            return (n * 48);
        }

        // Converts pixel coordinates to array coordinates
        public static int CoordToIndex(int n)
        {
            return (int)(n / 48);
        }

        #endregion
    }
}