using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace BeySoft
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        // Enables getting version with:
        //     string version = Program.Version;
        internal static string Version =>
            $"{FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}";
    }
}
