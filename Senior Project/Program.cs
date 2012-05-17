using System;
using System.Windows.Forms;

namespace Senior_Project
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
			new ICanSeeForever.AIBoard(new Board());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
