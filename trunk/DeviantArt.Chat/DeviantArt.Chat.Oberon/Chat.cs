using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// Class that represents a chatroom.
    /// </summary>
    public class Chat
    {
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

        /// <summary>
        /// Get the file writer for the log file.
        /// </summary>
        /// <returns>Log file writer.</returns>
        private StreamWriter GetLogFileWriter()
        {
            string logFile = string.Format("{0}_{1}.log", Name.TrimStart('#'), DateTime.Now.ToString("yyyy-MM-dd"));
            string logFilePath = Path.Combine(Bot.Instance.CurrentDirectory, "Logs\\Chats\\" + logFile);
            return new StreamWriter(File.Open(logFilePath, FileMode.OpenOrCreate, FileAccess.Write));
        }

        /// <summary>
        /// Logs a message to the chatroom log file.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Log(string message)
        {
            Notice("[" + DateTime.Now.ToString("T") + "] " + message);
        }

        /// <summary>
        /// Logs a notice to the chatroom log file. No timestamp is included.
        /// </summary>
        /// <param name="message"></param>
        public void Notice(string message)
        {
            LogFile.WriteLine(message);
            LogFile.Flush();
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
    }
}
