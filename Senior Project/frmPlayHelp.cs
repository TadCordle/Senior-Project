using System;
using System.Drawing;
using System.Windows.Forms;

namespace Senior_Project
{
    partial class frmPlayHelp : Form
    {
        #region Variables

        const int max = 4;
        int index;
        Image[] helpImages;
        string[] helpText;

        #endregion

        // Create the form
        public frmPlayHelp()
        {
            InitializeComponent();
        }

        // Load images and help text
        private void frmPlayHelp_Load(object sender, EventArgs e)
        {
            index = 0;
            
            // Load help images
            helpImages = new Image[max];
            helpImages[0] = Properties.Resources.help0;
            helpImages[1] = Properties.Resources.help1;
            helpImages[2] = Properties.Resources.help2;
            helpImages[3] = Properties.Resources.help3;

            // Load help text
            helpText = new string[max];
            helpText[0] = "In this game, player 1 will control the green pieces and player 2 (which may be human \n" +
                          "or the CPU) will control the red pieces. Gray pieces are neutral; they can't be \n" +
                          "moved or converted.";
            helpText[1] = "Left click on a piece to select it (a selected piece will have a black box drawn around\n" +
                          "it) and right click on the area you want to move to. If you right click on an adjacent\n" +
                          "space (shown in orange in the picture above) you will spawn a new piece in that space.\n" +
                          "If you right click on a space 2 squares away (shown in blue), the selected piece will\n" +
                          "jump to that spot.";
            helpText[2] = "When you make a move, all enemy pieces adjacent to where you moved will be\n" +
                          "converted into your pieces.";
            helpText[3] = "The game ends when the next player has no moves left. The winner is the player with\n" +
                          "the most pieces at the end of the game!";

            picHelpPic.Image = helpImages[index];
            lblHelpText.Text = helpText[index];
        }

        // Cycle through rules
        private void btnRight_Click(object sender, EventArgs e)
        {
            // Cycle through the game rules -->
            index++;
            btnLeft.Enabled = true;
            if (index == max - 1)
                btnRight.Enabled = false;

            picHelpPic.Image = helpImages[index];
            lblHelpText.Text = helpText[index];
        }
        private void btnLeft_Click(object sender, EventArgs e)
        {
            // Cycle through the game rules <--
            index--;
            btnRight.Enabled = true;
            if (index == 0)
                btnLeft.Enabled = false;

            picHelpPic.Image = helpImages[index];
            lblHelpText.Text = helpText[index];
        }
    }
}
