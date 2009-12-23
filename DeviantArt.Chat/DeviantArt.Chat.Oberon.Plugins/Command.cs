using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Oberon.Plugins
{
    public class Command : Plugin
    {
        #region Private Variables
        private string _PluginName = "Oberon System Commands Plugin";
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

        public override void Load()
        {
            RegisterCommand("help", new BotCommandEvent(Help), "Displays help information about a particular command.");
            RegisterCommand("time", new BotCommandEvent(Time), "Displays the current bot time.");
            RegisterCommand("about", new BotCommandEvent(About), "Displays information about the bot.");
            RegisterCommand("autojoin", new BotCommandEvent(AutoJoin), "Adds or removes a chatroom from the auto join list.");
            RegisterCommand("join", new BotCommandEvent(Join), "Makes the bot join the provided chatroom.");
            RegisterCommand("part", new BotCommandEvent(Part), "Makes the bot leave the provided chatroom.");
            RegisterCommand("list", new BotCommandEvent(List), "List the users in a chatroom.");
            RegisterCommand("chats", new BotCommandEvent(Chats), "The chat rooms that the bot is currently signed into.");
        }

        private void Help(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            if (args.Length > 1)
            {
                Respond(ns, from, "Invalid help request.");
                return; // invalid help
            }

            // display appropriate help
            Bot.TriggerHelp(args[0], ns, from);
        }

        private void Time(string ns, string from, string message)
        {
            Respond(ns, from, DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
        }

        private void About(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string command = string.Empty;
            if (args.Length > 0)
                command = args[0];
            string about;
            switch (command.ToLower())
            {
                case "system":
                    about = string.Format("{0}: Running .NET 3.5 on {1}", from, Utility.GetOperatingSystemVersion());
                    dAmn.NpMsg(ns, about);                                            
                    break;
                case "uptime":
                    TimeSpan diff = DateTime.Now.Subtract(Bot.Start);
                    about = string.Format("<abbr title=\"{0}\"></abbr>Uptime: {1} hours, {2} minutes, and {3} seconds.", from, diff.Hours, diff.Minutes, diff.Seconds);
                    dAmn.Say(ns, about);
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
                    about = about.Replace("%R%", Bot.Info["Release"]);
                    about = about.Replace("%D%", Bot.IsDebug ? "Running in debug mode." : "");
                    dAmn.Say(ns, about);
                    break;
            }
        }

        private void AutoJoin(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            if (args.Length != 2)
            {
                dAmn.Say(ns, "Invalid command format.");
                return;
            }

            // get chatroom name
            string chatroom = args[1];
            switch(args[0])
            {
                case "add":
                    Bot.AddAutoJoinChatroom(chatroom);
                    dAmn.Say(ns, string.Format("{0} chatroom added to auto join list.", chatroom));
                    break;
                case "remove":
                    Bot.RemoveAutoJoinChatroom(chatroom);
                    dAmn.Say(ns, string.Format("{0} chatroom removed from auto join list.", chatroom));
                    break;
            }
        }

        private void Join(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string room = ns;
            if (args.Length == 1)
                room = args[0];

            dAmn.Join(room);
        }

        private void Part(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string room = ns;
            if (args.Length == 1)
                room = args[0];

            dAmn.Part(room);
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
                dAmn.Say(ns, string.Format("The bot is not signed into the chatroom {0}", room));
                return;
            }

            // build string
            StringBuilder list = new StringBuilder(string.Format("Users in the chatroom {0}:<br />", room));
            foreach (User u in chat.GetAllMembers())
            {
                list.Append(string.Format("<sub>{0}</sub><br />", u.Username));
            }

            // send list to server
            dAmn.Say(ns, list.ToString());
        }

        private void Chats(string ns, string from, string message)
        {
            StringBuilder list = new StringBuilder("Chatrooms currently signed into:<br />");
            foreach (Chat c in Bot.GetAllChatrooms())
            {
                list.Append(string.Format("{0} <sub>({1} users)</sub><br />", c.Name, c.UserCount));
            }

            dAmn.Say(ns, list.ToString());
        }
    }
}
