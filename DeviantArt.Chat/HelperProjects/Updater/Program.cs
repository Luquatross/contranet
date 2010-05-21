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
                MessageBox.Show("This is the bot updater program. The bot will detect when there is an update and invoke the updater. You cannot run this program directly.", "Oberon Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
