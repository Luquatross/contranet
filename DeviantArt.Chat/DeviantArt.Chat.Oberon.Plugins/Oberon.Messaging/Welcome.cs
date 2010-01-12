using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Oberon.Collections;
using DeviantArt.Chat.Library;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Welcome plugin to allow welcome message for a chatroom.
    /// </summary>
    public class Welcome : Plugin
    {
        #region Private Variables
        private string _PluginName = "Welcome Message";
        private string _FolderName = "Messaging";

        /// <summary>
        /// Welcome messages we will be storing.
        /// </summary>
        private RoomSettingCollection<string> WelcomeMessages
        {
            get
            {
                // retrieve welcome messages from settings so that they are stored when 
                // the bot shuts down. if they haven't been created already, add them.
                if (!Settings.ContainsKey("WelcomeMessages"))
                {
                    Settings["WelcomeMessages"] = new RoomSettingCollection<string>(null);
                }
                return (RoomSettingCollection<string>)Settings["WelcomeMessages"];
            }
        }

        /// <summary>
        /// Determines if a chatroom has settings enabled or not. The key is the chatroom
        /// and the value is whether or not it is enabled.
        /// </summary>
        private Dictionary<string, bool> WelcomeEnabled
        {
            get
            {
                // retrieve welcome enabled from settings so that they are stored 
                // when the bot shuts down.
                if (!Settings.ContainsKey("WelcomeEnabled"))
                {
                    Settings["WelcomeEnabled"] = new Dictionary<string, bool>();
                }
                return (Dictionary<string, bool>)Settings["WelcomeEnabled"];
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

        #endregion

        #region Plugin Methods
        public override void Load()
        {
            // tie into the join event so we can detect when a user has entered the chatroom
            RegisterEvent(dAmnPacketType.MemberJoin, new BotServerPacketEvent(UserJoined));

            // register commands
            RegisterCommand("wt", new BotCommandEvent(WelcomeManage), new CommandHelp(
                "Manage chat room welcome message settings.",
                "wt (#room) all [message] - Sets the welcome message used to great all users.<br />" +
                "wt (#room) pc [privclass] [message] - Set the welcome message used to greet users in the privclasss<br />" +
                "wt (#room) indv - Allow users to set their own welcome messages.<br />" +
                "wt (#room) [on / off] - Turn welcome messages on or off.<br />" +
                "wt (#room) settings - View the current settings for welcome messages.<br />" +
                "<i>Optional parameter \"channel\" always defaults to the channel you are in.</i>"), (int)PrivClassDefaults.Moderators);

            RegisterCommand("welcome", new BotCommandEvent(IndvWelcomeMessage), new CommandHelp(
                "Set your own welcome message.",
                "welcome [message] - Sets the welcome message to the message provided.<br />" +
                "Welcome messages can contain the following codes:<br /><sup>" +
                "<b>{from}</b> - This is replaced with the username of the person who joined.<br/>" +
                "<b>{channel}</b> - This is replaced with the channel the user just joined.<br/>" +
                "<b>{ns}</b> - This is replaced with the raw namespace of the channel the user just joined.</sup><br/>" +
                "Typing '" + Bot.Trigger + "welcome off' will delete your welcome message!"), (int)PrivClassDefaults.Guests);

            // load settings from file system
            LoadSettings();
        }

        public override void Close()
        {
            // save settings to the file system
            SaveSettings();
        }
        #endregion

        #region Event & Command Handlers
        private void UserJoined(string chatroom, dAmnServerPacket packet)
        {
            dAmnCommandPacket command = new dAmnCommandPacket(packet);   
        }

        private void IndvWelcomeMessage(string ns, string from, string message)
        {
            if (!WelcomeEnabled.ContainsKey(ns) || WelcomeEnabled[ns] == false)
            {
                Respond(ns, from, "Welcomes are not being used in " + ns + ".");
                return;
            }
            string[] args = GetArgs(message);
            string say = string.Empty;

            if (args.Length == 1 && args[0] == "off")
            {
                string awayMessage = WelcomeMessages.Get(ns, from);
                if (string.IsNullOrEmpty(awayMessage))
                {
                    say = "You didn't have a welcome message stored anyway.";
                }
                else
                {
                    WelcomeMessages.SetIndividual(ns, from, null);
                    say = "Welcome message deleted!";
                }
            }
            else
            {
                if (string.IsNullOrEmpty(message))
                {
                    say = "You need to give a welcome message to be set.";
                }
                else
                {
                    WelcomeMessages.SetIndividual(ns, from, message);
                    say = "Welcome sest! Your welcome message is as follows:<br />" + message;
                }
            }
            SaveSettings();
            Respond(ns, from, say);
        }

        private void WelcomeManage(string ns, string from, string message)
        {
            // get command arguments
            string[] args = GetArgs(message);

            // get room from command if it's there
            string room = ns;
            if (args.Length > 1 && args[0].StartsWith("#"))
            {
                room = args[0];
                // strip room from string and reset args
                message = message.Substring(0, message.IndexOf(' ')) + message.Substring(message.IndexOf(room) + room.Length);
                args = GetArgs(message);
            }
            string subCommand = GetArg(args, 0);
            string var1 = GetArg(args, 1);
            string var2 = GetArg(args, 2);
            string var3 = GetArg(args, 4);

            // create output variables
            StringBuilder say = new StringBuilder();

            switch (subCommand.ToLower())
            {
                case "all":
                    
                    break;
                case "pc":

                    break;
                case "indv":

                    break;
                case "on":
                case "off":

                    break;
                case "clear":

                    break;
                case "settings":

                    break;
                default:
                    ShowHelp(ns, from, "wt");
                    return;
            }

            // send to user!
            SaveSettings();
            Respond(ns, from, say.ToString());
        }
        #endregion
    }
}
