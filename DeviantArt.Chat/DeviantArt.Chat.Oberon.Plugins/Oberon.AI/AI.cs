using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using DeviantArt.Chat.Library;
using DeviantArt.Chat.Oberon.Collections;

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

        // thread that will load settings
        private Thread loadSettingsThread;
        // thread that will save settings
        private Thread saveSettingsThread;

        /// <summary>
        /// Instance of AIMLBot.
        /// </summary>
        private AIMLbot.Bot AimlBot;

        /// <summary>
        /// String representing owner plus ":".
        /// </summary>
        private string OwnerString;

        /// <summary>
        /// Local cache of AIMLBot users. Kept so we don't have to create them from scratch each time
        /// and so the bot remembers info about the user.
        /// </summary>
        private Dictionary<string, AIMLbot.User> Users = new Dictionary<string, AIMLbot.User>();

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

        /// <summary>
        /// Path to the aimlbot settings file.
        /// </summary>
        private string BotSettingsFilePath
        {
            get { return System.IO.Path.Combine(PluginPath, "config\\Settings.xml"); }
        }

        /// <summary>
        /// File path to the "brain" of the bot. 
        /// </summary>
        private string BotBrainFilePath
        {
            get { return System.IO.Path.Combine(PluginPath, "brain\\brain.bin"); }
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

        /// <summary>
        /// Loads settings associated with bot.
        /// </summary>
        private void LoadBotSettings()
        {
            if (System.IO.File.Exists(BotBrainFilePath))
            {
                // start a thread to read the file since it is an exepensive
                // operation (perhaps 1 to 2 min!) and let the console keep
                // executing. Won't accept user input until file is loaded.
                loadSettingsThread = new Thread(delegate()
                {
                    AimlBot.isAcceptingUserInput = false;
                    try
                    {
                        AimlBot.loadFromBinaryFile(BotBrainFilePath);
                        AimlBot.isAcceptingUserInput = true;
                    }
                    catch (SerializationException ex)
                    {
                        // log error
                        Bot.Console.Warning("Error loading AI settings. File was corrupt.");
                        Bot.Console.Log("Error laoding AI settings: " + ex.ToString());

                        // try to delete corrupt file
                        try { File.Delete(BotBrainFilePath); }
                        catch { }                    
    
                        // tell the bot to begin accepting user input again
                        AimlBot.isAcceptingUserInput = true;
                        return;
                    }
                    Bot.Console.Notice("AI settings loading is complete. Accepting user input.");
                });
                
                // register thread with bot
                Bot.RegisterPluginThread(loadSettingsThread);

                // fire it up
                loadSettingsThread.Start();
            }
        }

        /// <summary>
        /// Saves bot settings to the file system.
        /// </summary>
        private void SaveBotSettings()
        {
            // if the loading thread is still running (perhaps they started the bot
            // then shut it down immediately), wait until it's finished, so we 
            // aren't accessing the file at the same time.
            if (loadSettingsThread != null && loadSettingsThread.IsAlive)
                loadSettingsThread.Join();

            Bot.Console.Notice("Saving AI settings. This may take a minute...");
            AimlBot.saveToBinaryFile(BotBrainFilePath);
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
            LoadBotSettings();                       
        }

        public override void Close()
        {
            // save our settings for next use
            SaveSettings();
            SaveBotSettings();
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

                // form request
                AIMLbot.Request request = new AIMLbot.Request(
                    input,
                    GetUser(from),
                    AimlBot);
                // get the response
                AIMLbot.Result reply = AimlBot.Chat(request);

                // send it to the client!
                base.Respond(chatroom, from, reply.Output);
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
