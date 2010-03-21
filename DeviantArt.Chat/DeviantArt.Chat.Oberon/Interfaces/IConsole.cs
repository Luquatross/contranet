using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// Represents methods a class must implement to take care of logging to a 
    /// log file or writing to the console.
    /// </summary>
    public interface IConsole
    {
        string Time();
        string Time(DateTime ts);

        string Clock();
        string Clock(DateTime ts);

        void Message(string message);
        void Message(string message, DateTime ts);

        void Notice(string message);
        void Notice(string message, DateTime ts);

        void Warning(string message);
        void Warning(string message, DateTime ts);

        void Write(object data);

        void Write(string data);

        void Log(string message);

        void Debug(string message);

        string GetValueFromConsole(string message);
    }
}
