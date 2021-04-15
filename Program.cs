using System;
using System.Windows.Forms;

namespace QTSuperMarket
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new adminMainForm());
            //Application.Run(new GuideForm());
            //Application.Run(new LoginForm());
            //Application.Run(new Loading());
            //Application.Run(new workerMainForm());
            //Application.Run(new tiaoshi());
        }
    }
}
