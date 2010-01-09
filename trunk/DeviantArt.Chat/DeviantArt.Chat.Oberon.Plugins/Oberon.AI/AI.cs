using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using DeviantArt.Chat.Library;
using DeviantArt.Chat.Oberon.Collections;
using System.Web;
using System.Net;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Class that allows interaction with the AIMLBot. See http://aimlbot.sourceforge.net/ for
    /// details about how to use and configure this bot.
    /// </summary>
    public class AI : Plugin
    {
        #region Private Variables
        // Plugin variables
        private string _PluginName = "Oberon AI";
        private string _FolderName = "AI";

        /// <summary>
        /// String representing owner plus ":".
        /// </summary>
        private string OwnerString;

        /// <summary>
        /// URL to the bot webservice.
        /// </summary>
        private string BotWebService = "http://kato.botdom.com/respond/";

        /// <summary>
        /// If set to true will respond to a tab even if the bot username owner is signed in.        
        /// </summary>
        private RoomSettingCollection<bool> Respond
        {
            get
            {
                if (!Settings.ContainsKey("Respond"))
                    Settings["Respond"] = new RoomSettingCollection<bool>(false);
                return (RoomSettingCollection<bool>)Settings["Respond"];             
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

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public AI()
        {
            // get owner string
            OwnerString = Bot.Owner + ":";
        }
        #endregion

        #region Helper Methods
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
        /// Displays ai status to user.
        /// </summary>
        /// <param name="ns"></param>
        private void ShowStatus(string ns)
        {
            StringBuilder output = new StringBuilder();
            output.Append("<b><u>Current Bot status</u></b>:<ul>");

            // get status for each chatroom we're signed into
            Chat[] chatrooms = Bot.GetAllChatrooms();
            foreach (Chat chat in chatrooms)
            {
                string respond = (Respond.Get(chat.Name) == true) ? "on" : "off";
                output.Append("<li>" + chat.Name + ". Respond: " + respond + "</li>");
            }
            output.Append("</ul>");

            // send status
            Say(ns, output.ToString());
        }

        /// <summary>
        /// Returns a string that's base 64 encoded and appropriate for URLs.
        /// </summary>
        /// <param name="message">Message to encode.</param>
        /// <returns>Encoded message.</returns>
        private string EncodeMessage(string message)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(message);
            string enc = Convert.ToBase64String(encbuff);
            return enc; 
        }
        #endregion

        #region Plugin Methods
        public override void Load()
        {
            // tie into the chat event so we can detect messages sent to us
            RegisterEvent(dAmnPacketType.Chat, new BotServerPacketEvent(ChatReceived));

            // register our commands
            CommandHelp help = new CommandHelp(
                "Controls settings for the AI chat bot.",
                "ai status - lists chatrooms and their ai settings<br />" +
                "ai (#room | all) respond [on | off]  - responds even when user with same username is present. Is off by default<br />" +                
                "<b>Example:<b> !ai all respond on - turns respond on for all rooms");
            RegisterCommand("ai", new BotCommandEvent(AIHandler), help, (int)PrivClassDefaults.Owner);

            // load our settings            
            LoadSettings();                      
        }

        public override void Close()
        {
            // save our settings for next use
            SaveSettings();
        }
        #endregion

        #region Event Handler
        /// <summary>
        /// Process the chat packet from the dAmn server and decide whether or not
        /// to have the AI respond to it.
        /// </summary>
        /// <param name="chatroom">Chatroom.</param>
        /// <param name="packet">Packet received.</param>
        private void ChatReceived(string chatroom, dAmnServerPacket packet)
        {                        
            // if we're not set to respond don't bother processing packet
            bool respond = Respond.Get(chatroom);
            if (!respond)
                return;

            // get relevant data about the packet
            dAmnCommandPacket commandPacket = new dAmnCommandPacket(packet);
            string from = commandPacket.From.ToLower();
            string message = commandPacket.Message;
            string owner = Bot.Username;
            bool toBot = false; 

            // determine if the message was directed at us
            toBot = (message.ToLower().StartsWith(owner.ToLower() + ":"));
            if (toBot)
            {
                // First let's make sure the owner didn't send it accidently...otherwise
                // we'll respond and then we'll see the response as a message and try to 
                // respond...and make one big recursive loop
                if (from.ToLower() == owner.ToLower())
                    return;

                string input = GetInput(message);
                
                try
                {
                    // form request
                    WebRequest request = HttpWebRequest.Create(BotWebService + from + "/" + EncodeMessage(input));
                    request.Timeout = (int)TimeSpan.FromSeconds(30.0).TotalMilliseconds;

                    // get the response
                    WebResponse response = request.GetResponse();
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // read response into string
                    string reply = reader.ReadToEnd();

                    // send it to the client!
                    base.Respond(chatroom, from, reply);
                }
                catch (WebException ex)
                {
                    // log error and try to restart the conversation
                    Bot.Console.Log("Error getting AI response. " + ex.ToString());
                    base.Respond(chatroom, from, "Sorry, I missed that. Could you say that again?");
                }
            }
        }
        #endregion

        #region Command Handlers
        /// <summary>
        /// Handles commands for the bot. 
        /// </summary>        
        private void AIHandler(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            
            // init variables
            string room = ns;
            bool isGlobal = false;         

            // if this is a status message show status
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

            // process respond message
            if (args.Length != 2)
            {
                ShowHelp(ns, from, "ai");
            }
            else
            {
                string value = args[1];
                bool status = (value == "on") ? true : false;
                if (isGlobal)
                {
                    if (status)
                        Respond.Set(status);

                    else
                        Respond.Clear();
                }
                else
                {
                    Respond.Set(room, status);
                }
                Say(ns, string.Format("** AI respond set to '{0}' for {1} by {2} *", value, room, from));
            }
        }
        #endregion
    }
}
