using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace DeviantArt.Chat.Oberon
{
    public class Console
    {
        private ILog Logger = LogManager.GetLogger(typeof(Console));
        private string Format = "HH:mm:ss";

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
            System.Console.WriteLine(">>" + data);
            Log(">>" + data);
        }

        public void Log(string message)
        {
            Logger.InfoFormat(message);
        }

        public string GetValue(string message)
        {
            while (true)
            {
                System.Console.Write("> " + message);
                string userInput = System.Console.ReadLine();
                if (!string.IsNullOrEmpty(userInput))
                    return userInput;
            }
        }
    }
}
