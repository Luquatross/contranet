using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Threading;
using DeviantArt.Chat.Library;
using System.Security.Authentication;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// An event that represents a packet received from the dAmn servers.
    /// </summary>
    /// <param name="ns">Chat room that this packet is associated with. May be null.</param>    
    /// <param name="packet">The raw server packet.</param>
    public delegate void BotServerPacketEvent(string ns, dAmnServerPacket packet);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ns"></param>
    /// <param name="from"></param>
    /// <param name="message"></param>
    /// <param name="target"></param>
    public delegate void BotCommandEvent(string ns, string from, string message, string target);

    /// <summary>
    /// Custom object made to simplify code syntax. This is a list of KeyValue pairs.
    /// The key/value being the plugin and the method. 
    /// </summary>
    public class BotEventList : List<KeyValuePair<Plugin, BotServerPacketEvent>> { }

    /// <summary>
    /// The core of the system, the bot that runs all processes.
    /// </summary>
    public class Bot
    {
        #region Public Properties
        public DateTime Start { get; private set; }
        public Dictionary<string, string> Info = new Dictionary<string, string>
        {
            { "Name", "Contra" },
            { "Version", "4" },
            { "Status", "" },
            { "Release", "public" },
            { "Author", "bigmanhaywood" }
        };
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Owner { get; private set; }
        public string Trigger { get; private set; }
        public string AboutString { get; private set; }
        public string[] AutoJoin { get; private set; }
        public HttpCookie AuthCookie { get; private set; }
        public Console Console { get; private set; }
        public string SystemString { get; private set; }        
        public string Session { get; private set; }
        public bool IsDebug { get; private set; }

        public string[] ShutDownString = new string[]
        {
            "Bot has quit.",
            "Bye bye!"
        };
        #endregion

        #region Private Variables
        private dAmnNET dAmn;
        private Thread listenThread;
        private bool authTokenFromConfig = true;

        /// <summary>
        /// Variable to hold mapping of packet types to a BotEventList. This way, one packet type
        /// can be mapped to a many plugin/method combinations.
        /// </summary>
        private Dictionary<dAmnPacketType, BotEventList> eventMap = new Dictionary<dAmnPacketType, BotEventList>();

        /// <summary>
        /// Variable to hold mapping of one command to one plugin and method.
        /// </summary>
        private Dictionary<string, KeyValuePair<Plugin, BotCommandEvent>> commandMap = new Dictionary<string, KeyValuePair<Plugin, BotCommandEvent>>();
        #endregion

        #region Private Properties
        /// <summary>
        /// Path to the bot config file.
        /// </summary>
        private string ConfigPath
        {
            get
            {
                string currentDirectory = System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location);
                return System.IO.Path.Combine(currentDirectory, "Config\\Bot.config");
            }
        }
        #endregion

        #region Constructor and Singleton Methods
        /// <summary>
        /// Constructor. Initializes variables needed for bot.
        /// </summary>
        private Bot()
        {
            // Generate a session ID code.
            this.Session = Utility.SHA1(DateTime.Now.Ticks.ToString());

            // Our start time is here.
            this.Start = DateTime.Now;

            // System information string
            this.SystemString = Utility.GetOperatingSystemVersion();

            // Get a new console interface
            this.Console = new Console();

            // Some introduction messages! We've already done quite a bit but only introduce things here...
            this.Console.Notice("Hey thar!");
            this.Console.Notice(string.Format("Loading {0} {1}{2} by {3}",
                this.Info["Name"],
                this.Info["Version"],
                this.Info["Status"],
                this.Info["Author"]));
            Console.Notice("Loading bot config file.");
            // Loading the config file
            if (!LoadConfig())
            {
                authTokenFromConfig = false;
            }
            // Display debug info
            if (IsDebug)
            {
                // This is for when we're running in debug mode.
                Console.Notice("Running in debug mode!");
                Console.Notice("Session ID: " + Session);
            }
            
            // Load the dAmn interface
            dAmn = new dAmnNET();

            // get cookie information if needed
            if (!authTokenFromConfig)
            {
                // get basic config data
                GetConfigDataFromUser();
                // get cookie information
                HttpCookie authCookie = dAmn.GetAuthCookie(Username, Password);
                if (authCookie == null)
                {
                    Console.Warning("Login information was invalid! Shutting down.");
                    throw new AuthenticationException(string.Format("The usernamae '{0}' and password '{1}' were invalid. Login was unsuccessful.", Username, Password));
                }
                // otherwise, save it!
                AuthCookie = authCookie;

                // save the config for next run
                SaveConfig();
            }

            // load cookie if it exists
            if (AuthCookie != null)
                dAmn.AuthCookie = AuthCookie;

            // initialize event map values
            InitializeEventMap();

            // Now we're ready to get some work done!
            Console.Notice("Ready!");
        }

        /// <summary>
        /// Singleton instance of this class. Lazy-loading and thread-safe.
        /// </summary>
        /// <remarks>See http://www.yoda.arachsys.com/csharp/singleton.html</remarks>
        public static readonly Bot Instance = new Bot();

        /// <summary>
        /// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit.
        /// </summary>
        static Bot() { }
        #endregion

        #region Bot Config Methods
        /// <summary>
        /// Loads bot state and login information from config file.
        /// </summary>
        /// <returns>True if successful, otherwise false.</returns>
        public bool LoadConfig()
        {
            // if file doesn't exist
            if (!System.IO.File.Exists(ConfigPath))
                return false;

            // create xml document
            XmlDocument configDoc = new XmlDocument();
            configDoc.Load(ConfigPath);

            // get root element
            XmlNode root = configDoc.DocumentElement;

            // get general settings
            Username = root.SelectSingleNode("general/add[@key='username']").Attributes["value"].Value;
            Password = root.SelectSingleNode("general/add[@key='password']").Attributes["value"].Value;
            Trigger = root.SelectSingleNode("general/add[@key='trigger']").Attributes["value"].Value;
            Owner = root.SelectSingleNode("general/add[@key='owner']").Attributes["value"].Value;
            IsDebug = Convert.ToBoolean(root.SelectSingleNode("general/add[@key='debug']").Attributes["value"].Value);

            // get about string
            AboutString = root.SelectSingleNode("about").InnerText;

            // get the auto joins
            List<string> autoJoins = new List<string>();
            XmlNodeList autoJoinNodes = root.SelectNodes("autoJoins/add");
            foreach (XmlNode joinNode in autoJoinNodes)
                autoJoins.Add(joinNode.Attributes["channel"].Value);
            AutoJoin = autoJoins.ToArray();

            // get the cookie
            AuthCookie = new HttpCookie("BotCookie");
            XmlNodeList cookieData = root.SelectNodes("cookieData/add");
            foreach (XmlNode data in cookieData)
                AuthCookie.Values.Add(data.Attributes["key"].Value, data.Attributes["value"].Value);

            return true;
        }

        /// <summary>
        /// Saves the config values to the file system.
        /// </summary>
        public void SaveConfig()
        {
            // if directory doesn't exist, create it
            string configDirectory = System.IO.Path.GetDirectoryName(ConfigPath);
            if (!System.IO.Directory.Exists(configDirectory))
                System.IO.Directory.CreateDirectory(configDirectory);

            // get auto join channels
            List<XElement> autoJoinsElements = new List<XElement>();
            foreach (string autoJoinChannel in this.AutoJoin)
                autoJoinsElements.Add(new XElement("add", new XAttribute("channel", autoJoinChannel)));

            // construct document in linq to xml
            XDocument botConfig = new XDocument(
                new XDeclaration("1.0", "utf-8", ""),
                    new XElement("botSettings",
                        new XElement("general",
                            new XElement("add", new XAttribute("key", "username"), new XAttribute("value", Username)),
                            new XElement("add", new XAttribute("key", "password"), new XAttribute("value", Password)),
                            new XElement("add", new XAttribute("key", "trigger"), new XAttribute("value", Trigger)),
                            new XElement("add", new XAttribute("key", "owner"), new XAttribute("value", Owner)),
                            new XElement("add", new XAttribute("key", "debug"), new XAttribute("value", IsDebug))
                        ),
                        new XElement("about", new XCData(AboutString)),
                        new XElement("autoJoins", autoJoinsElements.ToArray()),
                        new XElement("cookieData",
                            new XElement("add", new XAttribute("key", "uniqueid"), new XAttribute("value", AuthCookie.Values["uniqueid"])),
                            new XElement("add", new XAttribute("key", "visitcount"), new XAttribute("value", AuthCookie.Values["visitcount"])),
                            new XElement("add", new XAttribute("key", "visittime"), new XAttribute("value", AuthCookie.Values["visittime"])),
                            new XElement("add", new XAttribute("key", "username"), new XAttribute("value", AuthCookie.Values["username"])),
                            new XElement("add", new XAttribute("key", "authtoken"), new XAttribute("value", AuthCookie.Values["authtoken"]))
                        )
                    )
                );
            botConfig.Save(ConfigPath);
        }

        /// <summary>
        /// Gets bot config data from the console user.
        /// </summary>
        protected void GetConfigDataFromUser()
        {
            Console.Notice("You don't have a config file! Creating a new one...");
            Console.Message("Please enter the following information.");

            // get values from the user
            Username = Console.GetValueFromConsole("Bot Username: ");
            Password = Console.GetValueFromConsole("Bot Password: ");
            Trigger = Console.GetValueFromConsole("Bot Trigger: ");
            Owner = Console.GetValueFromConsole("Bot Owner: ");

            // get about string
            AboutString = "This is an about string.";

            // get auto-join channels
            Console.Message("What channels would you like your bot to join? Separate with commas.");
            AutoJoin = Console.GetValueFromConsole("").Split(',');

            // set debug
            IsDebug = false;
        }
        #endregion

        #region Events and Commands
        /// <summary>
        /// Initializes the event map so that each packet type has an associated empty
        /// list. Each plugin will register itself to fill up this list.
        /// </summary>
        private void InitializeEventMap()
        {
            foreach (dAmnPacketType packetType in Enum.GetValues(typeof(dAmnPacketType)))
            {
                eventMap.Add(packetType, new BotEventList());
            }
        }

        /// <summary>
        /// Adds a listener for a certain packet type. When it occurs the listener added will be notified.
        /// </summary>
        /// <param name="packetType">Packet type to listen for.</param>
        /// <param name="plugin">Plugin associated with method.</param>
        /// <param name="method">Method to execute.</param>
        public void AddEventListener(dAmnPacketType packetType, Plugin plugin, BotServerPacketEvent method)
        {
            eventMap[packetType].Add(
                new KeyValuePair<Plugin, BotServerPacketEvent>(plugin, method));
        }

        /// <summary>
        /// Adds a listener for certain commands. When it occurs the listener added will be notified.
        /// </summary>
        /// <param name="commandName">Command to listen for.</param>
        /// <param name="plugin">Plugin assocaited with the method.</param>
        /// <param name="command">Method to execute.</param>
        public void AddCommandListener(string commandName, Plugin plugin, BotCommandEvent commandMethod)
        {            
            commandMap.Add(commandName,
                new KeyValuePair<Plugin, BotCommandEvent>(plugin, commandMethod));
        }

        /// <summary>
        /// Triggers the command provided by invoking its listener.
        /// </summary>
        /// <param name="commandName">Command name to trigger.</param>
        public void TriggerCommand(string commandName, dAmnServerPacket packet)
        {            
            if (commandMap.ContainsKey(commandName))
            {
                Plugin plugin = commandMap[commandName].Key;
                BotCommandEvent method = commandMap[commandName].Value;

                // only trigger command if plugin is activated
                if (plugin.Status == PluginStatus.On)
                {
                    string ns = null;
                    string from;
                    string message;
                    string target;

                    // convert packet into command                    
                    ConvertPacketIntoCommand(packet, out from, out message, out target);

                    // get chat room if it exists
                    if (!string.IsNullOrEmpty(packet.param))
                        ns = dAmn.DeformatChat(packet.param);

                    // send to listener
                    method(ns, from, message, target);
                }
            }
        }

        /// <summary>
        /// Converts a packet into a usable command by the plugin system.
        /// </summary>
        /// <param name="packet">Packet to convert.</param>
        /// <param name="from"></param>
        /// <param name="message"></param>
        /// <param name="target"></param>
        private void ConvertPacketIntoCommand(dAmnServerPacket packet, out string from, out string message, out string target)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Listen / Process Packets
        /// <summary>
        /// Method that listens on the read stream from dAmn for new
        /// packets. When new packets appear, it processes them.
        /// </summary>
        private void Listen()
        {
            try
            {
                while (true)
                {
                    // packet received from the server
                    dAmnServerPacket packet = null;
                    try
                    {
                        packet = (dAmnServerPacket)dAmn.ReadPacket();
                    }
                    catch
                    {
                        try
                        {
                            Thread.CurrentThread.Abort();
                        }
                        catch
                        {
                            return;
                        }
                    }
                                        
                    // process packet if it exists
                    if (packet != null)
                        ProcessPacket(packet);
                }
            }
            catch (ThreadAbortException ex)
            {
                return;
            }
        }

        /// <summary>
        /// Processes a packet received from the server and delegates it out the associated listeners.
        /// </summary>
        /// <param name="packet">Packet to process.</param>
        private void ProcessPacket(dAmnServerPacket packet)
        {
            // find the right listeners for this event
            BotEventList listeners = eventMap[packet.PacketType];
            if (listeners != null && listeners.Count > 0)
            {
                // trigger the event for each listener
                foreach (KeyValuePair<Plugin, BotServerPacketEvent> listener in listeners)
                {
                    Plugin plugin = listener.Key;
                    BotServerPacketEvent method = listener.Value;

                    // only trigger event if plug is activated
                    if (plugin.Status == PluginStatus.On)
                    {
                        method(dAmn.DeformatChat(packet.param), packet);
                    }
                }
            }
        }
        #endregion

        #region Run
        /// <summary>
        /// Starts the bot running. Connects to dAmn server and starts executing.
        /// </summary>
        public void Run()
        {
            listenThread = new Thread(new ThreadStart(Listen));
            // try to connect!
            if (dAmn.Connect(Username, Password))
            {
                // if connected auto join all associated channels
                foreach (string channel in AutoJoin)
                {
                    dAmn.Join(channel);
                }
                // start listening for packets
                listenThread.Start();
            }
            else
            {
                if (authTokenFromConfig)
                {
                    // if we got the auth token from the config it may have expired.
                    // in that case, get a new one.
                    AuthCookie = dAmn.GetAuthCookie(Username, Password);
                    dAmn.AuthCookie = AuthCookie;
                    authTokenFromConfig = false;

                    // save new auth token to config
                    SaveConfig();

                    // try connecting again.
                    Run();
                }
                else
                {
                    Console.Warning("Unable to connect to dAmn server!");
                    return;
                }
            }
        }
        #endregion
    }
}
