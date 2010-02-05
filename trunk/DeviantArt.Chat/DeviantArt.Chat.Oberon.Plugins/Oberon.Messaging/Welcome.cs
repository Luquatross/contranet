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
        private WelcomeSettingsCollection WelcomeMessages
        {
            get
            {
                // retrieve welcome messages from settings so that they are stored when 
                // the bot shuts down. if they haven't been created already, add them.
                if (!Settings.ContainsKey("WelcomeMessages"))
                {
                    Settings["WelcomeMessages"] = new WelcomeSettingsCollection();
                }
                return (WelcomeSettingsCollection)Settings["WelcomeMessages"];
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
                "wt (#room) all [message] - Adds a welcome message used to greet all users.<br />" +
                "wt (#room) list - lists welcome messages used to greet all users.<br />" +
                "wt (#room) remove [index] - index of the message to remove for all user greattings.<br />" +
                "wt (#room) pc [privclass] [message] - Set the welcome message used to greet users in the privclasss<br />" +                                
                "wt (#room) indv [on / off] - Allow users to set their own welcome messages.<br />" +
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
            // get data from packet
            dAmnCommandPacket command = new dAmnCommandPacket(packet);

            // retrieve welcome settings for room
            WelcomeSettings welcomeSettings = WelcomeMessages.Get(chatroom);

            if (welcomeSettings.Enabled)
            {
                // get our variables
                string from = command.From;
                string privClass = welcomeSettings.GetChat().GetUser(from).PrivClass;
                string welcomeMsg = welcomeSettings.GetMessage(from, privClass);

                // send welcome message if we have one
                if (!string.IsNullOrEmpty(welcomeMsg))
                {
                    welcomeMsg = welcomeMsg.Replace("{channel}", chatroom);
                    welcomeMsg = welcomeMsg.Replace("{ns}", chatroom);
                    welcomeMsg = welcomeMsg.Replace("{from}", from);
                    Say(chatroom, welcomeMsg);
                }
            }
        }

        private void IndvWelcomeMessage(string ns, string from, string message)
        {
            // retrieve welcome settings
            WelcomeSettings welcomeSettings = WelcomeMessages.Get(ns);

            if (!welcomeSettings.Enabled)
            {
                Respond(ns, from, "Welcomes are not being used in " + ns + ".");
                return;
            }
            else if (!welcomeSettings.AllowIndividualSettings)
            {
                Respond(ns, from, "You don't have the ability to set a welcome message in " + ns + ".");
            }
            string[] args = GetArgs(message);
            string say = string.Empty;

            if (args.Length == 1 && args[0] == "off")
            {
                string awayMessage = welcomeSettings.GetIndividualMessage(from);
                if (string.IsNullOrEmpty(awayMessage))
                {
                    say = "You didn't have a welcome message stored anyway.";
                }
                else
                {                    
                    welcomeSettings.SetIndividualMessage(from, null);
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
                    welcomeSettings.SetIndividualMessage(from, message);
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
            string var3 = GetArg(args, 3);            

            // get welcome settings
            WelcomeSettings welcomeSettings = WelcomeMessages.Get(room);

            // create output variables
            StringBuilder say = new StringBuilder();

            switch (subCommand.ToLower())
            {
                case "all":
                    message = ParseArg(message, 1);
                    if (string.IsNullOrEmpty(message))
                    {
                        say.Append("You need to give a welcome message to set.");
                    }
                    else
                    {
                        welcomeSettings.AddAllMessage(message);
                        say.AppendFormat("Welcome message for {0} set to \"{1}\".", room, message);
                    }
                    break;
                case "list":
                    List<string> allUserList = welcomeSettings.GetAllMessageList();
                    say.Append("<b><u>Welcomes for all users:</u></b>:<sub><br />");
                    foreach (string welcomeMsg in allUserList)
                        say.AppendFormat("{0}<br />", welcomeMsg);
                    say.Append("</sub>");
                    break;
                case "remove":
                    int index;
                    if (int.TryParse(var1, out index))
                    {
                        welcomeSettings.RemoveAllMessage(index);
                        say.AppendFormat("Welcome message at {0} removed successfully.", index);
                    }
                    else
                    {
                        welcomeSettings.ClearAllMessage();
                        say.Append("All global welcome messages have been removed.");
                    }
                    break;
                case "pc":
                    if (string.IsNullOrEmpty(var1))
                    {
                        say.Append("You need to give a privclass name.");
                    }
                    else
                    {
                        string pc = var1;
                        message = ParseArg(message, 2);
                        // make sure priv class exists if we're signed into the room
                        Chat chat = Bot.GetChatroom(room);
                        if (chat != null)
                        {
                            if (!chat.PrivClasses.ContainsKey(pc))
                            {
                                say.AppendFormat("{0} is not a valid privclass in {1}.", pc, room);
                                break;
                            }
                        }
                        // delete if need be
                        if (message.ToLower() == "off")
                        {                            
                            welcomeSettings.SetPrivClassMessage(pc, null);
                            say.AppendFormat("Welcome for {0} members has been deleted.", pc);
                        }
                        else
                        {
                            welcomeSettings.SetPrivClassMessage(pc, message);
                            say.AppendFormat("Welcome message set.");
                        }
                    }
                    break;
                case "indv":
                    if (string.IsNullOrEmpty(var1) || (var1.ToLower() != "on" && var1.ToLower() != "off"))
                    {
                        say.Append("The please specify whether to turn individual welcomes on or off.");
                    }
                    else
                    {
                        bool indvStatus = var1.ToLower() == "on" ? true : false;
                        welcomeSettings.AllowIndividualSettings = indvStatus;                        
                        say.AppendFormat("Welcomes set to '{0}' for individuals.", var1);
                    }
                    break;
                case "on":
                case "off":
                    bool welcStatus = subCommand.ToLower() == "on" ? true : false;                    
                    welcomeSettings.Enabled = welcStatus;
                    say.AppendFormat("Welcomes in {0} have been set to '{1}'.", room, welcStatus);
                    break;
                case "clear":
                    if (string.IsNullOrEmpty(var1))
                    {
                        say.AppendFormat("This will delete all welcome data! Type <code>{0}wt {1} yes</code> to clear all welcome data.", Bot.Trigger, subCommand);
                    }
                    else
                    {
                        welcomeSettings.Clear();
                        say.AppendFormat("Welcome data for {0} has been deleted.", room);
                    }
                    break;
                case "settings":
                    say.AppendFormat("<b><u>Welcome settings for {0}</u></b>:<br /><sub>");
                    say.AppendFormat("Welcome messages are turned <b>{0}</b><br />", welcomeSettings.Enabled == true ? "on" : "off");
                    say.AppendFormat("Welcome messages for individuals are turned <b>{0}</b><br />", welcomeSettings.AllowIndividualSettings == true ? "on" : "off");
                    Chat foundChat = Bot.GetChatroom(room);
                    if (room != null)
                    {
                        foreach (string privClass in foundChat.PrivClasses.Keys)
                        {
                            string privStatus = string.IsNullOrEmpty(welcomeSettings.GetPrivClassMessage(privClass)) ? "off" : "on";
                            say.AppendFormat("Welcome messages for {0} members are turned <b>{0}</b><br />", privClass, privStatus);
                        }
                    }
                    say.Append("</sub>");
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
