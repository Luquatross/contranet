using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using DeviantArt.Chat.Library;
using System.Text;

namespace DeviantArt.Chat.Oberon
{
    #region Bot Delegates
    /// <summary>
    /// An event that represents a packet received from the dAmn servers.
    /// </summary>
    /// <param name="ns">Chat room that this packet is associated with. May be null.</param>    
    /// <param name="packet">The raw server packet.</param>
    public delegate void BotServerPacketEvent(string ns, dAmnServerPacket packet);

    /// <summary>
    /// Event that is executed when a command is issued by a user. All fields can have
    /// a null value.
    /// </summary>
    /// <param name="ns">Chatroom command applies to if applicable.</param>
    /// <param name="from">User who issued the command.</param>
    /// <param name="message">Contents of the command.</param>    
    public delegate void BotCommandEvent(string ns, string from, string message);
    #endregion

    #region Bot Helper Classes
    /// <summary>
    /// Custom object made to simplify code syntax. This is a list of KeyValue pairs.
    /// The key/value being the plugin and the method. 
    /// </summary>
    public class BotEventList : List<KeyValuePair<Plugin, BotServerPacketEvent>> { }

    /// <summary>
    /// Class that encapsulates the summary and usage notes for a command.
    /// </summary>
    public class CommandHelp
    {
        /// <summary>
        /// Short description of the command.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Short description of the command.
        /// </summary>
        public string Usage { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="summary">Short description of the command.</param>
        /// <param name="usage">Short description of the command.</param>
        public CommandHelp(string summary, string usage)
        {
            Summary = summary;
            Usage = usage;
        }

        /// <summary>
        /// Returns help text as formatted string.
        /// </summary>
        /// <returns>Help text formatted for output.</returns>
        public override string ToString()
        {
            StringBuilder help = new StringBuilder("<ul>");

            // add summary
            if (!string.IsNullOrEmpty(Summary))
                help.Append("<li><b><u>Summary</u></b>: " + Summary + "</li>");

            // add usage
            if (!string.IsNullOrEmpty(Usage))           
                help.Append("<li><b><u>Usage</u></b>:<br />" + Usage + "</li>");            

            help.Append("</ul>");
            return help.ToString();
        }
    }
    #endregion

    /// <summary>
    /// The core of the system, the bot that runs all processes.
    /// </summary>
    public class Bot
    {
        #region Public Properties
        public DateTime Start { get; private set; }
        public Dictionary<string, string> Info = new Dictionary<string, string>
        {
            { "Name", "Oberon" },
            { "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
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

        /// <summary>
        /// Strings to display when shutting down.
        /// </summary>
        public string[] ShutDownString = new string[]
        {
            "Bot has shutdown.",
            "Bye bye!"
        };        

        /// <summary>
        /// This is only true once the bot has completely shut down.
        /// </summary>
        public bool IsAlive = true;

        /// <summary>
        /// Access levels for users and commands.
        /// </summary>
        public AccessLevel Access;

        /// <summary>
        /// The current directory for the executing assembly.
        /// </summary>
        public string CurrentDirectory = System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Path to the bot config file.
        /// </summary>
        public string ConfigPath
        {
            get { return System.IO.Path.Combine(CurrentDirectory, "Config\\Bot.config"); }
        }

        /// <summary>
        /// Path to plugin directory.
        /// </summary>
        public string PluginPath
        {
            get { return System.IO.Path.Combine(CurrentDirectory, "Plugins"); }
        }
        #endregion

        #region Private Variables
        /// <summary>
        /// Reference to dAmn library.
        /// </summary>
        private dAmnNET dAmn;

        /// <summary>
        /// Thread that listens to open socket.
        /// </summary>
        private Thread listenThread;

        /// <summary>
        /// Boolean determining if our authtoken was taken from the bot config or not.
        /// </summary>
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

        /// <summary>
        /// Variable to hold mapping of command names to their help text.
        /// </summary>
        private Dictionary<string, CommandHelp> commandHelp = new Dictionary<string, CommandHelp>();

        /// <summary>
        /// Private list of all plugins that are loaded.
        /// </summary>
        private Dictionary<string, Plugin> botPlugins = new Dictionary<string, Plugin>();

        /// <summary>
        /// A list of chatrooms currently joined.
        /// </summary>
        private Dictionary<string, Chat> Chats = new Dictionary<string, Chat>();

        /// <summary>
        /// Variable that determines if the listening thread should keep listening.
        /// </summary>
        private volatile bool KeepListening = true;

        /// <summary>
        /// Event that indicates the the bot has stopped listening to the dAmn server.
        /// </summary>
        private ManualResetEvent ListeningStoppedEvent = new ManualResetEvent(false);

        /// <summary>
        /// Should be set to true if restart has been called. Otherwise false.
        /// </summary>
        private volatile bool IsRestarting = false;

        /// <summary>
        /// Threads created by plugins.
        /// </summary>
        private List<Thread> pluginThreads = new List<Thread>();
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
                Console.Notice("Running in debug mode!");
                Console.Notice("Session ID: " + Session);
                Console.Notice("Bot config file loaded successfully.");
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
            if (IsDebug)
                Console.Notice("Initializing event map.");
            InitializeEventMap();

            // create access level object
            Access = new AccessLevel(this);

            // Now we're ready to get some work done!
            Console.Notice("Ready!");

            // set up app domain resolve (needed for loading plugins!
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
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

            // get plugin settings
            List<XElement> pluginElements = new List<XElement>();
            foreach (Plugin plugin in botPlugins.Values)
                pluginElements.Add(new XElement("add", new XAttribute("key", plugin.PluginName), new XAttribute("value", plugin.Status.ToString("G"))));

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
                        ),
                        new XElement("plugins", pluginElements)
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

            // register plugin so we have a reference to it
            RegisterPlugin(plugin);
        }

        /// <summary>
        /// Adds a listener for certain commands. When it occurs the listener added will be notified.
        /// </summary>
        /// <param name="commandName">Command to listen for.</param>
        /// <param name="plugin">Plugin assocaited with the method.</param>
        /// <param name="command">Method to execute.</param>
        /// <param name="commandHelp">Help text for the command.</param>
        /// <param name="accessLevel">Default access level for command. Can be changed by admins.</param>
        public void AddCommandListener(string commandName, Plugin plugin, BotCommandEvent commandMethod, CommandHelp help, int accessLevel)
        {
            // make sure command is free
            if (commandMap.ContainsKey(commandName))
            {
                Console.Warning(string.Format(plugin.PluginName + " error. The command '{0}' is already taken.", commandName));
                return;
            }

            // add mapping
            commandMap.Add(commandName,
                new KeyValuePair<Plugin, BotCommandEvent>(plugin, commandMethod));

            // add help
            if (commandHelp != null)
                commandHelp.Add(commandName, help);

            // add access level
            Access.SetCommandLevel(commandName, accessLevel);

            // register plugin so we have a reference to it
            RegisterPlugin(plugin);
        }

        /// <summary>
        /// Triggers the command provided by invoking its listener.
        /// </summary>
        /// <param name="commandName">Command name to trigger.</param>
        /// <param name="ns">Chatroom.</param>
        /// <param name="from">Who issued the command.</param>
        /// <param name="message">Contents of the command.</param>        
        public void TriggerCommand(string commandName, string ns, string from, string message)
        {
            if (commandMap.ContainsKey(commandName))
            {
                // get plugin info for the command
                Plugin plugin = commandMap[commandName].Key;
                BotCommandEvent method = commandMap[commandName].Value;

                // only trigger command if plugin is activated
                if (plugin.Status == PluginStatus.On)
                {
                    // anything could happen when calling a plugin method. if it blows up, 
                    // handle it gracefully and let the bot continue to run.
                    try
                    {
                        method(ns, from, message);
                    }
                    catch (Exception ex)
                    {
                        Console.Notice(string.Format("Error executing the command '{0}'. See bot log for details.", commandName));
                        Console.Log(ex.ToString());
                        dAmn.Say(ns, string.Format("{0}: error occured executing the command '{1}'. Notify the bot admin.", from, commandName));
                    }
                }
            }
            else
            {
                dAmn.Say(ns, string.Format("{0}: '{1}' is not a recognized command.", from, commandName));
            }
        }

        /// <summary>
        /// Shows the help text associated with the command.
        /// </summary>
        /// <param name="commandName">Command to show help for.</param>
        /// <param name="ns">Chatroom.</param>
        public void TriggerHelp(string commandName, string ns, string from)
        {
            if (commandHelp.ContainsKey(commandName))
            {
                dAmn.Say(ns, from + ": " + commandHelp[commandName].ToString());
            }
            else
            {
                dAmn.Say(ns, from + ": help text does not exist for this command.");
            }
        }

        /// <summary>
        /// Updates command help text.
        /// </summary>
        /// <param name="commandName">Command to update.</param>
        /// <param name="help">New help text.</param>
        public void UpdateCommandHelp(string commandName, CommandHelp help)
        {
            // make sure command exists
            if (!commandMap.ContainsKey(commandName))
                return;

            // add or update key
            if (commandHelp.ContainsKey(commandName))
            {
                commandHelp[commandName] = help;
            }
            else
            {
                commandHelp.Add(commandName, help);
            }
        }

        /// <summary>
        /// Gets a dictionary of all bot commands that have been mapped.
        /// The key is the command name and the value is the name of the plugin
        /// that contains the command.
        /// </summary>
        /// <returns>Dictionary of all commands. The key is the command name and the value is the name of the plugin.</returns>
        public Dictionary<string, string> GetCommandsDetails()
        {
            Dictionary<string, string> commandDetails = new Dictionary<string, string>();
            foreach (string key in commandMap.Keys.ToArray())
            {
                if (commandMap[key].Key.Status == PluginStatus.On)
                    commandDetails.Add(key, commandMap[key].Key.PluginName);
            }
            return commandDetails;
        }
        #endregion

        #region Plugin Methods
        /// <summary>
        /// Finds all plugin in the plugin directory and sub-directories.
        /// </summary>
        /// <returns>List of found plugins</returns>
        private Plugin[] FindPlugins()
        {
            List<Plugin> plugins = new List<Plugin>();

            // get all assemblies in the plugin folder structure
            string[] assemblies = (from file in Utility.GetFilesRecursive(new DirectoryInfo(PluginPath), "*.dll")
                                   select file.FullName).ToArray();

            // find all assemblies that are plugins
            foreach (string assembly in assemblies)
            {                
                Assembly a = Assembly.LoadFrom(assembly);
                Type[] assemblyTypes = a.GetTypes();
                foreach (Type t in assemblyTypes)
                {
                    // check to see if this type inherits from the plugin class
                    if (typeof(Plugin).IsAssignableFrom(t) && !t.IsAbstract)
                    {
                        try
                        {
                            Plugin p = (Plugin)Activator.CreateInstance(t);
                            plugins.Add(p);
                        }
                        catch (Exception ex)
                        {
                            Console.Notice(string.Format(
                                "Error creating plugin '{0}'. See bot log for details.",
                                t.ToString()));
                            Console.Log("Error creating plugin.\n" + ex.ToString());
                        }
                    }                    
                }
            }
            return plugins.ToArray();
        }

        /// <summary>
        /// Registers plugin so we can reference any time by it's name.
        /// </summary>
        /// <param name="plugin"></param>
        private void RegisterPlugin(Plugin plugin)
        {
            if (!botPlugins.ContainsKey(plugin.PluginName))
            {
                botPlugins.Add(plugin.PluginName, plugin);
                if (IsDebug)
                    Console.Notice("Plugin registered: " + plugin.PluginName);
            }
        }

        /// <summary>
        /// Load all of our plugins
        /// </summary>
        private void LoadPlugins()
        {
            if (IsDebug)
                Console.Notice("Starting plugin loading.");

            // find and instantiate all plugins
            Plugin[] allPlugins = FindPlugins();

            // set plugin statuses
            LoadPluginStatuses();

            // load all plugins
            int pluginsLoaded = 0;
            foreach (Plugin p in allPlugins)
            {
                try
                {
                    p.dAmn = dAmn;
                    p.Load();                    
                    pluginsLoaded++;
                }
                catch (Exception ex)
                {
                    Console.Notice(string.Format("Error loading plugin '{0}'. See bot log for details.", p.PluginName));
                    Console.Log("Error loading plugin.\n" + ex.ToString());
                }
            }
            if (IsDebug)
                Console.Notice(string.Format(
                    "Plugin loading completed. {0} of {1} plugins are running.",
                    pluginsLoaded,
                    allPlugins.Length
                ));            

            // add file watcher for plugin directory
            FileSystemWatcher pluginWatcher = new FileSystemWatcher();
            pluginWatcher.Filter = "*.dll"; // only watch for assemblies
            pluginWatcher.Path = PluginPath;
            pluginWatcher.EnableRaisingEvents = true;
            pluginWatcher.Changed += new FileSystemEventHandler(PluginDirectoryChanged);
        }

        /// <summary>
        /// Turns plugins on or off based on values from the bot config file.
        /// </summary>
        private void LoadPluginStatuses()
        {
            // if file doesn't exist
            if (!System.IO.File.Exists(ConfigPath))
                return;

            // create xml document
            XmlDocument configDoc = new XmlDocument();
            configDoc.Load(ConfigPath);

            // get root element
            XmlNode root = configDoc.DocumentElement;

            XmlNodeList statusSettings = root.SelectNodes("plugins/add");
            foreach (XmlNode statusSetting in statusSettings)
            {
                // get status
                string pluginName = statusSetting.Attributes["key"].Value;
                PluginStatus pluginStatus = (statusSetting.Attributes["value"].Value == "On" ? PluginStatus.On : PluginStatus.Off);

                SetPluginStatus(pluginName, pluginStatus);
            }
        }

        /// <summary>
        /// Executed when the plugin directory has been modified.
        /// </summary>
        private void PluginDirectoryChanged(object sender, FileSystemEventArgs e)
        {
            if (IsDebug)
                Console.Notice(string.Format("The plugin '{0}' was created/modified/deleted.", e.Name));
        }

        /// <summary>
        /// Reloads all the plugins for the bot.
        /// </summary>
        public void ReloadPlugins()
        {
            // clear all variables holding commands and events
            commandHelp.Clear();
            commandMap.Clear();
            eventMap.Clear();

            // clear all plugins
            botPlugins.Clear();

            // load plugins again
            LoadPlugins();

            // reload access levels for everything
            Access.LoadAccessLevels();
        }

        /// <summary>
        /// Set the plugin status on or off.
        /// </summary>
        /// <param name="pluginName">Plugin to change.</param>
        /// <param name="status">Plugin status.</param>
        public void SetPluginStatus(string pluginName, PluginStatus status)
        {
            if (botPlugins.ContainsKey(pluginName))
            {       
                // if we're setting it to the same value, don't bother
                if (botPlugins[pluginName].Status == status)
                    return;

                // set status 7 trigger appropriate plugin method
                botPlugins[pluginName].Status = status;
                if (status == PluginStatus.On)
                    botPlugins[pluginName].Activate();
                else
                    botPlugins[pluginName].Deactivate();
            }
        }

        /// <summary>
        /// Register a plugin thread with the bot. This is so when the
        /// bot shuts down it can wait for the thread to complete.
        /// </summary>
        /// <param name="t">Thread to register.</param>
        public void RegisterPluginThread(Thread t)
        {
            pluginThreads.Add(t);
        }
        #endregion

        #region Listen / Process Packets
        /// <summary>
        /// Method that listens on the read stream from dAmn for new
        /// packets. When new packets appear, it processes them.
        /// </summary>
        private void Listen()
        {
            ListeningStoppedEvent.Reset();
            Console.Notice("Bot has started listening for data.");

            while (KeepListening == true)
            {
                // packet received from the server
                dAmnServerPacket packet = null;
                try
                {
                    packet = (dAmnServerPacket)dAmn.ReadPacket();
                }
                catch (Exception ex)
                {
                    Console.Warning("Fatal error occured while reading packet. Aborting.");
                    Console.Log(ex.ToString());
                }

                // process packet if it exists
                if (packet != null)
                {
                    ProcessPacket(packet);
                }

                // sleep for a second (in case loop needs to be interrupted)
                Thread.Sleep(1);
            }
            
            ListeningStoppedEvent.Set();
            Console.Notice("Bot has stopped listening.");
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
                        string ns = null;
                        if (!string.IsNullOrEmpty(packet.param))
                            ns = dAmn.DeformatChat(packet.param);

                        // anything could happen when calling the plugin method. if it blows
                        // up handle it gracefully and let the bot continue to run.
                        try
                        {
                            method(ns, packet);
                        }
                        catch (ThreadInterruptedException ex)
                        {
                            // read loop was interrupted
                            return;
                        }
                        catch (Exception ex)
                        {
                            // log error to console and log
                            Console.Notice(string.Format(
                                "Error occurred during packet processing in plugin '{0}'. See bot log for details.",
                                plugin.PluginName));
                            Console.Log(string.Format("Error processing packet in plugin '{0}'.\n{1}",
                                plugin.PluginName,
                                ex.ToString()));
                        }
                    }
                }
            }
        }
        #endregion

        #region Run / Restart / Shutdown
        /// <summary>
        /// Starts the bot running. Connects to dAmn server and starts executing.
        /// </summary>
        public void Run()
        {            
            // load all of our plugins for the system
            LoadPlugins();

            // load saved access levels from file if there is one
            Access.LoadAccessLevels();
            
            // The listening thread will keep running until
            // the shutdown or restart command is called at which
            // point the ListeningStoppedEvent will release. If
            // it was a restart command, start the bot all over
            // again. Otherwise, proceed to shut down.
            do
            {
                if (IsRestarting)
                {
                    // disconnect and wait a bit for the dAmn
                    // servers to register that we've left.
                    dAmn.Disconnect();
                    Thread.Sleep(300);
                }

                // start up the bot
                StartBot();
                
                // reset the restart flag
                IsRestarting = false;

                // run until we the listening thread has stopped
                if (listenThread.IsAlive)
                {
                    // listening thread has exited
                    ListeningStoppedEvent.WaitOne();
                    // wait until thread has been disposed & releases resources
                    listenThread.Join();
                }
            }
            while (IsRestarting == true);

            // shut it down
            ShutdownBot();
        }

        /// <summary>
        /// Connects the bot to the dAmns servers and starts the listening thread.
        /// </summary>
        private void StartBot()
        {
            // try to connect!
            if (dAmn.Connect(Username, Password))
            {
                KeepListening = true;

                // load listening thread and start listening for packets     
                listenThread = new Thread(new ThreadStart(Listen));
                listenThread.Start();                

                // if connected auto join all associated channels
                foreach (string channel in AutoJoin)
                {
                    dAmn.Join(channel);
                }
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
                    StartBot();
                }
                else
                {
                    Console.Warning("Unable to connect to dAmn server!");
                    return;
                }
            }
        }

        /// <summary>
        /// Restarts the bot.
        /// </summary>
        public void Restart()
        {
            Console.Notice("Restarting bot...");  
            IsRestarting = true;
            KeepListening = false;
        }

        /// <summary>
        /// Shuts down the bot.
        /// </summary>
        public void Shutdown()
        {            
            KeepListening = false; 
        }

        /// <summary>
        /// Disconnects from server, saves files and closes plugins.
        /// </summary>
        private void ShutdownBot()
        {                        
            Console.Notice("Shutting down bot...");            

            // disconnect from the server
            dAmn.Disconnect();

            // save our master config
            SaveConfig();

            // save access levels
            Access.SaveAccessLevels();

            // call the close method on each of our plugins
            Console.Notice("Shutting down plugins. May take up to a few minutes.");
            foreach (Plugin plugin in botPlugins.Values.ToArray())
            {
                try
                {
                    plugin.Close();
                }
                catch (Exception ex)
                {
                    // log the exception
                    Console.Log(string.Format("Error occurred during plugin shutdown.\nPlugin Name: {0}\nException: {1}", 
                        plugin.PluginName,
                        ex.ToString()));
                }
            }

            // wait for any plugin threads to complete
            if (IsDebug)
                Console.Notice("Shutting down plugin threads...");
            foreach (Thread t in pluginThreads)
            {
                if (t.IsAlive)
                    t.Join();
            }

            // display parting message
            foreach (string str in ShutDownString)
                Console.Notice(str);

            // show that we're not running any more
            IsAlive = false;
        }
        #endregion

        #region Chatroom Methods
        /// <summary>
        /// Registers chatroom with bot.
        /// </summary>
        /// <param name="chatroomName">Chatroom name.</param>
        /// <param name="room">Chatroom.</param>
        public void RegisterChatroom(string chatroomName, Chat room)
        {
            chatroomName = chatroomName.ToLower();
            room.Notice(string.Format("** Bot has joined the {0} *", chatroomName));
            Chats.Add(chatroomName, room);
        }

        /// <summary>
        /// Removes chatroom from register list.
        /// </summary>
        /// <param name="chatroomName">Chatroom to remove.</param>
        public void UnregisterChatroom(string chatroomName)
        {
            chatroomName = chatroomName.ToLower();
            if (Chats.ContainsKey(chatroomName))
            {
                Chats[chatroomName].Notice(string.Format("** Bot has left the {0} *", chatroomName));
                Chats.Remove(chatroomName);
            }
        }

        /// <summary>
        /// Get chatroom.
        /// </summary>
        /// <param name="ns">Chatroom name.</param>
        /// <returns>Chatroom.</returns>
        public Chat GetChatroom(string ns)
        {
            ns = ns.ToLower();
            if (Chats.ContainsKey(ns))
                return Chats[ns];
            else
                return null;
        }

        /// <summary>
        /// Returns all chatrooms the bot is currently signed into.
        /// </summary>
        /// <returns>Array of all chatrooms.</returns>
        public Chat[] GetAllChatrooms()
        {
            return Chats.Values.ToArray();
        }

        /// <summary>
        /// Gets the number of opened chatrooms.
        /// </summary>
        /// <returns>Number of opened chatrooms.</returns>
        public int ChatroomsOpen()
        {
            return Chats.Count;
        }

        /// <summary>
        /// Adds a chatroom to be autojoined.
        /// </summary>
        /// <param name="chatroom">Chatroom to add.</param>
        public void AddAutoJoinChatroom(string chatroom)
        {
            chatroom = chatroom.ToLower();
            if (!AutoJoin.Contains(chatroom))
            {                
                List<string> temp = new List<string>(AutoJoin);
                temp.Add(chatroom);
                AutoJoin = temp.ToArray();                

                SaveConfig();
            }
        }

        /// <summary>
        /// Removes a chatroom from the autojoin.
        /// </summary>
        /// <param name="chatroom">Chatroom to remove.</param>
        public void RemoveAutoJoinChatroom(string chatroom)
        {
            chatroom = chatroom.ToLower();
            if (AutoJoin.Contains(chatroom))
            {
                List<string> temp = new List<string>(AutoJoin);
                temp.Remove(chatroom);
                AutoJoin = temp.ToArray();

                SaveConfig();
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Changes the bot trigger to the new trigger. Is not permanent - config
        /// file must be changed to make permanent.
        /// </summary>
        /// <param name="newTrigger">New trigger to use.</param>
        public void ChangeTrigger(string newTrigger)
        {
            Trigger = newTrigger;
        }

        /// <summary>
        /// Gets all bot plugins. Careful with the plugins! Modifying their settings
        /// could make the system unstable.
        /// </summary>
        /// <returns>Plugin list.</returns>
        public List<Plugin> GetPlugins()
        {
            return botPlugins.Values.ToList();
        }

        /// <summary>
        /// When loading plugin assemblies often the .NET loader doesn't know where to find assemblies.
        /// Using this method we will look through each assembly in the current app domain so that 
        /// we can find the right assembly.
        /// </summary>        
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly ayResult = null;
            string sShortAssemblyName = args.Name.Split(',')[0];
            Assembly[] ayAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly ayAssembly in ayAssemblies)
            {
                if (sShortAssemblyName == ayAssembly.FullName.Split(',')[0])
                {
                    ayResult = ayAssembly;
                    break;
                }
            }
            return ayResult;
        }
        #endregion
    }
}