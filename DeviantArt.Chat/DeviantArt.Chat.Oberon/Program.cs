using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;
using System.Web;
using System.Collections;
using System.Runtime.InteropServices;

namespace DeviantArt.Chat.Oberon
{
    class Program
    {
        /// <summary>
        /// Reference to main bot.
        /// </summary>
        private static Bot bot;

        /// <summary>
        /// Delegate for handling the console close event.
        /// </summary>
        private static ConsoleCloseEventHandler consoleCloseHandler;

        /// <summary>
        /// Main.
        /// </summary>
        /// <param name="args">Args from the command line.</param>
        static void Main(string[] args)
        {            
            try
            {
                // create bot and start it
                bot = Bot.Instance;

                // set up the close handler
                consoleCloseHandler = new ConsoleCloseEventHandler(Close);
                SetConsoleCtrlHandler(consoleCloseHandler, true);

                // run the bot
                bot.Run();   
             
                // run until bot shuts down
                while (bot.IsAlive) ;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
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
