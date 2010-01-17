using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;
using System.Text.RegularExpressions;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Plugin that allows operator to kick users who use banned words within the chatroom.
    /// </summary>
    public class BannedWords : Plugin
    {
        #region Private Variables
        private string _PluginName = "Banned Words";
        private string _FolderName = "Extras";

        /// <summary>
        /// Determines if banned words is turned on or not for the chatroom. Key is chatroom, 
        /// value is if it is enabled or not.
        /// </summary>
        private Dictionary<string, bool> BannedWordsEnabled
        {
            get
            {
                if (!Settings.ContainsKey("BannedWordsEnabled"))
                    Settings["BannedWordsEnabled"] = new Dictionary<string, bool>();
                return (Dictionary<string, bool>)Settings["BannedWordsEnabled"];
            }
            set
            {
                Settings["BannedWordsEnabled"] = value;
            }
        }

        /// <summary>
        /// Banned words list for chatroom. Key is chatroom, value is list of banned words.
        /// </summary>
        private Dictionary<string, List<string>> BannedWordList
        {
            get
            {
                if (!Settings.ContainsKey("BannedWords"))
                    Settings["BannedWords"] = new Dictionary<string, List<string>>();
                return (Dictionary<string, List<string>>)Settings["BannedWords"];
            }
            set
            {
                Settings["BannedWords"] = value;
            }
        }

        /// <summary>
        /// Messages that are sent to kicked users. Key is chatroom, value is kick message.
        /// </summary>
        private Dictionary<string, string> KickMessage
        {
            get
            {
                if (!Settings.ContainsKey("KickMessage"))
                    Settings["KickMessage"] = new Dictionary<string, string>();
                return (Dictionary<string, string>)Settings["KickMessage"];
            }
            set
            {
                Settings["KickMessage"] = value;
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
        private bool IsBannedWordsEnabled(string chatroom)
        {
            if (BannedWordsEnabled.ContainsKey(chatroom))
                return BannedWordsEnabled[chatroom];
            else
                return false;
        }

        private bool ContainsBannedWord(string chatroom, string message)
        {
            if (!BannedWordList.ContainsKey(chatroom))
                return false;
            List<string> bannedList = BannedWordList[chatroom];
            MatchCollection matches = StringHelper.HasWords(message, bannedList.ToArray());
            return (matches.Count > 0);
        }

        private bool IsWordBanned(string chatroom, string word)
        {
            if (!BannedWordList.ContainsKey(chatroom))
                return false;
            return BannedWordList[chatroom].Contains(word);
        }

        private void AddBannedWord(string chatroom, string word)
        {
            if (!BannedWordList.ContainsKey(chatroom))
                BannedWordList.Add(chatroom, new List<string>());
            BannedWordList[chatroom].Add(word);
        }
        #endregion

        #region Plugin Methods
        public override void Load()
        {
            // register event
            RegisterEvent(dAmnPacketType.Chat, new BotServerPacketEvent(ChatReceived));

            // register command
            RegisterCommand("banwords", new BotCommandEvent(BanWords), new CommandHelp(
                "Allows managing a list of words that gets users kick from a channel. <i>Note: the bot has to have +kick privs for this module to fully work.</i>",
                "banwords (#room) add [word] - adds [word] to the banned words list<br />" +
                "banwords (#room) remove [word] - deletes [word] from the list<br />" +
                "banwords (#room | all) clearwords - clears banned words<br />" +
                "banwords (#room | all) clearmessage - clears /kick messages<br />" +
                "banwords (#room) list - lists banned words for chatroom<br />" + 
                "banwords [on / off] - turns ban words on or off for the chatroom<br />" +
                "banwords status - lists which rooms have banwords turned on or off<br />" +
                "<i>Optional parameter \"room\" always defaults to the channel you are in.</i>"),
                (int)PrivClassDefaults.Operators);

            // load settings
            LoadSettings();
        }

        public override void Close()
        {
            // save settings
            SaveSettings();            
        }
        #endregion

        #region Event & Command Handlers
        private void ChatReceived(string chatroom, dAmnServerPacket packet)
        {
            // if room isn't enabled, don't bother processing
            if (!IsBannedWordsEnabled(chatroom))
                return;

            // get relevant data about the packet
            dAmnCommandPacket commandPacket = new dAmnCommandPacket(packet);

            // see if message contains a banned word
            if (ContainsBannedWord(chatroom, commandPacket.Message))
            {
                // get kick message if there is one
                string kickMessage = string.Empty;
                if (KickMessage.ContainsKey(chatroom))
                    kickMessage = KickMessage[chatroom];

                // kick message
                dAmn.Kick(chatroom, commandPacket.From, kickMessage);
            }
        }

        public void BanWords(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string room = ns;
            bool isGlobal = false;

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

            string subCommand = GetArg(args, 0);
            string var1 = GetArg(args, 1);
            string var2 = GetArg(args, 2);
            string word = string.Empty;

            switch (subCommand)
            {
                case "add":
                    word = ParseArg(message, 1);
                    if (string.IsNullOrEmpty(word))
                    {
                        Respond(ns, from, "Please provide a word(s) to ban.");
                        return;
                    }
                    else if (IsWordBanned(room, word))
                    {
                        Respond(ns, from, "This word is already banned.");
                        return;
                    }
                    AddBannedWord(room, word);
                    Respond(ns, from, string.Format("\"{0}\" has been added to the ban list for {1}.", word, room));
                    return;
                case "remove":
                    word = ParseArg(message, 1);
                    if (string.IsNullOrEmpty(word))
                    {
                        Respond(ns, from, "Please provide a word(s) to remove.");
                        return;
                    }
                    else if (!IsWordBanned(room, word))
                    {
                        Respond(ns, from, "This word is not in the ban list.");
                        return;
                    }
                    BannedWordList[room].Remove(word);
                    Respond(ns, from, string.Format("\"{0}\" has been removed from the ban list for {1}.", word, room));
                    return;
                case "clearwords":
                    if (isGlobal)
                    {
                        if (BannedWordList.Count == 0)
                            Respond(ns, from, "There are no banned words to clear.");
                        else
                        {
                            BannedWordList.Clear();
                            Respond(ns, from, "All banned words have been deleted.");
                        }
                    }
                    else
                    {
                        if (!BannedWordList.ContainsKey(room) || BannedWordList[room].Count == 0)
                            Respond(ns, from, "There are no banned words for " + room);
                        else
                        {
                            BannedWordList[room].Clear();
                            Respond(ns, from, "Banned words in " + room + " have been deleted.");
                        }
                    }
                    return;
                case "clearmessage":
                    if (isGlobal)
                    {
                        if (KickMessage.Count == 0)
                            Respond(ns, from, "There are no kick messages to clear.");
                        else
                        {
                            KickMessage.Clear();
                            Respond(ns, from, "All kick messages have been cleared.");
                        }
                    }
                    else
                    {
                        if (!KickMessage.ContainsKey(room))
                            Respond(ns, from, "There is not a kick message for " + room);
                        else
                        {
                            KickMessage.Remove(room);
                            Respond(ns, from, "Kick message for " + room + " has been deleted.");
                        }
                    }
                    return;
                case "list":
                    if (!BannedWordList.ContainsKey(room) || BannedWordList[room].Count == 0)
                        Respond(ns, from, "No banned words were found.");
                    else
                    {
                        StringBuilder list = new StringBuilder(string.Format("<b><u>Banned words for {0}</u></b>:<sub><ul>", room));
                        foreach (string w in BannedWordList[room])                        
                            list.Append("<li>" + w + "</li>");
                        list.Append("</ul></sub>");
                        Respond(ns, from, list.ToString());
                    }
                    return;
                case "message":
                    string kickMsg = ParseArg(message, 1);
                    if (string.IsNullOrEmpty(kickMsg))
                    {
                        Respond(ns, from, "You must provide a message.");
                        return;
                    }
                    if (KickMessage.ContainsKey(room))
                        KickMessage[room] = kickMsg;
                    else
                        KickMessage.Add(room, kickMsg);
                    Respond(ns, from, string.Format("Kick message set to '<i>{0}</i>' for {1}.", kickMsg, room));
                    return;
                case "on":
                    if (BannedWordsEnabled.ContainsKey(room))
                        BannedWordsEnabled[room] = true;
                    else
                        BannedWordsEnabled.Add(room, true);
                    Respond(ns, from, "Banned words is set to 'on' for " + room);
                    return;
                case "off":
                    if (BannedWordsEnabled.ContainsKey(room))
                        BannedWordsEnabled[room] = false;
                    else
                        BannedWordsEnabled.Add(room, false);
                    Respond(ns, from, "Banned words is set to 'off' for " + room);
                    return;
                case "status":
                    if (BannedWordsEnabled.Count == 0)
                        Respond(ns, from, "Banwords is not enabled in any chatrooms.");
                    else
                    {
                        StringBuilder enabledList = new StringBuilder(string.Format("<b><u>Banned words status</u></b>:<sub><ul>"));
                        foreach (KeyValuePair<string, bool> item in BannedWordsEnabled)
                            enabledList.AppendFormat("<li>{0} - Enabled: {1}</li>", item.Key, item.Value);
                        enabledList.Append("</ul></sub>");
                        Respond(ns, from, enabledList.ToString());
                    }
                    return;
                default:
                    ShowHelp(ns, from, "banwords");
                    return;
            }
        }
        #endregion
    }
}
