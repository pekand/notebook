using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notebook
{
    static class Program
    {
        public static GlobalOptions options = new GlobalOptions();

        public static FormConsole console = null;


        public static FormNotebook formNotebook = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if DEBUG
            string appID = "NOTEBOOK_DEBUG";
#else
            string appID = "NOTEBOOK_ZHE4JRDLA4YAXMN8TO2DM4PTMBLM2L11";
#endif

            Program.options.LoadConfigFile();

            bool createdNew = false;
            using (Mutex mutex = new Mutex(true, appID, out createdNew))
            {
                if (createdNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    console = new FormConsole();
                    console.Write("AppId: " + appID);
                    formNotebook = new FormNotebook();
                    Application.Run(formNotebook);
                }
            }
        }
    }
}
