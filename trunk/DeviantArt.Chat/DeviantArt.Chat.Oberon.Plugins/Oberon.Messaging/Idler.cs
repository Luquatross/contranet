using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using DeviantArt.Chat.Library;
using DeviantArt.Chat.Oberon.Collections;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Action to take after idle time expires.
    /// </summary>
    enum IdlerAction
    {
        /// <summary>
        /// The action has not been configured.
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Send the user a message after idle time has expired.
        /// </summary>
        Message = 1,

        /// <summary>
        /// Send the user a message after idle time has expired. If idle time 
        /// expires again, kick the user from the room.
        /// </summary>
        MessageThenKick = 2
    }

    /// <summary>
    /// Holds data about a user's idle time in the chatroom.
    /// </summary>
    class IdleData
    {
        /// <summary>
        /// Number of messages that have been sent to the user.
        /// </summary>
        public int NumberOfMessages { get; set; }

        /// <summary>
        /// The last time a user has chatted in the room.
        /// </summary>
        public DateTime TimeOfLastChat { get; set; }

        /// <summary>
        /// Constructor. 
        /// </summary>
        public IdleData()
        {
            Reset();
        }

        /// <summary>
        /// Resets idle time data.
        /// </summary>
        public void Reset()
        {
            TimeOfLastChat = DateTime.Now;
            NumberOfMessages = 0;
        }
    }

    /// <summary>
    /// Collection to hold idle data for users in a chatroom.
    /// </summary>
    class IdleDictionary : Dictionary<string, IdleData>
    {
        /// <summary>
        /// Returns a key idle data.
        /// </summary>
        /// <param name="room">Chatroom.</param>
        /// <param name="username">Username.</param>
        /// <returns></returns>
        public static string CalculateKey(string room, string username)
        {
            string key = room + "_" + username;
            Chat chat = Bot.Instance.GetChatroom(room);
            if (chat.ContainsUser(username))
                key = key + "_" + chat[username].Count;
            else
                key = key + "_0";

            return key;
        }
    }

    /// <summary>
    /// Idle settings for a chatroom.
    /// </summary>
    [Serializable]
    class IdleRoomSettings
    {
        /// <summary>
        /// Random number generator.
        /// </summary>
        private Random Randomizer = new Random();

        /// <summary>
        /// True if enabled, otherwise false.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// List of messages to send idling user.
        /// </summary>
        public List<string> IdleMessages { get; set; }

        /// <summary>
        /// List of messages to send to a user who has been kicked.
        /// </summary>
        public List<string> KickMessages { get; set; }

        /// <summary>
        /// Maximum amount of time a user can idle before getting
        /// messaged or kicked.
        /// </summary>
        public TimeSpan MaxIdleTime { get; set; }

        /// <summary>
        /// Type of action to perform in this chatrom.
        /// </summary>
        public IdlerAction Action { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public IdleRoomSettings()
        {
            // init variables to default
            Enabled = false;
            IdleMessages = new List<string>();
            KickMessages = new List<string>();
            MaxIdleTime = TimeSpan.MaxValue;
            Action = IdlerAction.NotSet;
        }

        /// <summary>
        /// Gets a random idle message.
        /// </summary>
        /// <returns>A random idle message.</returns>
        public string GetRandomIdleMessage()
        {
            if (IdleMessages.Count == 0)
                return string.Empty;
            else
                return IdleMessages[Randomizer.Next(IdleMessages.Count)];
        }

        /// <summary>
        /// Gets a random kick message.
        /// </summary>
        /// <returns>A random kick message.</returns>
        public string GetRandomKickMessage()
        {
            if (KickMessages.Count == 0)
                return string.Empty;
            else
                return KickMessages[Randomizer.Next(KickMessages.Count)];
        }
    }

    /// <summary>
    /// Collection of idle room settings.
    /// </summary>
    [Serializable]
    class IdleRoomSettingCollection : Dictionary<string, IdleRoomSettings>
    { }

    // Plugin to allow room administrators to perform an action on users who idle for too long.
    class Idler : Plugin
    {
        #region Private Variables
        private const string _PluginName = "Idler";
        private const string _FolderName = "Messaging";

        /// <summary>
        /// Idle timer.
        /// </summary>
        private Timer IdlerTimer;

        /// <summary>
        /// Dictionary to keep track of idle time.
        /// </summary>
        private IdleDictionary IdleTracking = new IdleDictionary();

        /// <summary>
        /// Idle room settings for all chatrooms.
        /// </summary>
        private IdleRoomSettingCollection IdleRoomSettings
        {
            get
            {
                // retrieve idle settings so that they are stored when 
                // the bot shuts down. if they haven't been created already, add them.
                if (!Settings.ContainsKey("IdleRoomSettings"))
                {
                    Settings["IdleRoomSettings"] = new IdleRoomSettingCollection();
                }
                return (IdleRoomSettingCollection)Settings["IdleRoomSettings"];
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
        private void TimerElapsed(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, IdleData> item in IdleTracking)
            {
                // get values from string, it is in the format [chatoom]_[username]_[usercount]
                string[] tokens = item.Key.Split('_');
                string room = tokens[0];
                string user = tokens[1];

                // get idle data
                IdleData idleData = item.Value;

                // get settings for this room - if they don't exist, skip it
                IdleRoomSettings settings = IdleRoomSettings.GetValueOrDefault(room, null);
                if (settings == null)
                    continue;

                // if idling is enabled and enough time has elapsed
                if (settings.Enabled && DateTime.Now - idleData.TimeOfLastChat > settings.MaxIdleTime)
                {
                    string message = null;
                    switch (settings.Action)
                    {
                        case IdlerAction.Message:
                            // message user
                            message = Utility.FormatMessage(settings.GetRandomIdleMessage(), room, user);
                            Say(room, message);
                            break;
                        case IdlerAction.MessageThenKick:
                            // see if user has been messaged before
                            // if they have, kick them
                            if (idleData.NumberOfMessages > 0)
                            {
                                message = Utility.FormatMessage(settings.GetRandomKickMessage(), room, user);
                                dAmn.Kick(room, user, message);

                                // reset user
                                idleData.Reset();
                            }
                            // if they haven't message them
                            else
                            {
                                // send message
                                message = Utility.FormatMessage(settings.GetRandomIdleMessage(), room, user);
                                Say(room, message);

                                // mark as having been sent
                                idleData.NumberOfMessages++;
                            }
                            break;
                        default:
                            // action has not been set
                            break;
                    }
                }
            }
        }

        private string GetIdleActionString(IdlerAction action)
        {
            switch (action)
            {
                case IdlerAction.NotSet:
                    return "not set";
                case IdlerAction.Message:
                    return "message";
                case IdlerAction.MessageThenKick:
                    return "message then kick";
                default:
                    Exception ex = new Exception(string.Format("Idle action does not have a string associated with it."));
                    ex.Data.Add("IdleAction", action);
                    throw ex;
            }
        }

        private void RegisterExistingUsers()
        {
            // get all chatrooms
            Chat[] chats = Bot.GetAllChatrooms();
            foreach (Chat chat in chats)
            {
                // get all users in this room
                User[] users = chat.GetAllMembers();
                foreach (User user in users)
                {
                    // keep track of user idle time
                    string key = IdleDictionary.CalculateKey(chat.Name, user.Username);
                    if (!IdleTracking.ContainsKey(key))
                        IdleTracking.Add(key, new IdleData());
                }
            }
        }
        #endregion

        #region Plugin Methods
        public override void Load()
        {
            // run the timer every minute
            IdlerTimer = new Timer(TimeSpan.FromMinutes(1.00).TotalMilliseconds);
            IdlerTimer.Enabled = true;
            IdlerTimer.AutoReset = true;
            IdlerTimer.Elapsed += TimerElapsed;            

            // tie into the join, part and chat event
            RegisterEvent(dAmnPacketType.MemberJoin, new BotServerPacketEvent(UserJoined));
            RegisterEvent(dAmnPacketType.MemberPart, new BotServerPacketEvent(UserParted));
            RegisterEvent(dAmnPacketType.Chat, new BotServerPacketEvent(UserChatted));

            // register commands
            RegisterCommand("idle", new BotCommandEvent(IdleCommand), new CommandHelp(
                "Manage chatroom idle settings.",
                "idle [on / off] - Turn idle messages on or off.<br />" + 
                "idle settings - View the current settings for idle messages<br />" + 
                "idle list - List all idle messages and kick messages<br />" + 
                "idle action [1 / 2] - Set idle action to message user (1) or message then kick (2).<br />" + 
                "idle time [num in minutes] - Sets the max idle time before performing an action.<br />" + 
                "idle add [message] - Adds an idle message.<br />" + 
                "idle add-kick [message] - Adds a kick message.<br />" + 
                "idle remove [index] - index of idle message to remove<br />" +
                "idle remove-kick [index] - index of kick message to remove<br />"), (int)PrivClassDefaults.Operators);
        }

        public override void Close()
        {
            // save settings
            SaveSettings();

            // kill timer
            IdlerTimer.Stop();
            IdlerTimer.Dispose();            
        }

        public override void Activate()
        {
            // users who are already in chatrooms need to be tracked
            RegisterExistingUsers();

            // start timer here, when we're activated, so when plugin isn't activated
            // we're not running the timer anyway.
            IdlerTimer.Start();
        }

        public override void Deactivate()
        {
            // when someone turns off this plugin, stop timer 
            IdlerTimer.Stop();

            // clear tracking dictionary
            IdleTracking.Clear();
        }

        public override void Restart()
        {
            // when bot restarts, we don't want to be using old data
            this.Deactivate();
            this.Activate();
        }        
        #endregion

        #region Event Handlers
        private void UserJoined(string chatroom, dAmnServerPacket packet)
        {
            // get data from packet
            dAmnCommandPacket command = new dAmnCommandPacket(packet);

            // add user to our idle tracking collection
            string key = IdleDictionary.CalculateKey(chatroom, command.From);
            if (!IdleTracking.ContainsKey(key))
            {
                IdleTracking.Add(key, new IdleData());
            }
        }

        private void UserParted(string chatroom, dAmnServerPacket packet)
        {
            // get data from the packet
            dAmnCommandPacket command = new dAmnCommandPacket(packet);

            // remove user from our idle tracking collection
            string key = IdleDictionary.CalculateKey(chatroom, command.From);
            if (IdleTracking.ContainsKey(key))
            {
                IdleTracking.Remove(key);
            }
        }

        private void UserChatted(string chatroom, dAmnServerPacket packet)
        {
            // get data from the packet
            dAmnCommandPacket command = new dAmnCommandPacket(packet);

            // reset user in our idle tracking collection
            string key = IdleDictionary.CalculateKey(chatroom, command.From);
            if (IdleTracking.ContainsKey(key))
            {
                IdleTracking[key].Reset();
            }
        }
        #endregion

        #region Commands
        private void IdleCommand(string ns, string from, string message)
        {
            // we'll be setting at least some value for the idle settings, so create
            // a new instance since there isn't one
            if (!IdleRoomSettings.ContainsKey(ns))
            {                
                IdleRoomSettings.Add(ns, new IdleRoomSettings());
            }
            IdleRoomSettings settings = IdleRoomSettings[ns];

            // get command args
            string[] args = GetArgs(message);
            string subCommand = GetArg(args, 0);
            string var1 = GetArg(args, 1);
            string var2 = GetArg(args, 2);
            string var3 = GetArg(args, 3);

            // response to send 
            StringBuilder say = new StringBuilder();

            switch (subCommand)
            {
                case "on":
                    settings.Enabled = true;
                    say.Append("** Idle messages have been turned on. *");
                    break;
                case "off":
                    settings.Enabled = false;
                    say.Append("** Idle messages have been turned off. *");
                    break;
                case "settings":
                    say.Append("<u><b>Idle settings</b></u>:<sub><br />");
                    say.AppendFormat("enabled : <b>{0}</b><br />", settings.Enabled);
                    say.AppendFormat("time : <b>{0} minute(s)</b><br />", Math.Round(settings.MaxIdleTime.TotalMinutes));
                    say.AppendFormat("action : <b>{0}</b><br />", GetIdleActionString(settings.Action));
                    say.AppendFormat("msgs : <b>{0} msgs, {1} kick msgs</b><br />", settings.IdleMessages.Count, settings.KickMessages.Count);                    
                    break;
                case "list":
                    say.Append("<u><b>Idle messages:</b></u>:<sub><br />");
                    if (settings.IdleMessages.Count == 0)
                    {
                        say.Append("There are no messages.");
                    }
                    else
                    {
                        for (int i = 0; i < settings.IdleMessages.Count; i++)
                            say.AppendFormat("[{0}] - {1}<br />", i, settings.IdleMessages[i]);
                    }
                    say.Append("</sub><br /><u><b>Kick messages:</b></u>:<sub><br />");
                    if (settings.KickMessages.Count == 0)
                    {
                        say.Append("There are no messages.");
                    }
                    else
                    {
                        for (int i = 0; i < settings.KickMessages.Count; i++)
                            say.AppendFormat("[{0}] - {1}<br />", i, settings.KickMessages[i]);
                    }
                    break;
                case "action":
                    int action = -1;
                    bool actionParseResult = int.TryParse(var1, out action);
                    if (actionParseResult == false || (action != 1 && action != 2))
                    {
                        Respond(ns, from, "You must provide the type of action, either 1 or 2.");
                        return;
                    }
                    settings.Action = (IdlerAction)action;
                    say.AppendFormat("Idle action has been set to '{0}'.", GetIdleActionString(settings.Action));
                    break;
                case "time":
                    int time = -1;
                    bool timeParseResult = int.TryParse(var1, out time);
                    if (timeParseResult == false || time <= 0)
                    {
                        Respond(ns, from, "Invalid time. Please specify a non-zero max idle time in minutes.");
                        return;
                    }
                    settings.MaxIdleTime = TimeSpan.FromMinutes(time);
                    say.AppendFormat("Idle time has been set to {0} minute(s).", time);
                    break;
                case "add":
                    string addMsg = ParseArg(message, 1);
                    if (string.IsNullOrEmpty(addMsg))
                    {
                        Respond(ns, from, "Please provide an idle message.");
                        return;
                    }
                    settings.IdleMessages.Add(addMsg);
                    say.Append("New message has been added to idle messages successfully.");
                    break;
                case "add-kick":
                    string addKickMsg = ParseArg(message, 1);
                    if (string.IsNullOrEmpty(addKickMsg))
                    {
                        Respond(ns, from, "Please provide a kick message.");
                        return;
                    }
                    settings.KickMessages.Add(addKickMsg);
                    say.Append("New message has been added to kick messages successfully.");
                    break;
                case "remove":
                    int removeIndex = -1;
                    bool removeIndexResult = int.TryParse(var1, out removeIndex);
                    if (removeIndexResult == false || removeIndex < 0 || removeIndex >= settings.IdleMessages.Count)
                    {
                        Respond(ns, from, "Invalid index!");
                        return;
                    }
                    settings.IdleMessages.RemoveAt(removeIndex);
                    say.AppendFormat("Idle message at index {0} was successfully removed.", removeIndex);
                    break;
                case "remove-kick":
                    int removeIndex2 = -1;
                    bool removeIndexResult2 = int.TryParse(var1, out removeIndex2);
                    if (removeIndexResult2 == false || removeIndex2 < 0 || removeIndex2 >= settings.KickMessages.Count)
                    {
                        Respond(ns, from, "Invalid index!");
                        return;
                    }
                    settings.KickMessages.RemoveAt(removeIndex2);
                    say.AppendFormat("Kick message at index {0} was successfully removed.", removeIndex2);
                    break;
                default:
                    ShowHelp(ns, from, "idle");
                    return;
            }

            // send to user!
            SaveSettings();
            Say(ns, say.ToString());
        }
        #endregion
    }
}
