using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Library
{
    public class Console
    {
        protected string Format = "HH:mm:ss";

        public string Time() { return Time(DateTime.Now); }
        public string Time(DateTime ts) { return ts.ToString(Format); }

        public string Clock() { return Clock(DateTime.Now); }
        public string Clock(DateTime ts) { return "[" + Time(ts) + "]"; }

        public void Message(string message) { Message(message, DateTime.Now); }
        public void Message(string message, DateTime ts)
        {
            // TODO : if debug then log
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
            // TODO : if debug then log
        }
    }
}
