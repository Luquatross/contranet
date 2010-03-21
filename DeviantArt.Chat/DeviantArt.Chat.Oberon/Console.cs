using System;
using System.IO;
using log4net;
using log4net.Config;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// Class that takes care of logging to a log file or writing to the console.
    /// </summary>
    public class Console : IConsole
    {
        #region Private Variables
        /// <summary>
        /// Logger to use for the console.
        /// </summary>
        private ILog Logger = LogManager.GetLogger(typeof(Console));

        /// <summary>
        /// Format for time output.
        /// </summary>
        private string Format = "HH:mm:ss";

        /// <summary>
        /// TextWriter to send output to.
        /// </summary>
        private TextWriter Output;

        /// <summary>
        /// Path to log file.
        /// </summary>
        private string LogConfigPath
        {
            get
            {
                string currentDirectory = System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location);
                return System.IO.Path.Combine(currentDirectory, "Config\\Log.config");
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor, uses the Console.Out to write data.
        /// </summary>
        public Console() : this(System.Console.Out) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="output">Textwriter to send output to.</param>
        public Console(TextWriter output)
        {
            Output = output;
            // configure logging based on config
            XmlConfigurator.Configure(new FileInfo(LogConfigPath));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Current time.
        /// </summary>
        /// <returns>Current time as a string.</returns>
        public string Time() 
        { 
            return Time(DateTime.Now); 
        }
        
        /// <summary>
        /// Returns the time in the hh:mm:ss format as a string.
        /// </summary>
        /// <param name="ts">Time to return.</param>
        /// <returns>Time as a string.</returns>
        public string Time(DateTime ts) 
        { 
            return ts.ToString(Format); 
        }

        /// <summary>
        /// Returns the time wrapped in brackets.
        /// </summary>
        /// <returns>The time wrapped in brackets.</returns>
        public string Clock() 
        { 
            return Clock(DateTime.Now); 
        }

        /// <summary>
        /// Returns the time wrapped in brackets.
        /// </summary>
        /// <param name="ts">Time to return.</param>
        /// <returns>The time wrapped in brackets.</returns>
        public string Clock(DateTime ts) 
        { 
            return "[" + Time(ts) + "]"; 
        }

        /// <summary>
        /// Send a message to the console.
        /// </summary>
        /// <param name="message">Message to send to the console.</param>
        public void Message(string message) 
        { 
            Message(message, DateTime.Now); 
        }

        /// <summary>
        /// Send a message to the console with a timestamp.
        /// </summary>
        /// <param name="message">Message to send to the console.</param>
        /// <param name="ts">Timestamp to use on message.</param>
        public void Message(string message, DateTime ts)
        {
            Log(Clock(ts) + " " + message);
            System.Console.WriteLine(Clock(ts) + " " + message);
        }

        /// <summary>
        /// Send a notice (message preceded with a **) to the console.
        /// </summary>
        /// <param name="message">Notice to send to the console.</param>
        public void Notice(string message) 
        { 
            Notice(message, DateTime.Now); 
        }

        /// <summary>
        /// Send a notice (message preceded with a **) to the console with a timestamp.
        /// </summary>
        /// <param name="message">Notice to send to the console.</param>
        /// <param name="ts">Timestamp to use on message.</param>
        public void Notice(string message, DateTime ts) 
        { 
            Message("** " + message, ts); 
        }

        /// <summary>
        /// Send a warning to the console.
        /// </summary>
        /// <param name="message">Warning to send to the console.</param>
        public void Warning(string message) 
        { 
            Warning(message, DateTime.Now); 
        }

        /// <summary>
        /// Send a warning to the console with a timestamp.
        /// </summary>
        /// <param name="message">Warning to send to the console.</param>
        /// <param name="ts">Timestamp to use on warning.</param>
        public void Warning(string message, DateTime ts) 
        { 
            Message(">> " + message, ts); 
        }

        /// <summary>
        /// Write object to the console.
        /// </summary>
        /// <param name="data">Data to write to console.</param>
        public void Write(object data)
        {
            Write(data.ToString());
        }

        /// <summary>
        /// Writes string to console.
        /// </summary>
        /// <param name="data">Data to write to console.</param>
        public void Write(string data)
        {
            data = data.Replace("\n", "\n>>");        
            Output.WriteLine(">>" + data);
            Log(">>" + data);
        }

        /// <summary>
        /// Sends a message to the log file.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Log(string message)
        {
            Logger.Info(message);
        }

        /// <summary>
        /// Send debug message to the console.
        /// </summary>
        /// <param name="message">Message to send.</param>
        public void Debug(string message)
        {
            Logger.Debug(message);
        }

        /// <summary>
        /// Reads a value from the System.Console. Value is terminated by the user pressing enter.
        /// The newline is not included in the returned string.
        /// </summary>
        /// <param name="message">Message to display to use when asking for input.</param>
        /// <returns>Value from console.</returns>
        public string GetValueFromConsole(string message)
        {
            while (true)
            {
                System.Console.Write("> " + message);
                string userInput = System.Console.ReadLine();
                if (!string.IsNullOrEmpty(userInput))
                    return userInput;
            }
        }
        #endregion
    }
}
