using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// Class that represents a chatroom.
    /// </summary>
    public class Chat
    {
        #region Private Variables & Properties
        /// <summary>
        /// PrivClasses for the room. The key is the privclass name, the value is the 
        /// privclass int value.
        /// </summary>
        public Dictionary<string, int> PrivClasses = new Dictionary<string, int>();

        /// <summary>
        /// The chatroom name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Title for the chatroom. Can be html.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Topic for the chatroom. Can be html.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Users who are currently signed into the chatroom. The key is the username
        /// and the value is the <see cref="User"/>.
        /// </summary>
        private Dictionary<string, User> Members = new Dictionary<string, User>();

        /// <summary>
        /// Log file writer.
        /// </summary>
        private StreamWriter LogFile;
        #endregion

        #region Public Properties
        /// <summary>
        /// The number of users currently in the chatroom.
        /// </summary>
        public int UserCount
        {
            get { return Members.Count; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="chatroomName">Name of the chatroom.</param>
        public Chat(string chatroomName)
        {
            // init variables
            Name = chatroomName;
            LogFile = GetLogFileWriter();
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Get user from index.
        /// </summary>
        /// <param name="userName">Username to get.</param>
        /// <returns><see cref="User"/></returns>
        public User this[string userName]
        {
            get
            {
                if (Members.ContainsKey(userName))
                    return Members[userName];
                else
                    return null;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Get the file writer for the log file.
        /// </summary>
        /// <returns>Log file writer.</returns>
        private StreamWriter GetLogFileWriter()
        {
            try
            {
                string logFile = string.Format("{0}\\{0}_{1}.log", Name.TrimStart('#'), DateTime.Now.ToString("yyyy-MM-dd"));
                string logFilePath = Path.Combine(Bot.Instance.CurrentDirectory, "Logs\\Chats\\" + logFile);

                // make sure directory exists
                if (!Directory.Exists(Path.GetDirectoryName(logFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));

                return File.AppendText(logFilePath);
            }
            catch (IOException ex)
            {
                Bot.Instance.Console.Warning(string.Format(
                    "Error writing to the log file for the chatroom {0}. See bot log file for more information.",
                    Name));
                Bot.Instance.Console.Log("Error writing to chat room log file. " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Formats log message so it's suitable for reading in the log file.
        /// </summary>
        /// <param name="message">Message to format.</param>
        /// <returns>Chat log suitable string.</returns>
        private string FormatLogMessage(string message)
        {
            // get rid of abbr
            message = Regex.Replace(message, "<abbr title=\"(.*?)\"></abbr>", "");

            // format links
            message = Regex.Replace(message, "<a href=\"(.*?)\">(.*?)</a>", "[$2]");

            return message;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Logs a message to the chatroom log file.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Log(string message)
        {
            Notice("[" + DateTime.Now.ToString("T") + "] " + FormatLogMessage(message));
        }

        /// <summary>
        /// Logs a notice to the chatroom log file. No timestamp is included and 
        /// mesages is not formatted to remove tags.
        /// </summary>
        /// <param name="message">Message to log to chatroom log file.</param>
        public void Notice(string message)
        {
            if (LogFile != null && LogFile.BaseStream.CanWrite)
            {
                LogFile.WriteLine(message);
                LogFile.Flush();
            }
        }

        /// <summary>
        /// Closes the chat log file.
        /// </summary>
        public void CloseLog()
        {
            if (LogFile != null)
            {
                LogFile.Close();
                LogFile = null;
            }
        }

        /// <summary>
        /// Opens the chat log file for logging.
        /// </summary>
        public void OpenLog()
        {
            if (LogFile == null)
                LogFile = GetLogFileWriter();
        }

        /// <summary>
        /// True if user is signed on, otherwise false.
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <returns>True if user is part of chatroom, otherwise false.</returns>
        public bool ContainsUser(string username)
        {
            return Members.ContainsKey(username);
        }

        /// <summary>
        /// Adds user to current chat.
        /// </summary>
        /// <param name="user">User to register.</param>
        public void RegisterUser(User user)
        {
            Members.Add(user.Username, user);
        }

        /// <summary>
        /// Remove user from the chat.
        /// </summary>
        /// <param name="username">User to remove.</param>
        public void UnregisterUser(string username)
        {
            Members.Remove(username);
        }

        /// <summary>
        /// Gets all users in the chatroom.
        /// </summary>
        /// <returns>All chatroom memebers.</returns>
        public User[] GetAllMembers()
        {
            return Members.Values.ToArray();
        }
        #endregion
    }
}
