using System;
using System.Drawing;
using System.Windows.Forms;

namespace Senior_Project
{
    public sealed class GamePiece
    {
        private static readonly Image[] codes = { 
                                            null,
                                            Image.FromFile(Application.StartupPath + @"\1.bmp"),
                                            Image.FromFile(Application.StartupPath + @"\2.bmp"),
                                            Image.FromFile(Application.StartupPath + @"\3.bmp")
                                         };

		
		private bool selected;

        public int Code // An integer that represents the type of game piece
        {
            get { return ct; }
            set 
            {
                ct = value;
                Sprite = codes[value]; 
            }
        }
        public Image Sprite { get; set; } // The piece's image
		public bool Selected { get { return selected; } set { selected = value; } } // Whether or not the piece is selected
        public int x, y; // The piece's position (in array coordinates) 

        private int ct;

        // Create a new game piece
        public GamePiece(int xp, int yp, int c)
        {
            Code = c;
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
            return Code.ToString();
        }
    }
}
