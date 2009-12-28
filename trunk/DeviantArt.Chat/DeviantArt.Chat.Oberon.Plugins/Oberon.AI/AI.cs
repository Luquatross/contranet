﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;

namespace DeviantArt.Chat.Oberon.Plugins
{
    public class AI : Plugin
    {
        #region Private Variables
        // Plugin variables
        private string _PluginName = "Oberon AI";
        private string _FolderName = "AI";

        /// <summary>
        /// Instance of AIMLBot.
        /// </summary>
        private AIMLbot.Bot AimlBot;

        /// <summary>
        /// String representing owner plug ":".
        /// </summary>
        private string OwnerString;

        /// <summary>
        /// Local cache of AIMLBot users.
        /// </summary>
        private Dictionary<string, AIMLbot.User> Users = new Dictionary<string, AIMLbot.User>();

        /// <summary>
        /// Path to the setting files.
        /// </summary>
        private string BotSettingsFilePath
        {
            get { return System.IO.Path.Combine(PluginPath, "config\\Settings.xml"); }
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

        /// <summary>
        /// Constructor;
        /// </summary>
        public AI()
        {
            // this has an unexpected value - must set to aimlbot can find
            // it's path correctly.
            Environment.CurrentDirectory = PluginPath;

            // init chatbot
            AimlBot = new AIMLbot.Bot();
            AimlBot.loadSettings(BotSettingsFilePath);
            AimlBot.loadAIMLFromFiles();

            // get owner string
            OwnerString = Bot.Owner + ":";
        }

        /// <summary>
        /// Gets string with the username stripped from the front.
        /// </summary>
        /// <param name="message">Message to parse.</param>
        /// <returns>String ready to send to chatbot.</returns>
        private string GetInput(string message)
        {
            return message.Replace(OwnerString, "").Trim();
        }

        /// <summary>
        /// Get AIML user. Cached once created.
        /// </summary>
        /// <param name="username">Username of user.</param>
        /// <returns>AIMLBot user.</returns>
        private AIMLbot.User GetUser(string username)
        {
            if (Users.ContainsKey(username))
            {
                return Users[username];
            }
            else
            {
                AIMLbot.User u = new AIMLbot.User(username, AimlBot);
                Users.Add(username, u);
                return u;
            }
        }

        public override void Load()
        {
            RegisterEvent(dAmnPacketType.Chat, new BotServerPacketEvent(ChatReceived));
        }

        private void ChatReceived(string chatroom, dAmnServerPacket packet)
        {
            string from;
            string message;
            string target;
            string eventResponse;
            string owner = Bot.Owner;
            bool toBot;

            // get details about the packet
            dAmnServerPacket.SortdAmnPacket(packet, out from, out message, out target, out eventResponse);

            // get the chatroom. if null bot isn't signed into this chatroom, 
            // and we're not sure where this came from
            Chat chat = Bot.GetChatroom(chatroom);
            if (chatroom == null)
                return;

            // determine if the message was directed at us
            toBot = (message.StartsWith(owner + ":"));
            if (!chat.ContainsUser(Bot.Owner) && toBot)
            {
                // ensure that the real use isn't signed on
                if (chat[owner].Count != 1)
                    return;

                string input = GetInput(message);

                // form request
                AIMLbot.Request request = new AIMLbot.Request(
                    input,
                    GetUser(from),
                    AimlBot);
                // get the response
                AIMLbot.Result reply = AimlBot.Chat(request);

                // send it to the client!
                Respond(chatroom, from, reply.Output);
            }
        }
    }
}
