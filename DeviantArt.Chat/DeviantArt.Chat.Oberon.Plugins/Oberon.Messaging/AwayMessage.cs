using System.Collections.Generic;
using DeviantArt.Chat.Library;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Plugin that allows users in a chatroom to use the bot to manage their away message.
    /// </summary>
    public class AwayMessage : Plugin
    {        
        #region Private Variables
        private string _PluginName = "Away Message";
        private string _FolderName = "Messaging";

        /// <summary>
        /// Away messages stored by the bot.
        /// </summary>
        private Dictionary<string, string> AwayMessages = new Dictionary<string, string>();        
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
            // tie into the chat event so we can detect messages sent to a user
            RegisterEvent(dAmnPacketType.Chat, new BotServerPacketEvent(ChatReceived));

            // register commands
            RegisterCommand("setaway", new BotCommandEvent(SetAway), new CommandHelp(
                "Allows users to set themselves away.",
                "setaway [away message]<br />" +
                "<b>Example:</b> !setaway Gone for a bit"), (int)PrivClassDefaults.Guests);

            RegisterCommand("setback", new BotCommandEvent(SetBack), new CommandHelp(
                "Sets a user so they are no longer away.",
                "setback"),
                (int)PrivClassDefaults.Guests);
        }
        #endregion        

        #region Command & Event Handlers
        private void ChatReceived(string chatroom, dAmnServerPacket packet)
        {
            // get relevant data about the packet
            dAmnCommandPacket commandPacket = new dAmnCommandPacket(packet);
            string from = commandPacket.From.ToLower();
            string message = commandPacket.Message;
            string owner = Bot.Username;

            // check to see if user is in our list
            foreach (KeyValuePair<string, string> away in AwayMessages)
            {
                // since the away key is room_username, have to get the username out of the key
                string username = away.Key.Substring(away.Key.IndexOf('_') + 1);

                // see if message contains the username
                if (Utility.IsMessageToUser(message, username, MsgUsernameParse.Lazy))
                {                         
                    Say(chatroom, username + " is currently away. Reason: <b><i>" + away.Value + "</i></b>");
                    return;
                }
            }
        }

        private void SetAway(string ns, string from, string message)
        {
            // make sure it isn't the bot sign on - that is for botaway command
            if (from.ToLower() == Bot.Username.ToLower())
                return;

            // make the key a combination of room and the user. this way, if the user
            // is set away in a different room, they'll get a different away message.
            string awayKey = ns + "_" + from;

            // add away message
            if (AwayMessages.ContainsKey(awayKey))
                AwayMessages[awayKey] = message;
            else
                AwayMessages.Add(awayKey, message);

            // let user know it worked
            Say(ns, from  + " away message set to: <b><i>" + message + "</i></b>");
        }

        private void SetBack(string ns, string from, string message)
        {
            string awayKey = ns + "_" + from;

            if (AwayMessages.ContainsKey(awayKey))
            {
                AwayMessages.Remove(awayKey);                                
                Say(ns, "<b><i>" + from + " is back.</i></b>");
            }
        }
        #endregion
    }
}
