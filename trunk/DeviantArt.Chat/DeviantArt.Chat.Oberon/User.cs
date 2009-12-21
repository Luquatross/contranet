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
    }
}
