using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Class to hold visit count data. Use HashSet because it doesn't 
    /// allow duplicate data. That way we can add usernames to it and
    /// not care if we've seen them or not.
    /// </summary>
    [Serializable]
    class VisitCountCollection : Dictionary<string, HashSet<string>>
    { }

    /// <summary>
    /// Plugin to keep track of new username's seen in chatroom since bot logs in.
    /// </summary>
    public class VisitCount : Plugin
    {
        #region Private Variables
        private string _PluginName = "VisitCount";
        private string _FolderName = "Extras";
        private VisitCountCollection VisitCounts = new VisitCountCollection();
        #endregion

        #region Public Properties
        public override string PluginName
        {
            get { return _PluginName; }
        }

        public override string FolderName
        {
            get { return _FolderName; }
        }
        #endregion

        #region Plugin Methods
        public override void Load()
        {
            // register our events with the bot
            RegisterEvent(dAmnPacketType.Join, ChatroomJoined);
            RegisterEvent(dAmnPacketType.Part, ChatroomPart);
            RegisterEvent(dAmnPacketType.MemberJoin, UserJoined);

            // register our comment with bot
            RegisterCommand("visitcount", new BotCommandEvent(VisitCountCmd), new CommandHelp(
                "Shows the number of new users seen in the room since the bot logged in.",
                "visitcount (#room) - Show visit count.<br />" +
                "<b>Example:</b> !visitcount"), (int)PrivClassDefaults.Guests);
        }

        public override void Restart()
        {
            VisitCounts.Clear();
        }
        #endregion

        #region Event Handlers
        private void ChatroomJoined(string chatroom, dAmnServerPacket packet)
        {
            if (!VisitCounts.ContainsKey(chatroom))
                VisitCounts.Add(chatroom, new HashSet<string>());
        }

        private void ChatroomPart(string chatroom, dAmnServerPacket packet)
        {
            if (VisitCounts.ContainsKey(chatroom))
                VisitCounts.Remove(chatroom);
        }

        private void UserJoined(string chatroom, dAmnServerPacket packet)
        {            
            dAmnCommandPacket commandPacket = new dAmnCommandPacket(packet);

            if (VisitCounts.ContainsKey(chatroom))
                VisitCounts[chatroom].Add(commandPacket.From);
        }
        #endregion

        #region Command Handlers
        private void VisitCountCmd(string ns, string from, string message)
        {
            string room = ns;
            if (!string.IsNullOrEmpty(message) && message.StartsWith("#"))
                room = message.Trim();

            if (VisitCounts.ContainsKey(room))
            {
                Say(ns, string.Format("** {0} new users seen. *", VisitCounts[room].Count)); 
            }
            else
            {
                Respond(ns, from, string.Format("The bot is not signed into the chatroom '{0}'.", room));
            }            
        }
        #endregion
    }
}
