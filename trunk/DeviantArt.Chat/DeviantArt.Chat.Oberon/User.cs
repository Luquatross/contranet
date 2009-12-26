using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// Class the represents a user in the chatroom.
    /// </summary>
    [Serializable()]
    public class User
    {
        /// <summary>
        /// The deviantart username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Chatroom privclass.
        /// </summary>
        public string PrivClass { get; set; }

        /// <summary>
        /// Symbol for user. For example: ~.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Deviant's real name.
        /// </summary>
        public string Realname { get; set; }

        /// <summary>
        /// The artists type. For example: fantasy artist.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Server priv class. Most times will be "guest".
        /// </summary>
        public string ServerPrivClass { get; set; }

        /// <summary>
        /// Number of users in chatroom of the same username.
        /// </summary>
        public int Count = 0;

        /// <summary>
        /// Access level for this user. If not set, returns the default user 
        /// access level.
        /// </summary>
        public int AccessLevel
        {
            get
            {
                return Bot.Instance.Access.GetUserLevel(Username);
            }
        }

        /// <summary>
        /// Returns true if the user as access to the command. Otherwise false.
        /// </summary>
        /// <param name="command">Command to test access to.</param>
        /// <returns>True if authorized, otherwise false.</returns>
        public bool HasAccess(string command)
        {
            return (AccessLevel >= Bot.Instance.Access.GetCommandLevel(command));
        }
    }
}
