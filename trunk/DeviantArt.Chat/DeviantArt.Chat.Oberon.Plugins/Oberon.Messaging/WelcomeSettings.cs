using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Class that holds welcome message settings for a particular room.
    /// </summary>
    [Serializable()]
    internal class WelcomeSettings
    {
        /// <summary>
        /// Chatroom these settings apply to.
        /// </summary>
        private string ChatroomName;

        /// <summary>
        /// Collection of messages for the whole room.
        /// </summary>
        private List<string> AllMessages = new List<string>();

        /// <summary>
        /// Collection of messages for a priv class. Key is priv class, value is the message.
        /// </summary>
        private Dictionary<string, string> PrivClassMessages = new Dictionary<string, string>();

        /// <summary>
        /// Collection of messages for individual users. Key is username, value is the message.
        /// </summary>
        private Dictionary<string, string> IndividualMessages = new Dictionary<string, string>();

        /// <summary>
        /// Random-number generator.
        /// </summary>
        private Random Randomizer = new Random();

        /// <summary>
        /// Set to true to allow individual settings. 
        /// </summary>
        public bool AllowIndividualSettings { get; set; }

        /// <summary>
        /// Set to true to allow welcome messages. 
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="chatroomName">Chatroom these settinsg apply to.</param>
        public WelcomeSettings(string chatroomName)
        {
            ChatroomName = chatroomName;
        }

        /// <summary>
        /// Helper method to remove null values from a dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to modify.</param>
        private void RemoveNullValues(Dictionary<string, string> dict)
        {
            var nullKeys = from i in dict where i.Value == null select i.Key;
            foreach (string key in nullKeys)
                dict.Remove(key);
        }

        /// <summary>
        /// Gets Chat object from the running bot.
        /// </summary>
        /// <returns>Chat.</returns>
        public Chat GetChat()
        {
            return Bot.Instance.GetChatroom(ChatroomName);
        }

        /// <summary>
        /// Gets the message for all users, if there is one. If there is more than
        /// one message it will pick one randomly.
        /// </summary>
        /// <returns>Message for all users.</returns>
        public string GetAllMessage()
        {
            if (AllMessages.Count == 0)
            {
                return null;
            }
            else
            {
                int randomIndex = Randomizer.Next(AllMessages.Count);
                return AllMessages[randomIndex];
            }
        }

        /// <summary>
        /// Adds message for all users.
        /// </summary>
        /// <param name="message">Message to add.</param>
        public void AddAllMessage(string message)
        {
            AllMessages.Add(message);
        }

        /// <summary>
        /// Removes the specified message from messages for all users.
        /// </summary>
        /// <param name="index">Index of message to remove.</param>
        public void RemoveAllMessage(int index)
        {
            if (AllMessages.Count >= index)
                AllMessages.RemoveAt(index);
        }

        /// <summary>
        /// Removes all messages that apply to all users.
        /// </summary>
        public void ClearAllMessage()
        {
            AllMessages.Clear();
        }

        /// <summary>
        /// Get list of messages for all users.
        /// </summary>
        /// <returns>List of messages for all users.</returns>
        public List<string> GetAllMessageList()
        {
            return new List<string>(AllMessages);
        }

        /// <summary>
        /// Gets the message for a priv class. If none, returns null.
        /// </summary>
        /// <param name="privClass">Privclass to look for.</param>
        /// <returns>Message for a priv class. if none, returns null.</returns>
        public string GetPrivClassMessage(string privClass)
        {
            return PrivClassMessages.GetValueOrDefault(privClass.ToLower());
        }

        /// <summary>
        /// Sets the message for a priv class.
        /// </summary>
        /// <param name="privClass">Priv class to set message for.</param>
        /// <param name="message">Message to set.</param>
        public void SetPrivClassMessage(string privClass, string message)
        {
            PrivClassMessages[privClass.ToLower()] = message;
            RemoveNullValues(PrivClassMessages);
        }

        /// <summary>
        /// Gets the message for a user. If none, returns null.
        /// </summary>
        /// <param name="username">Username to look for.</param>
        /// <returns>Message for a user. If none, returns null.</returns>
        public string GetIndividualMessage(string username)
        {
            return IndividualMessages.GetValueOrDefault(username.ToLower());
        }

        /// <summary>
        /// Sets message for a user. 
        /// </summary>
        /// <param name="username">Username to set message for.</param>
        /// <param name="message">Message to set.</param>
        public void SetIndividualMessage(string username, string message)
        {
            IndividualMessages[username.ToLower()] = message;
            RemoveNullValues(IndividualMessages);
        }

        /// <summary>
        /// Gets a welcome message in a heirarchical manner. Checks individual 
        /// first, then privclass, then all messages. If none is found, returns null.
        /// The object must be enabled in order to retrieve messages. Likewise, 
        /// individual settings must be enabled for individual messages to work.
        /// </summary>
        /// <param name="username">username to look for.</param>
        /// <param name="privClass">privclass to look for.</param>
        /// <returns>Welcome message if there is one. Otherwise null.</returns>
        public string GetMessage(string username, string privClass)
        {
            if (!Enabled)
                return null;

            string message = null;

            // make args lower case so we don't have case-sensitivity
            username = username.ToLower();
            privClass = privClass.ToLower();

            // first get individiaul, then privclass, then all 
            if (AllowIndividualSettings && IndividualMessages.ContainsKey(username))
                message = IndividualMessages[username];
            else if (PrivClassMessages.ContainsKey(privClass))
                message = PrivClassMessages[privClass];
            else if (AllMessages.Count > 0)
                message = GetAllMessage();

            return message;
        }

        /// <summary>
        /// Deletes all welcome message data.
        /// </summary>
        public void Clear()
        {
            AllMessages.Clear();
            PrivClassMessages.Clear();
            IndividualMessages.Clear();
        }
    }

    /// <summary>
    /// Collection of welcome message setttings.
    /// </summary>
    [Serializable()]
    internal class WelcomeSettingsCollection : Dictionary<string, WelcomeSettings>
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public WelcomeSettingsCollection() { }

        /// <summary>
        /// Constructor to allow serialization.
        /// </summary>
        public WelcomeSettingsCollection(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Retrieves welcome message settings for chatroom. If there isn't any, creates one.
        /// </summary>
        /// <param name="chatroomName">Chatroom to check for.</param>
        /// <returns>Welcome message settings.</returns>
        public WelcomeSettings Get(string chatroomName)
        {
            if (!ContainsKey(chatroomName))
                this[chatroomName] = new WelcomeSettings(chatroomName);    
            return this[chatroomName];
        }
    }
}
