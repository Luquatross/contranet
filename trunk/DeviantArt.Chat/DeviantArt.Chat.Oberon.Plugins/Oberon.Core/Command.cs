using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// This class encapsulates all internal Oberon commands. Whereas the dAmnCore.cs class allows 
    /// basic functionality in the bot, this class enables non-built-in dAmn functionality.
    /// </summary>
    public class Command : CorePluginBase
    {
        #region Private Variables
        private string _PluginName = "Oberon Commands Plugin";
        private string _FolderName = "System";
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
        /// <summary>
        /// Returns our Resource Manager so that we can can use the GetCommandHelp method for 
        /// retrieving help strings from our resource file.
        /// </summary>
        /// <returns></returns>
        protected override ResourceManager GetResourceManager()
        {
            return PluginResource.ResourceManager;
        }

        /// <summary>
        /// Registers all our internal commands, their help texts, and their default privileges.
        /// </summary>
        public override void Load()
        {
            base.Load();

            // register comamnds
            RegisterCommand("trigcheck", new BotCommandEvent(TrigCheck), GetCommandHelp("TrigCheck.Summary", "TrigCheck.Usage"), (int)PrivClassDefaults.Guests);
            RegisterCommand("help", new BotCommandEvent(Help), GetCommandHelp("Help.Summary", "Help.Usage"), (int)PrivClassDefaults.Guests);
            RegisterCommand("time", new BotCommandEvent(Time), GetCommandHelp("Time.Summary", "Time.Usage"), (int)PrivClassDefaults.Guests);
            RegisterCommand("about", new BotCommandEvent(About), GetCommandHelp("About.Summary", "About.Usage"), (int)PrivClassDefaults.Guests);
            RegisterCommand("autojoin", new BotCommandEvent(AutoJoin), GetCommandHelp("Autojoin.Summary", "Autojoin.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("join", new BotCommandEvent(Join), GetCommandHelp("Join.Summary", "Join.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("part", new BotCommandEvent(Part), GetCommandHelp("Part.Summary", "Part.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("list", new BotCommandEvent(List), GetCommandHelp("List.Summary", "List.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("chats", new BotCommandEvent(Chats), GetCommandHelp("Chats.Summary", "Chats.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("access", new BotCommandEvent(Access), GetCommandHelp("Access.Summary", "Access.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("user", new BotCommandEvent(User), GetCommandHelp("User.Summary", "User.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("priv", new BotCommandEvent(Priv), GetCommandHelp("Priv.Summary", "Priv.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("commands", new BotCommandEvent(Commands), GetCommandHelp("Commands.Summary", "Commands.Usage"), (int)PrivClassDefaults.Guests);
            RegisterCommand("credits", new BotCommandEvent(Credits), GetCommandHelp("Credits.Summary", "Credits.Usage"), (int)PrivClassDefaults.Guests);
            RegisterCommand("ctrig", new BotCommandEvent(CTrig), GetCommandHelp("CTrig.Summary", "CTrig.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("kick", new BotCommandEvent(Kick), GetCommandHelp("Kick.Summary", "Kick.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("plugins", new BotCommandEvent(Plugins), GetCommandHelp("Plugins.Summary", "Plugins.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("ping", new BotCommandEvent(Ping), GetCommandHelp("Ping.Summary", "Ping.Usage"), (int)PrivClassDefaults.Members);
            RegisterCommand("quit", new BotCommandEvent(Quit), GetCommandHelp("Quit.Summary", "Quit.Usage"), (int)PrivClassDefaults.Owner);
            RegisterCommand("restart", new BotCommandEvent(Restart), GetCommandHelp("Restart.Summary", "Restart.Usage"), (int)PrivClassDefaults.Owner);
            RegisterCommand("say", new BotCommandEvent(Say), GetCommandHelp("Say.Summary", "Say.Usage"), (int)PrivClassDefaults.Members);
            RegisterCommand("saveconfig", new BotCommandEvent(SaveConfig), GetCommandHelp("SaveConfig.Summary", "SaveConfig.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("sudo", new BotCommandEvent(Sudo), GetCommandHelp("Sudo.Summary", "Sudo.Usage"), (int)PrivClassDefaults.Owner);
            RegisterCommand("debug", new BotCommandEvent(Debug), GetCommandHelp("Debug.Summary", "Debug.Usage"), (int)PrivClassDefaults.Operators);
            RegisterCommand("ignore", new BotCommandEvent(Ignore), GetCommandHelp("Ignore.Summary", "Ignore.Usage"), (int)PrivClassDefaults.Operators);
        }
        #endregion

        #region Command Event Handlers
        private void TrigCheck(string ns, string from, string message)
        {
            Respond(ns, from, Bot.Trigger);
        }

        private void Help(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            if (args.Length != 1)
            {                
                ShowHelp(ns, from, "help");
                return; 
            }

            // display appropriate help            
            ShowHelp(ns, from, args[0]);
        }

        private void Time(string ns, string from, string message)
        {
            Respond(ns, from, DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
        }

        private void About(string ns, string from, string message)
        {
            string[] args = GetArgs(message);

            // get command
            string command = string.Empty;
            if (args.Length > 0)
                command = args[0];

            // about string
            string about;

            switch (command.ToLower())
            {
                case "system":
                    about = string.Format("{0}: Running .NET 3.5 on {1}", from, Utility.GetOperatingSystemVersion());
                    dAmn.NpMsg(ns, about);                                            
                    break;
                case "uptime":
                    TimeSpan diff = DateTime.Now.Subtract(Bot.Start);
                    about = string.Format("<b><u>Bot Uptime</b><br />Bot running {0} days, {1} hours, {2} minutes, and {3} seconds.", diff.Days, diff.Hours, diff.Minutes, diff.Seconds);
                    Say(ns, about);
                    break;
                case "":
                default:
                    about = Bot.AboutString;
                    about = about.Replace("%N%", Bot.Info["Name"]);
                    about = about.Replace("%V%", Bot.Info["Version"]);
                    about = about.Replace("%S%", Bot.Info["Status"]);
                    about = about.Replace("%R%", Bot.Info["Release"]);
                    about = about.Replace("%O%", Bot.Owner);
                    about = about.Replace("%A%", Bot.Info["Author"]);
                    about = about.Replace("%T%", Bot.Trigger);
                    about = about.Replace("%R%", Bot.Info["Release"]);
                    about = about.Replace("%D%", Bot.IsDebug ? "Running in debug mode." : "");
                    Say(ns, about);
                    break;
            }
        }

        private void AutoJoin(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            bool showUsage = false;
            string chatroom;
            string command = string.Empty;

            // get command if available
            if (args.Length > 0)
                command = args[0];
         
            switch(command)
            {
                case "add":
                    if (args.Length == 2)
                    {
                        chatroom = args[1];
                        Bot.AddAutoJoinChatroom(chatroom);
                        Say(ns, string.Format("** #{0} chatroom added to auto join list *", chatroom));
                    }
                    else
                        showUsage = true;
                    break;
                case "remove":
                    if (args.Length == 2)
                    {
                        chatroom = args[1];
                        Bot.RemoveAutoJoinChatroom(chatroom);
                        Say(ns, string.Format("** #{0} chatroom removed from auto join list *", chatroom));
                    }
                    else
                        showUsage = true;
                    break;
                case "list":
                    if (args.Length == 1)
                    {
                        StringBuilder list = new StringBuilder("<b><u>Auto-join list</u></b>:<ul>");
                        foreach (string room in Bot.AutoJoin)
                            list.Append("<li>#" + room + "</li>");
                        list.Append("</ul>");
                        Say(ns, list.ToString());
                    }
                    else
                        showUsage = true;
                    break;
                default:
                    showUsage = true;
                    break;
            }

            if (showUsage)
            {
                ShowHelp(ns, from, "autojoin");
                return;
            }
        }

        private void Join(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            if (args.Length != 1)
            {                
                ShowHelp(ns, from, "join");
                return;
            }

            dAmn.Join(args[0]);
            Say(ns, string.Format("** joined the chatroom #{0} *", args[0]));
        }

        private void Part(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            
            // leave provided rooms
            if (args.Length == 0)
            {
                dAmn.Part(ns);                
            }
            else if (args.Length == 1)
            {
                dAmn.Part(args[0]);
                Say(ns, string.Format("** left the chatroom #{0} *", args[0]));
            }
            else if (args.Length > 1)
            {
                foreach (string room in args)
                    dAmn.Part(room);
                Say(ns, string.Format("** left the chatrooms {0} *", message));
            }            
        }

        private void List(string ns, string from, string message)
        {
            string room = ns;

            // get room from command args
            if (!string.IsNullOrEmpty(message))
            {
                string[] args = GetArgs(message);

                if (args.Length == 1)
                    room = args[0];
            }

            // format the room correctly
            if (!room.StartsWith("#"))
                room = "#" + room;

            Chat chat = Bot.GetChatroom(room);
            if (chat == null)
            {
                Say(ns, string.Format("The bot is not signed into the chatroom {0}", room));
                return;
            }

            // build string
            StringBuilder list = new StringBuilder(string.Format("Users in the chatroom {0}:<br />", room));
            foreach (User u in chat.GetAllMembers())
            {
                list.Append(string.Format("<sub>{0}</sub><br />", u.Username));
            }

            // send list to server
            Say(ns, list.ToString());
        }

        private void Chats(string ns, string from, string message)
        {
            StringBuilder list = new StringBuilder("<b><u>Chatrooms signed into</u></b>:<ul>");
            foreach (Chat c in Bot.GetAllChatrooms())
            {
                list.Append(string.Format("<li>{0} <sub>({1} users)</sub></li>", c.Name, c.UserCount));
            }
            list.Append("</ul>");
            Say(ns, list.ToString());
        }

        private void Access(string ns, string from, string message)
        {
            bool showUsage = false;
            string[] args = GetArgs(message);
            int accessLevel = 0;
            string command = string.Empty;

            // parse command args
            if (args.Length == 2)
            {
                command = args[0];
                if (!int.TryParse(args[1], out accessLevel))
                    showUsage = true;
            }
            else
            {
                showUsage = true;
            }
            
            // show usage if necessary
            if (showUsage)
            {                
                ShowHelp(ns, from, "access");
                return;
            }

            // set level
            Bot.Access.SetCommandLevel(command, accessLevel);
            Say(ns, string.Format(
                "** Access level for command '{0}' was set to {1} *", command, accessLevel));
            Bot.Access.SaveAccessLevels();
        }

        private void User(string ns, string from, string message)
        {
            string[] args = GetArgs(message);            
            bool showUsage = false;
            string command = string.Empty;
            string user;
            string name;
            int level;

            if (args.Length >= 1)
                command = args[0];

            switch (command)
            {
                case "add":
                case "edit":
                    if (args.Length == 3)
                    {
                        user = args[1];
                        // if level is an int pass it, otherwise use priv class
                        if (int.TryParse(args[2], out level))
                        {
                            Bot.Access.SetUserLevel(user, level);
                            Say(ns, string.Format(
                                "** :dev{0}: has been given access level privilege {1} by {2} *",
                                user,
                                level,
                                from));
                        }
                        else
                        {
                            string privClass = args[2];
                            if (Bot.Access.HasUserLevel(privClass))
                            {
                                Bot.Access.SetUserLevel(user, privClass);
                                Say(ns, string.Format(
                                    "** :dev{0}: has been made a member of bot privclass {1} by {2} *",
                                    user,
                                    privClass,
                                    from));
                            }
                            else
                            {
                                Say(ns, string.Format("<b>Error:</b> bot privclass {0} does not exist.", privClass));
                            }
                        }
                    }
                    else
                    {
                        showUsage = true;
                    }
                    break;
                case "list":
                    StringBuilder output = new StringBuilder("<b><u>Bot User list:</u></b><br />");
                    Dictionary<string, int> userList = Bot.Access.GetAllUserAccessLevels();
                    // order them by access level
                    var sortedList = from u in userList
                                     orderby u.Value descending
                                     select u;

                    foreach (var u in sortedList)
                    {
                        output.Append(string.Format(":dev{0}: <sub>(Access level: {1})</sub><br />", u.Key, u.Value)); 
                    }                    
                    Say(ns, output.ToString());
                    break;
                case "del":
                    if (args.Length == 2)
                    {
                        user = args[1];
                        Bot.Access.RemoveUserLevel(user);
                    }
                    else
                    {
                        showUsage = true;
                    }
                    break;
                case "addprivclass":
                    if (args.Length == 2)
                    {
                        name = args[1];
                        level = Convert.ToInt32(args[2]);
                        Bot.Access.AddPrivClass(PrivClass.Create(name, level));
                    }
                    else
                    {
                        showUsage = true;
                    }
                    break;
                case "delprivclass":
                    if (args.Length == 1)
                    {
                        name = args[1];
                        Bot.Access.RemovePrivClass(name);
                    }
                    else
                    {
                        showUsage = true;
                    }
                    break;
            }

            if (showUsage)
            {                
                ShowHelp(ns, from, "user");
                return;
            }
            else
            {
                // save all changes
                Bot.Access.SaveAccessLevels();
            }
        }

        private void Priv(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            bool showUsage = false;

            if (args.Length == 1)
            {
                if (args[0] == "list")
                {
                    List<PrivClass> privClasses = Bot.Access.GetAllPrivClasses();
                    StringBuilder output = new StringBuilder("<b><u>Priv Classes</u></b>:<sub><ul>");
                    foreach (PrivClass privClass in privClasses)
                    {
                        output.AppendFormat("<li><b>{0}</b> - Level: {1}</li>", privClass.Name, privClass.AccessLevel);
                    }
                    output.Append("</ul></sub>");
                    Say(ns, output.ToString());
                }
                else
                    showUsage = true;
            }
            else if (args.Length == 2)
            {
                string privClass = args[0];
                int level;
                if (!int.TryParse(args[1], out level))
                {
                    showUsage = true;
                }
                else
                {
                    Bot.Access.UpdatePrivClassAccessLevel(privClass, level);
                    Say(ns, string.Format(
                        "** Priv class {0} was changed to access level {1} by {2} *",
                        privClass, 
                        level,
                        from));
                    Bot.Access.SaveAccessLevels();
                }
            }

            if (showUsage)
            {                
                ShowHelp(ns, from, "priv");
                return;
            }
        }

        private void Commands(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            bool showUsage = false;
            string command = string.Empty;

            // get the command
            if (args.Length > 0)
                command = args[0];

            Dictionary<string, string> commands = Bot.GetCommandsDetails();
            StringBuilder output = new StringBuilder();

            switch (command)
            {
                case "all":
                    // list all commands
                    output.Append("<b><u>All commands</u></b>:<ul>");
                    foreach (KeyValuePair<string, string> cmd in commands)
                    {
                        output.Append(string.Format(
                            "<li>{0} <sub>(Access level: {1})</sub></li>",
                            cmd.Key,
                            Bot.Access.GetCommandLevel(cmd.Key)
                        ));
                    }
                    output.Append("</ul>");
                    break;
                case "details":
                    // list all commands and show modules
                    output.Append("<b><u>All commands and modules</u></b>:<ul>");
                    foreach (KeyValuePair<string, string> cmd in commands)
                    {
                        output.Append(string.Format(
                            "<li>{0} <sub>(Access level: {1}, Module: {2})</sub></li>",
                            cmd.Key,
                            Bot.Access.GetCommandLevel(cmd.Key),
                            cmd.Value
                        ));
                    }
                    output.Append("</ul>");
                    break;
                default:                    
                    // show list of commands user has access to
                    output.Append("<b><u>Available commands</u></b>:<ul>");
                    foreach (KeyValuePair<string, string> cmd in commands)
                    {
                        if (Bot.Access.UserHasAccess(from, cmd.Key))
                        {
                            output.Append(string.Format(
                                "<li>{0} <sub>(Access level: {1})</sub></li>",
                                cmd.Key,
                                Bot.Access.GetCommandLevel(cmd.Key)
                            ));
                        }
                    }
                    output.Append("</ul>");
                    break;
            }

            if (showUsage)
            {
                ShowHelp(ns, from, "commands");
                return;
            }
            else
            {
                Say(ns, output.ToString());
            }
        }

        private void Credits(string ns, string from, string message)
        {
            string credit = "Oberon is a bot by :devbigmanhaywood:<br /><sub>"
                + "&middot; Inspired by Contra 4 and and Dante 0.10<br />"
                + "&middot; Module system created from scratch. Uses dynamic assembly loading.<br />"
                + "&middot; Configuration system uses standard .NET config files<br />" 
                + "&middot; Ideas stolen from anywhere and everywhere.</sub>";
            Say(ns, "<b><u>:bow: Credits</u></b>:<br />" + credit);
        }

        private void CTrig(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            if (args.Length != 1)
            {
                ShowHelp(ns, from, "ctrig");
                return;
            }

            string trigger = args[0];
            Bot.ChangeTrigger(trigger);
            Say(ns, string.Format("** bot trigger changed to {0} by {1} *", trigger, from));
        }

        private void Kick(string ns, string from, string message)
        {            
            string[] args = GetArgs(message);
            string room = ns;
            string user;
            string reason = null;

            // get the room to respond to
            if (args.Length >= 1 && args[0].StartsWith("#"))
            {
                room = args[0];
                // remove the room name from the string and get new args
                int index = message.IndexOf(room);
                message = message.Substring(0, index) + message.Substring(index + room.Length + 1);
                args = GetArgs(message);
            }

            // get arguments
            user = GetArg(args, 0);
            reason = ParseArg(message, 1);

            // show help if needed
            if (string.IsNullOrEmpty(user))
            {
                ShowHelp(ns, from, "kick");
                return;
            }

            // kick user
            if (string.IsNullOrEmpty(reason))
                dAmn.Kick(room, user);
            else
                dAmn.Kick(room, user, reason);

            if (room != ns)
                Say(ns, string.Format("** :dev{0}: kicked from #{1} by {2} *", user, room, from));
        }

        private void Plugins(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string command = string.Empty;
            string pluginKey = string.Empty;
            string status = string.Empty;
            Plugin plugin;

            // get args
            if (args.Length == 1)
            {
                command = args[0];
            }
            else if (args.Length == 2)
            {
                command = args[1];
                pluginKey = args[0];
            }
            else if (args.Length == 3)
            {
                command = args[1];
                pluginKey = args[0];
                status = args[2];
            }
            else
            {
                ShowHelp(ns, from, "plugins");
                return;
            }

            StringBuilder output = new StringBuilder();
            switch (command)
            {
                case "list":
                    List<Plugin> plugins = Bot.GetPlugins();
                    output.Append("<b><u>All plugins loaded</u></b>:<ul>");                    
                    foreach (Plugin p in plugins)
                        output.Append(string.Format("<li>{0} <sub>(Status: {1}, Key: {2})</li>",
                            p.PluginName,
                            p.Status.ToString("G"),
                            p.GetPluginKey()));
                    output.Append("</ul>");
                    break;
                case "reload":
                    Bot.ReloadPlugins();
                    output.Append("** all plugins reloaded *");
                    break;
                case "info":
                    plugin = (from p in Bot.GetPlugins()
                              where p.GetPluginKey() == pluginKey
                              select p).SingleOrDefault();
                    if (plugin == null)
                    {
                        Respond(ns, from, "** the plugin with key " + pluginKey + " does not exist *");
                        return;
                    }
                    output.Append("<b><u>Plugin Details</u></b>:<br />");
                    output.Append(plugin.PluginName + "<br />");
                    output.Append("<b>Key</b>: " + plugin.GetPluginKey() + "<br />");
                    output.Append("<b>Commands</b>:<ul>");

                    // get commands for this plugin
                    var commands = from c in Bot.GetCommandsDetails()
                                   where c.Value == plugin.PluginName
                                   select c;
                    foreach (var cmd in commands)
                        output.Append(string.Format("<li><sub>{0}</sub></li>", cmd.Key));
                    output.Append("</ul>");
                    break;
                case "turn":
                    // set status
                    PluginStatus pluginStatus = (status == "on") ? PluginStatus.On : PluginStatus.Off;

                    if (pluginKey == "all")
                    {
                        // get all plugins (besides core ones)
                        List<Plugin> pluginList = Bot.GetPlugins();
                        pluginList.RemoveAll(p => p is CorePluginBase);

                        // set all of their statuses
                        foreach (Plugin p in pluginList)
                            Bot.SetPluginStatus(p.PluginName, pluginStatus);
                        output.Append(string.Format("** status for all plugins was set to '{0}' *", pluginStatus.ToString("G")));
                    }
                    else
                    {
                        plugin = (from p in Bot.GetPlugins()
                                  where p.GetPluginKey().ToLower() == pluginKey.ToLower()
                                  select p).SingleOrDefault();
                        if (plugin == null)
                        {
                            Respond(ns, from, "** the plugin with key " + pluginKey + " does not exist *");
                            return;
                        }
                        else if (plugin is CorePluginBase)
                        {
                            Respond(ns, from, "** core plugins cannot be turned off *");
                            return;
                        }

                        Bot.SetPluginStatus(plugin.PluginName, pluginStatus);
                        output.Append(string.Format("** status for plugin '{0}' was set to {1} *", pluginKey, pluginStatus.ToString("G")));
                    }
                    break;
                case "settings":
                    plugin = (from p in Bot.GetPlugins()
                              where p.GetPluginKey().ToLower() == pluginKey.ToLower()
                              select p).SingleOrDefault();
                    if (plugin == null)
                    {
                        Respond(ns, from, "** the plugin with key " + pluginKey + " does not exist *");
                        return;
                    }

                    if (status == "clear")
                    {
                        plugin.ClearSettings();
                        output.AppendFormat("** settings for plugin '{0}' have been cleared *", pluginKey);
                    }
                    else if (status == "save")
                    {
                        plugin.SaveSettings();
                        output.AppendFormat("** settings for plugin '{0}' have been saved *", pluginKey);
                    }
                    else
                    {
                        Respond(ns, from, "the valid options for settings are 'clear' or 'save'.");
                        return;
                    }
                    break;
                default:
                    ShowHelp(ns, from, "plugins");
                    return;
            }

            Say(ns, output.ToString());
        }

        private void Ping(string ns, string from, string message)
        {
            Respond(ns, from, "Pong?");
        }

        private void Quit(string ns, string from, string message)
        {
            Say(ns, "** shutting down the bot... *");
            Bot.Shutdown();
        }

        private void Restart(string ns, string from, string message)
        {
            Say(ns, "** restarting the bot... *");
            Bot.Restart();
        }

        private void Say(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string room = ns;

            if (args.Length > 2 && args[0].StartsWith("#"))
            {
                room = args[0];
                message = message.Substring(message.IndexOf(args[1]));
            }

            Say(room, message);
        }

        private void SaveConfig(string ns, string from, string message)
        {
            Bot.SaveConfig();
            Say(ns, from, "** bot config file saved successfully *");
        }

        private void Sudo(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string targetUser = GetArg(args, 0);
            string commandName = GetArg(args, 1);
            string newMessage = ParseArg(message, 2); 


            // make sure we have all our arguments
            if (string.IsNullOrEmpty(targetUser))
            {
                Respond(ns, from, "You must provide a user to impersonate.");
                return;
            }
            else if (string.IsNullOrEmpty(commandName))
            {
                Respond(ns, from, "You must provide a command to execute.");
                return;
            }

            // send it to the bot
            Bot.TriggerCommand(commandName, ns, targetUser, newMessage);
        }

        private void Debug(string ns, string from, string message)
        {
            if (string.Compare(message, "on") != 0 &&
               string.Compare(message, "off") != 0)
            {
                ShowHelp(ns, from, "debug");
                return;
            }

            // set bot status
            bool debugStatus = message.ToLower() == "on" ? true : false;
            Bot.ChangeDebugStatus(debugStatus);

            // notify user
            Respond(ns, from, string.Format("** Bot debug staus has been set to '{0}' *", message));
        }

        private void Ignore(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string subCommand = GetArg(args, 0);
            string value = GetArg(args, 1);

            switch (subCommand)
            {
                case "list":
                    if (Bot.IgnoredUsers.Count == 0)
                    {
                        Respond(ns, from, "The ignored user list is empty.");
                        return;
                    }
                    else
                    {
                        StringBuilder output = new StringBuilder("<b><u>Ignored Users</u></b>:<ul>");
                        foreach (string username in Bot.IgnoredUsers)
                            output.AppendFormat("<li><sub>{0}</sub></li>", username);
                        output.Append("</ul>");
                    }
                    break;
                case "add":
                    if (string.IsNullOrEmpty(value))
                    {
                        Respond(ns, from, "Please provide a username to add.");
                        return;
                    }
                    if (Bot.IgnoredUsers.Contains(value))
                    {
                        Respond(ns, from, "This username is already in the ignored list.");
                        return;
                    }
                    Bot.IgnoredUsers.Add(value);
                    Respond(ns, from, string.Format("** The username '{0}' has been added to the ignore list *", value));
                    break;
                case "remove":
                    if (string.IsNullOrEmpty(value))
                    {
                        Respond(ns, from, "Please provide a username to remove.");
                        return;
                    }
                    if (!Bot.IgnoredUsers.Contains(value))
                    {
                        Respond(ns, from, "This username is not in the ignored list.");
                        return;
                    }
                    Bot.IgnoredUsers.Remove(value);
                    Respond(ns, from, string.Format("** The username '{0}' has been removed from the ignore list *", value));
                    break;
            }
        }
        #endregion
    }
}
