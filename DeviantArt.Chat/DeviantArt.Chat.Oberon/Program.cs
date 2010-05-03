using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;
using System.Web;
using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.Practices.Unity;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// Console interface for bot.
    /// </summary>
    class Program
    {
        #region Private Variables
        /// <summary>
        /// Reference to main bot.
        /// </summary>
        private static Bot bot;

        /// <summary>
        /// Delegate for handling the console close event.
        /// </summary>
        private static ConsoleCloseEventHandler consoleCloseHandler;
        #endregion

        #region Main method
        /// <summary>
        /// Main.
        /// </summary>
        /// <param name="args">Args from the command line.</param>
        static void Main(string[] args)
        {            
            try
            {
                // ensure we only have one instance running
                if (IsPriorProcessRunning())
                {
                    System.Console.WriteLine("HEY! Only one instance of the application can be started.");
                    System.Console.WriteLine("Shutting down in 10 seconds.");
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10.0));
                    return;
                }

                // check for updates
                bool haveUpdate = Bot.CheckForUpdate();
                if (haveUpdate)
                {
                    // see if user wants to update
                    DialogResult result = MessageBox.Show(
                        "The version of Oberon is out of date. Do you want to update automatically?", 
                        "Oberon", 
                        MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        // Launch updater to update files
                        LaunchUpdater();

                        // exit!
                        return;
                    }
                }

                // create on IoC container
                IUnityContainer GlobalContainer = new UnityContainer();

                // setup the default bot dependencies
                Bot.Setup(GlobalContainer);

                // get a reference to the bot
                bot = GlobalContainer.Resolve<Bot>();

                // set up the close handler
                consoleCloseHandler = new ConsoleCloseEventHandler(Close);
                SetConsoleCtrlHandler(consoleCloseHandler, true);

                // if in update plugin mode, check for plugin updates
                if (args.Length > 0 && args[0] == "-update-plugins")
                {
                    bot.UpdatePlugins();
                }
                else
                {
                    // run the bot
                    bot.Run();

                    // run until bot shuts down
                    while (bot.IsAlive) ;
                }
            }
            catch (Exception ex)
            {
                // errors shouldn't bubble up to here, but if they do, fail gracfully.
                System.Console.WriteLine(ex);               
                System.Console.WriteLine("Fatal Error occurred. Shutting down in 10 seconds...");
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));

                // dump exception to log file
                try
                {
                    string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    string errorFile = Path.Combine(currentDirectory, "error.log");
                    File.WriteAllText(errorFile, ex.ToString());
                }
                catch { }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Returns true if a prior process from the same executable is running. 
        /// Otherwise false.
        /// </summary>
        /// <returns>True if a prior process from the same executable is running. 
        /// Otherwise false.</returns>
        static bool IsPriorProcessRunning()
        {
            Process curr = Process.GetCurrentProcess();
            Process[] procs = Process.GetProcessesByName(curr.ProcessName);
            foreach (Process p in procs)
            {
                if ((p.Id != curr.Id) &&
                    (p.MainModule.FileName == curr.MainModule.FileName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Launches the updater application.
        /// </summary>
        static void LaunchUpdater()
        {
            // files to copy
            string[] files = new string[] { "Updater.exe", "Ionic.Zip.dll" };

            // get directories
            string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string tempDirectory = Path.GetTempPath();            

            // copy files from current directory to temp
            foreach (string file in files)
                File.Copy(
                    Path.Combine(currentDirectory, file),
                    Path.Combine(tempDirectory, file),
                    true);

            // create process and start it
            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(tempDirectory, files.First());
            p.StartInfo.Arguments = Bot.DownloadUpdateUrl + " \"" + currentDirectory + "\"";
            p.Start();
        }

        /// <summary>
        /// Handles the close event if closed manually.
        /// </summary>
        /// <param name="ctrlType">Shutdown type.</param>
        static bool Close(CtrlTypes ctrlType)
        {
            switch (ctrlType)
            {

                case CtrlTypes.CTRL_C_EVENT:
                case CtrlTypes.CTRL_BREAK_EVENT:
                case CtrlTypes.CTRL_CLOSE_EVENT:
                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    bot.Console.Notice("Initiating shutdown.");

                    // send the shutdown command
                    bot.Shutdown();

                    // wait for it to shut down
                    // run until bot shuts down
                    while (bot.IsAlive) ;

                    // we won't be returning to the main thread,
                    // so we're done!
                    break;
            }

            return false;
        }
        #endregion

        #region Unmanaged Code
        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.
        [DllImport("Kernel32")]
        static extern bool SetConsoleCtrlHandler(ConsoleCloseEventHandler Handler, bool Add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        delegate bool ConsoleCloseEventHandler(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        #endregion
    }
}
