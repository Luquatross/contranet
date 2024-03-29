﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;
using DeviantArt.Chat.Oberon.Collections;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Plugin that allows the bot to be used for away messages.
    /// </summary>
    public class BotAway : Plugin
    {
        #region Private Variables and Properties
        private string _PluginName = "Bot Away";
        private string _FolderName = "Messaging";

        /// <summary>
        /// Away messages we will be storing.
        /// </summary>
        private RoomSettingCollection<string> AwayMessages
        {
            get
            {
                // retrieve away message from settings so that why are stored when 
                // the bot shuts down. if they haven't been created already, add them.
                if (!Settings.ContainsKey("AwayMessages"))
                {
                    Settings["AwayMessages"] = new RoomSettingCollection<string>(null);
                }
                return (RoomSettingCollection<string>)Settings["AwayMessages"];
            }
        }
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

        #region Helper Methods
        /// <summary>
        /// Displays bot away status to user.
        /// </summary>
        /// <param name="ns">Chatroom to show status in.</param>
        private void ShowStatus(string ns)
        {
            StringBuilder output = new StringBuilder();
            output.Append("<b><u>Current Away status</u></b>:<ul>");

            // get status for each chatroom we're signed into
            Chat[] chatrooms = Bot.GetAllChatrooms();
            foreach (Chat chat in chatrooms)
            {
                string respond = (string.IsNullOrEmpty(AwayMessages.Get(chat.Name)) ? "back" : "away");
                output.Append("<li>" + chat.Name + ". Status: " + respond + "</li>");
            }
            output.Append("</ul>");

            // send status
            Say(ns, output.ToString());
        }
        #endregion

        #region Plugin Methods
        public override void Load()
        {
            // tie into the chat event so we can detect messages sent to a user
            RegisterEvent(dAmnPacketType.Chat, new BotServerPacketEvent(ChatReceived));

            // register commands
            RegisterCommand("botaway", new BotCommandEvent(BotIsAway), new CommandHelp(
                "Allows the bot to be used for away messages.",
                "botaway status - shows what rooms the bot is set to be away in<br />" + 
                "botaway (#room | all) [away message] - bot will reply with away message when tabbed<br />" +                
                "<b>Example:<b> !botaway #botdom Browing the interwebs. Be back soon!"), (int)PrivClassDefaults.Owner);

            RegisterCommand("botback", new BotCommandEvent(BotIsBack), new CommandHelp(
                "Sets a bot so it is no longer away.",
                "botback (#room | all) - sets bot as back<br />" +
                "<b>Example:</b> !botback all"),
                (int)PrivClassDefaults.Owner);

            // load settings
            LoadSettings();
        }

        public override void Close()
        {
            SaveSettings();
        }
        #endregion

        #region Event & Command Handlers
        private void ChatReceived(string chatroom, dAmnServerPacket packet)
        {
            // get relevant data about the packet
            dAmnCommandPacket commandPacket = new dAmnCommandPacket(packet);
            string from = commandPacket.From;
            string message = commandPacket.Message;
            string username = Bot.Username;

            // make sure we didn't send a message! otherwise we would be replying to ourselves
            if (from.ToLower() == username.ToLower())
                return;

            // see if message is to us or not
            if (Utility.IsMessageToUser(message, username, MsgUsernameParse.Lazy))
            {
                string awayMessage = AwayMessages.Get(chatroom, username);
                if (!string.IsNullOrEmpty(awayMessage))
                {
                    Say(chatroom, "I am currently away. Reason: " + awayMessage);
                    return;
                }
            }
        }

        private void BotIsAway(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string room = ns;
            bool isGlobal = false;

            // show status 
            if (args.Length >= 1 && args[0] == "status")
            {
                ShowStatus(ns);
                return;
            }

            // get the room to respond to
            if (args.Length >= 1 && args[0].StartsWith("#"))
            {
                room = args[0];
                // remove the room name from the string and get new args
                int index = message.IndexOf(room);
                message = message.Substring(0, index) + message.Substring(index + room.Length + 1);
                args = GetArgs(message);
            }
            // determine if applies to all rooms or not
            else if (args.Length >= 1 && args[0] == "all")
            {
                isGlobal = true;

                // remove the 'all' string from the string and get new args
                int index = message.IndexOf("all");
                message = message.Substring(0, index) + message.Substring(index + "all".Length + 1);
                args = GetArgs(message);
            }

            // set chatroom away message
            if (isGlobal)
                AwayMessages.Set(message);
            else
                AwayMessages.Set(room, message);

            // let people know we are away
            if (isGlobal)
            {
                // notify all chatrooms
                Chat[] chatrooms = Bot.GetAllChatrooms();
                foreach (Chat chat in chatrooms)
                    Action(chat.Name, "is away: " + message);
            }
            else if (room == ns)
            {
                Action(ns, "is away: " + message);
            }
            else
            {
                // send action to appropriate room
                Action(room, "is away: " + message);

                // notify user in this room that command was successful
                Say(ns, string.Format("** away message set to: <b>{0}</b> for {1} *", message, room));
            }
        }

        private void BotIsBack(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string room = ns;
            bool isGlobal = false;

            // get the room to respond to
            if (args.Length >= 1 && args[0].StartsWith("#"))
            {
                room = args[0];
            }
            // determine if applies to all rooms or not
            else if (args.Length >= 1 && args[0] == "all")
            {
                isGlobal = true;
            }

            // clear away message
            if (isGlobal)
                AwayMessages.Clear();
            else
                AwayMessages.Set(room, null);

            // let people know we are back
            if (isGlobal)
            {
                // notify all chatrooms
                Chat[] chatrooms = Bot.GetAllChatrooms();
                foreach (Chat chat in chatrooms)
                    Action(chat.Name, "is back");
            }
            else if (room == ns)
            {
                Action(ns, "is back.");
            }
            else
            {
                // send action to appropriate room
                Action(room, "is back.");

                // notify user in this room that command was successful
                Say(ns, string.Format("** bot set back for {0} *", room));
            }
        }
        #endregion
    }
}
