using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Oberon;
using DeviantArt.Chat.Library;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Base class for notify classes. Takes care of detecting if a message was sent 
    /// to a user and loading and saving settings.
    /// </summary>
    public abstract class NotifyBase : Plugin
    {
        #region Protected Variables
        protected const string _FolderName = "Messaging";
        protected const string _Username = "deviant@thehomeofjon.net";
        protected const string _Password = "batman";
        #endregion

        #region Protected Properties
        /// <summary>
        /// Username to use to sign onto IM network.
        /// </summary>
        protected string Username
        {
            get
            {
                if (!Settings.ContainsKey("Username"))
                    return _Username;
                else
                    return (string)Settings["Username"];
            }
        }

        /// <summary>
        /// Password to use to sign onto IM network.
        /// </summary>
        protected string Password
        {
            get
            {
                if (!Settings.ContainsKey("Password"))
                    return _Password;
                else
                    return (string)Settings["Password"];
            }
        }

        /// <summary>
        /// Returns true if the plugin has been configured (username and password set).
        /// Otherwise false.
        /// </summary>
        protected bool IsConfigured
        {
            get
            {
                return (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password));
            }
        }

        /// <summary>
        /// List of people who want to be notified of messages.
        /// The keuyis their dA username and the value is their
        /// IM username.
        /// </summary>
        protected Dictionary<string, string> NotifyList
        {
            get
            {
                if (!Settings.ContainsKey("NotifyList"))
                    Settings["NotifyList"] = new Dictionary<string, string>();
                return (Dictionary<string, string>)Settings["NotifyList"];
            }
            set
            {
                Settings["NotifyList"] = value;
            }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Folder name
        /// </summary>
        public override string FolderName
        {
            get { return _FolderName; }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Sends a message to the provided username on the IM network.
        /// </summary>
        /// <param name="username">Username to send to.</param>
        /// <param name="chatroom">Chatroom message came from.</param>
        /// <param name="from">Person who sent the message.</param>
        /// <param name="message">Message to send.</param>
        /// <returns>True if the send was successful, otherwise false.</returns>
        protected abstract bool Send(string username, string chatroom, string from, string message);

        /// <summary>
        /// Adds a user to be notified.
        /// </summary>
        /// <param name="username">DeviantArt username to add.</param>
        /// <param name="imUsername">IM Network username.</param>
        protected void AddNotification(string username, string imUsername)
        {
            NotifyList.Add(username, imUsername);
        }

        /// <summary>
        /// Removes a user from the notification list.
        /// </summary>
        /// <param name="username">DeviantArt username to remove.</param>
        protected void RemoveNotification(string username)
        {
            NotifyList.Remove(username);
        }

        /// <summary>
        /// True if user is in notify list, otherwise false.
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <returns>True if user is in notify list, otherwise false.</returns>
        protected bool IsUserInList(string username)
        {
            return NotifyList.ContainsKey(username);
        }
        #endregion

        #region Plugin Methods
        /// <summary>
        /// Loads the plugin settings.
        /// </summary>
        public override void Load()
        {
            // register event handler
            RegisterEvent(dAmnPacketType.Chat, ChatReceived);

            // load our settings
            LoadSettings();
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        public override void Close()
        {
            SaveSettings();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Checks each message and sees if we should forward it on to a user.
        /// </summary>        
        protected void ChatReceived(string chatroom, dAmnServerPacket packet)
        {
            // if we're not configured, abort
            if (!IsConfigured)
                return;

            dAmnCommandPacket p = new dAmnCommandPacket(packet);
            string username;

            // see if we have a message to a user in our list
            if (Utility.IsMessageToUserInList(p.Message, NotifyList.Keys.ToList(), MsgUsernameParse.Lazy, out username))
            {
                Send(username, chatroom, p.From, p.Message);
            }
        }
        #endregion
    }
}
