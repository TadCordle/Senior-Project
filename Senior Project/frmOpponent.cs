using System;
using System.Windows.Forms;

namespace Senior_Project
{
    partial class frmOpponent : Form
    {
        // Create the form
        public frmOpponent()
        {
            InitializeComponent();
        }

        // Return a value based on the user's choice
        private void btnAI_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
        }
        private void btnHuman_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
        private void btnAIvsAI_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Ignore;
        }
    }
}