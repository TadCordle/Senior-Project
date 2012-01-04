using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Senior_Project
{
    class GamePiece
    {
        private static readonly Image[] codes = { 
                                            null,
                                            Image.FromFile(Application.StartupPath + @"\1.bmp"),
                                            Image.FromFile(Application.StartupPath + @"\2.bmp"),
                                            Image.FromFile(Application.StartupPath + @"\3.bmp")
                                         };

        public int code // An integer that represents the type of game piece
        {
            get { return ct; }
            set 
            {
                ct = value;
                Sprite = codes[value]; 
            }
        }
        public Image Sprite; // The piece's image
        public bool selected; // Whether or not the piece is selected
        public int x, y; // The piece's position (in array coordinates) 

        private int ct;

        // Create a new game piece
        public GamePiece(int xp, int yp, int c)
        {
            code = c;
            x = xp;
            y = yp;
            selected = false;
        }

        // Check if xto and yto are close enough to x and y to make a move
        public bool MoveIsInRange(int xto, int yto)
        {
            return
            (
                (Math.Abs(xto - x) <= 1 && Math.Abs(yto - y) <= 1) ||
                (Math.Abs(xto - x) == 2 && Math.Abs(yto - y) == 0) ||
                (Math.Abs(xto - x) == 0 && Math.Abs(yto - y) == 2) ||
                (Math.Abs(xto - x) == 2 && Math.Abs(yto - y) == 2)
            );
        }

        // Return the code of the game piece as a string
        public override string ToString()
        {
            return code.ToString();
        }
    }
}
