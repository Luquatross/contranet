using System;
using System.IO;
using log4net;
using log4net.Config;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// Class that takes care of logging to a log file or writing to the console.
    /// </summary>
    public class Console
    {
        #region Private Variables
        private ILog Logger = LogManager.GetLogger(typeof(Console));
        private string Format = "HH:mm:ss";
        private TextWriter Output;

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
        public Console() : this(System.Console.Out) { }

        public Console(TextWriter output)
        {
            Output = output;
            // configure logging based on config
            XmlConfigurator.Configure(new FileInfo(LogConfigPath));
        }
        #endregion

        #region Public Methods
        public string Time() { return Time(DateTime.Now); }
        public string Time(DateTime ts) { return ts.ToString(Format); }

        public string Clock() { return Clock(DateTime.Now); }
        public string Clock(DateTime ts) { return "[" + Time(ts) + "]"; }

        public void Message(string message) { Message(message, DateTime.Now); }
        public void Message(string message, DateTime ts)
        {
            Log(Clock(ts) + " " + message);
            System.Console.WriteLine(Clock(ts) + " " + message);
        }

        public void Notice(string message) { Notice(message, DateTime.Now); }
        public void Notice(string message, DateTime ts) { Message("** " + message, ts); }

        public void Warning(string message) { Warning(message, DateTime.Now); }
        public void Warning(string message, DateTime ts) { Message(">> " + message, ts); }

        public void Write(object data)
        {
            Write(data.ToString());
        }

        public void Write(string data)
        {
            data = data.Replace("\n", "\n>>");        
            Output.WriteLine(">>" + data);
            Log(">>" + data);
        }

        public void Log(string message)
        {
            Logger.InfoFormat(message);
        }

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
