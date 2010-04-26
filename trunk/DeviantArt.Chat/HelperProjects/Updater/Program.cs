using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Updater
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // ensure arg length
            if (args.Length < 2)
            {
                MessageBox.Show("You must supply the update URL and extract location as arguments.", "Updater Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // get args
            string updateUrl = args[0];
            string extractLocation = args[1];
            string applicationToLaunch = (args.Length >= 3) ? args[2] : null;
            
            // kick off app
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UpdateForm(updateUrl, extractLocation, applicationToLaunch));
        }
    }
}
