using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Tree = Senior_Project.ICanSeeForever.Tree<Senior_Project.ICanSeeForever.Move, Senior_Project.Board>;

namespace Senior_Project
{
    partial class AI3DebugForm : Form
    {
        public AI3DebugForm()
        {
            InitializeComponent();
        }

        public void AddTrace(string s)
        {
            if (txtTrace.InvokeRequired)
                txtTrace.BeginInvoke(new Action(delegate() { txtTrace.AppendText(s + "\n"); }));
            else
                txtTrace.AppendText(s + "\n");
        }

        public void DrawBoard(Board b)
        {
            // todo - implement this.
        }

        public void DrawTree(Tree t)
        {
            // todo - implement this
        }
    }
}
