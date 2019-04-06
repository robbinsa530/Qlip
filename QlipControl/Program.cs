using System;
using System.Windows.Forms;
using MutexManager;

namespace Qlip
{
    /// <summary>
    /// Framework for restricting app to a single instance and for running as a tray app.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!SingleInstance.Start()) { return; }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                var applicationContext = new CustomApplicationContext();
                Application.Run(applicationContext);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program Terminated Unexpectedly",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SingleInstance.Stop();
        }
    }
}
