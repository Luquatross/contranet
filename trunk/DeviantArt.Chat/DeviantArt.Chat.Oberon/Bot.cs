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
using System.Diagnostics;
using System.CodeDom.Compiler;
using Microsoft.Practices.Unity;
using System.Net;
using Ionic.Zip;

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

    /// <summary>
    /// Method which will sort event listeners in order of priority.
    /// </summary>
    /// <param name="x">Event to compate.</param>
    /// <param name="y">Event to compate.</param>
    public delegate int BotEventSorter(
        KeyValuePair<Plugin, BotServerPacketEvent> x,
        KeyValuePair<Plugin, BotServerPacketEvent> y);
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
        /// <summary>
        /// URL to download the lastest version of this bot.
        /// </summary>
        public const string DownloadUpdateUrl = "http://oberon.thehomeofjon.net/bin/latest.zip";

        /// <summary>
        /// Url to get the latest version number of this bot.
        /// </summary>
        public const string VersionUpdateUrl = "http://oberon.thehomeofjon.net/bin/latest-version.txt";

        /// <summary>
        /// Time this instance of the bot was started.
        /// </summary>
        public DateTime Start { get; private set; }

        /// <summary>
        /// General info about the bot.
        /// </summary>
        public static Dictionary<string, string> Info = new Dictionary<string, string>
        {
            { "Name", "Oberon" },
            { "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
            { "Status", "" },
            { "Release", "public" },
            { "Author", "bigmanhaywood" }
        };

        /// <summary>
        /// The .NET version of this executable.
        /// </summary>
        public Version AssemblyVersion
        {
            get { return new Version(Info["Version"]); }
        }

        /// <summary>
        /// Username to log onto the chat network.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Password used to log onto the chat network.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// The username for owner of the bot. 
        /// </summary>
        public string Owner { get; private set; }

        /// <summary>
        /// Trigger to activate bot commands.
        /// </summary>
        public string Trigger { get; private set; }

        /// <summary>
        /// String to describe this bot.
        /// </summary>
        public string AboutString { get; private set; }

        /// <summary>
        /// Collection of chatrooms the bot will autojoin.
        /// </summary>
        public string[] AutoJoin { get; private set; }

        /// <summary>
        /// The authentication cookie created when logging onto the server.
        /// </summary>
        public HttpCookie AuthCookie { get; private set; }

        /// <summary>
        /// Reference to class that outputs messages to the 
        /// console window.
        /// </summary>
        public IConsole Console { get; private set; }

        /// <summary>
        /// The operating system version and information.
        /// </summary>
        public string SystemString { get; private set; }

        /// <summary>
        /// The current session ID. Each time the bot is launched a 
        /// new session ID will be generated.
        /// </summary>
        public string Session { get; private set; }

        /// <summary>
        /// True if running in debug, otherwise false.
        /// </summary>
        public bool IsDebug { get; private set; }

        /// <summary>
        /// Pointer to the console window instance.
        /// </summary>
        public IntPtr ConsoleWindow { get; private set; }

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
        public IAccessLevel Access;

        /// <summary>
        /// The current directory for the executing assembly.
        /// </summary>
        public static string CurrentDirectory = System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Path to the bot config file.
        /// </summary>
        public static string ConfigPath
        {
            get { return System.IO.Path.Combine(CurrentDirectory, "Config\\Bot.config"); }
        }

        /// <summary>
        /// Path to plugin directory.
        /// </summary>
        public static string PluginPath
        {
            get { return System.IO.Path.Combine(CurrentDirectory, "Plugins"); }
        }

        /// <summary>
        /// Sorter for event listeners so that events are relayed in the correct order.
        /// </summary>
        public BotEventSorter EventListenerSorter { get; set; }

        /// <summary>
        /// List of users who the bot will ignore.
        /// </summary>
        public List<string> IgnoredUsers { get; private set; }
        #endregion

        #region Private Variables
        /// <summary>
        /// Inversion of control container.
        /// </summary>
        private IUnityContainer Container = new UnityContainer();

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
        /// Threads created by plugins or the bot that the bot will wait on
        /// when shutting down.
        /// </summary>
        private List<Thread> threadPool = new List<Thread>();

        /// <summary>
        /// The amount of time to wait for a thread in our threadpool list to terminate.
        /// </summary>
        private TimeSpan threadJoinWaitTime = TimeSpan.FromMinutes(1.00);

        /// <summary>
        /// The amount of time to wait for a new packet to arrive.
        /// </summary>
        private TimeSpan packetWaitTime = TimeSpan.FromMinutes(5.00);

        /// <summary>
        /// Thread to check for plugin updates.
        /// </summary>
        private Thread pluginUpdateThread;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor. Initializes variables needed for bot.
        /// </summary>
        /// <param name="access">AccessLevel manager.</param>
        /// <param name="console">Console manager.</param>
        /// <param name="dAmn">dAmnNET connector.</param>
        public Bot(IAccessLevel access, IConsole console, dAmnNET dAmn)
        {
            // check our variables
            if (access == null)
                throw new ArgumentNullException("access", "IAccessLevel cannot be null.");
            if (console == null)
                throw new ArgumentNullException("console", "IConsole cannot be null.");

            // Generate a session ID code.
            this.Session = Utility.SHA1(DateTime.Now.Ticks.ToString());

            // Our start time is here.
            this.Start = DateTime.Now;

            // System information string
            this.SystemString = Utility.GetOperatingSystemVersion();

            // Get a the console interface
            this.Console = console;

            // set the reference to the console window itself
            this.ConsoleWindow = Process.GetCurrentProcess().MainWindowHandle;

            // init ignored user list
            this.IgnoredUsers = new List<string>();

            // Some introduction messages! We've already done quite a bit but only introduce things here...
            this.Console.Notice("Hey thar!");
            this.Console.Notice(string.Format("Loading {0} {1}{2} by {3}",
                Info["Name"],
                Info["Version"],
                Info["Status"],
                Info["Author"]));
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

            // Ensure correct directories are present
            EnsureDirectoryStructure();

            // Load the dAmn interface
            this.dAmn = dAmn;
            this.dAmn.ReadTimeout = TimeSpan.FromMinutes(5.0);

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
            Access = access;
            Access.InitializeFor(this);
            Access.DefaultUserAccessLevel = (int)PrivClassDefaults.Guests;            

            // Now we're ready to get some work done!
            Console.Notice("Ready!");

            // set up app domain resolve (needed for loading plugins!
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }
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

            // get ignored users
            IgnoredUsers.Clear();
            XmlNodeList ignoredUserNodes = root.SelectNodes("ignoredUsers/add");
            foreach (XmlNode ignoreNode in ignoredUserNodes)
                IgnoredUsers.Add(ignoreNode.Attributes["username"].Value);

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

            // get ignored users
            List<XElement> ignoredUserElements = new List<XElement>();
            foreach (string ignoredUser in IgnoredUsers)
                ignoredUserElements.Add(new XElement("add", new XAttribute("username", ignoredUser)));

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
                        new XElement("plugins", pluginElements),
                        new XElement("ignoredUsers", ignoredUserElements)
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

            // output debug data
            Console.Debug(string.Format("Added command listener. Command = '{0}', Plugin = '{1}', AccessLevel = '{2}'", commandName, plugin.ToString(), accessLevel));

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
            Console.Debug(string.Format("command '{0}' is requesting to be triggered.", commandName));
            if (commandMap.ContainsKey(commandName))
            {
                Console.Debug(string.Format("command '{0}' was found in command map.", commandName));

                // get plugin info for the command
                Plugin plugin = commandMap[commandName].Key;
                BotCommandEvent method = commandMap[commandName].Value;

                // only trigger command if plugin is activated
                if (plugin.Status == PluginStatus.On)
                {
                    // output debug info
                    Console.Debug("executing command handler: " + method.ToString());

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
                else
                {
                    dAmn.Say(ns, string.Format("{0}: the plugin for this command is deactivated.", from));
                    Console.Debug(string.Format("The plugin for this command {0} is deactivated.", commandName));
                }
            }
            else
            {
                dAmn.Say(ns, string.Format("{0}: '{1}' is not a recognized command.", from, commandName));
                Console.Debug(string.Format("'{0}' was not found in command map.", commandName));
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
            foreach (string key in commandMap.Keys.ToArray().OrderBy(k => k))
            {
                if (commandMap[key].Key.Status == PluginStatus.On)
                    commandDetails.Add(key, commandMap[key].Key.PluginName);
            }
            return commandDetails;
        }
        #endregion

        #region Plugin Methods
        /// <summary>
        /// Creates plugins from provided assembly and adds them to the provided list.
        /// </summary>
        /// <param name="a">Assembly to create plugins from.</param>
        /// <param name="plugins">List to add created plugins to.</param>
        private void CreatePluginsFromAssembly(Assembly a, List<Plugin> plugins)
        {
            Type[] assemblyTypes = a.GetTypes();
            foreach (Type t in assemblyTypes)
            {
                // check to see if this type inherits from the plugin class
                if (typeof(Plugin).IsAssignableFrom(t) && !t.IsAbstract)
                {
                    Console.Debug(string.Format("{0} from assembly {1} has been detected as a plugin. Attempting to create plugin.", t.Name, a.FullName));

                    try
                    {
                        // create plugin
                        Plugin p = (Plugin)Activator.CreateInstance(t);
                        p.dAmn = dAmn;
                        p.LoadPluginManifest();

                        // check plugin compatability
                        if (p.HasManifest && !p.Manifest.IsCompatible(AssemblyVersion))
                            throw new Exception(string.Format("Plugin '{0}' is not compatible with this version of the bot.", p.PluginName));

                        // add to our list
                        plugins.Add(p);

                        // log success
                        Console.Debug(string.Format("Plugin {0} created successfully.", t.Name));
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

        /// <summary>
        /// Compiles the source files into assemblies.
        /// </summary>
        /// <param name="sourceFiles">Source files to compile.</param>
        /// <returns>Compiled assemblies.</returns>
        private List<Assembly> CompileSourceFilePlugins(List<FileInfo> sourceFiles)
        {
            string[] referencedAssemblies = { "System.dll", "Oberon.exe", "DeviantArt.Chat.Library.dll" };
            List<Assembly> compiledAssemblies = new List<Assembly>();

            // get compiler parameters
            CompilerParameters cp = new CompilerParameters();
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;
            cp.TreatWarningsAsErrors = false;

            // referenced assemblies
            cp.ReferencedAssemblies.AddRange(referencedAssemblies);
            
            // setup compiler variables
            CodeDomProvider provider = null;
            ICodeCompiler compiler = null;
            CompilerResults results = null;

            // find and compile all source files
            foreach (FileInfo sourceFile in sourceFiles)
            {
                // get the provider
                if (sourceFile.Extension.ToLower() == ".cs")
                    provider = CodeDomProvider.CreateProvider("CSharp");
                else if (sourceFile.Extension.ToLower() == ".vb")
                    provider = CodeDomProvider.CreateProvider("VisualBasic");
                else
                    continue;

                // compile!
                compiler = provider.CreateCompiler();
                results = compiler.CompileAssemblyFromFile(cp, sourceFile.FullName);

                // use compile assembly
                if (results.Errors.Count == 0)
                {
                    compiledAssemblies.Add(results.CompiledAssembly);
                }
                else
                {
                    // log error to console and log file, and skip
                    Console.Notice(string.Format(
                            "Error creating plugin from file '{0}'. See bot log for details.",
                            sourceFile.Name));
                    foreach (CompilerError error in results.Errors)
                        Console.Log(string.Format("Line {0}, Col {1}: Error {2} - {3}",
                            error.Line, 
                            error.Column,
                            error.ErrorNumber,
                            error.ErrorText));
                    continue;
                }
            }

            return compiledAssemblies;
        }

        /// <summary>
        /// Finds all plugin in the plugin directory and sub-directories.
        /// </summary>
        /// <returns>List of found plugins</returns>
        private Plugin[] FindAndCreatePlugins()
        {
            List<Plugin> plugins = new List<Plugin>();

            // get all assemblies in the plugin folder structure
            string[] assemblies = (from file in Utility.GetFilesRecursive(new DirectoryInfo(PluginPath), "*.dll")
                                   where Utility.PathContainsDirectory(file.DirectoryName, "_Off") == false
                                   select file.FullName).ToArray();

            // find all assemblies that are plugins
            foreach (string assembly in assemblies)
            {    
                // create plugin from assembly
                Console.Debug("Inspecting assembly " + assembly);
                Assembly a = null;
                try
                {
                    byte[] assemblyBytes = File.ReadAllBytes(assembly);
                    byte[] symbolBytes = null;

                    // see if symbol file exists
                    string symbolPath = assembly.Substring(0, assembly.LastIndexOf('.')) + ".pdb";
                    if (File.Exists(symbolPath))
                        symbolBytes = File.ReadAllBytes(symbolPath);

                    // load assembly - loading it this way prevents the locking of the file, so 
                    // we can overwrite the original file as needed.
                    if (symbolBytes == null)
                        a = Assembly.Load(assemblyBytes);
                    else
                        a = Assembly.Load(assemblyBytes, symbolBytes);
                }
                catch (Exception ex)
                {
                    Console.Warning(string.Format("Error loading assembly {0}. See bot log for details.", assembly));
                    Console.Debug(ex.ToString());
                    continue;
                }
                CreatePluginsFromAssembly(a, plugins);
            }

            // find all csharp and vb source files
            List<FileInfo> sourceFiles = new List<FileInfo>();
            sourceFiles.AddRange(Utility.GetFilesRecursive(new DirectoryInfo(PluginPath), "*.cs"));
            sourceFiles.AddRange(Utility.GetFilesRecursive(new DirectoryInfo(PluginPath), "*.vb"));

            // compile source files and add plugins
            if (sourceFiles.Count > 0)
            {
                List<Assembly> sourceAssemblies = CompileSourceFilePlugins(sourceFiles);
                foreach (Assembly a in sourceAssemblies)
                    CreatePluginsFromAssembly(a, plugins);
            }

            // remove any duplicates
            plugins = plugins.Distinct().ToList();

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
        /// Load all of our plugins.
        /// </summary>
        private void LoadPlugins()
        {
            if (IsDebug)
                Console.Notice("Starting plugin loading.");

            // find and instantiate all plugins
            Plugin[] allPlugins = FindAndCreatePlugins();            

            // load all plugins
            int pluginsLoaded = 0;
            foreach (Plugin p in allPlugins)
            {
                try
                {
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
                    "Plugin loading completed. {0} of {1} plugins were loaded.",
                    pluginsLoaded,
                    allPlugins.Length
                ));

            // set plugin statuses
            LoadPluginStatuses();

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

            int runningPlugins = 0;
            XmlNodeList statusSettings = root.SelectNodes("plugins/add");
            if (statusSettings.Count > 0)
            {
                foreach (XmlNode statusSetting in statusSettings)
                {
                    // get status
                    string pluginName = statusSetting.Attributes["key"].Value;
                    PluginStatus pluginStatus = (statusSetting.Attributes["value"].Value == "On" ? PluginStatus.On : PluginStatus.Off);

                    // set status
                    SetPluginStatus(pluginName, pluginStatus);

                    // record if it's on
                    if (pluginStatus == PluginStatus.On)
                        runningPlugins++;
                }

                // display how many plugins are activated
                Console.Notice(string.Format("Plugin statuses loaded. {0} plugins are activated.", runningPlugins));
            }
        }

        /// <summary>
        /// Executed when the plugin directory has been modified.
        /// </summary>
        private void PluginDirectoryChanged(object sender, FileSystemEventArgs e)
        {
            if (IsDebug)
                Console.Notice(string.Format("The plugin '{0}' was created/modified/deleted.", e.Name));
            Console.Log("Plugin directory has changed. Reloading plugins.");
            ReloadPlugins();
        }

        /// <summary>
        /// Reloads all the plugins for the bot.
        /// </summary>
        public void ReloadPlugins()
        {
            // first save bot config
            SaveConfig();

            // then shutdown all of our plugins
            ShutdownPlugins();

            // clear all variables holding commands and events
            commandHelp.Clear();
            commandMap.Clear();

            // clear all event handlers
            foreach (KeyValuePair<dAmnPacketType, BotEventList> evt in eventMap)
                evt.Value.Clear();

            // clear all plugins
            botPlugins.Clear();
           
            // save current access levels
            Access.SaveAccessLevels();

            // load plugins again
            LoadPlugins();

            // clear in memory levels
            Access.ClearAccessLevels();

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

                // output debug info
                Console.Debug(string.Format("Plugin status for '{0}' changed to {1}", pluginName, status.ToString("G")));

                // set status and trigger appropriate plugin method
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
            threadPool.Add(t);
        }

        /// <summary>
        /// Restarts all registered plugins.
        /// </summary>
        private void RestartPlugins()
        {
            // call the restart method on each of our plugins
            Console.Notice("Restarting plugins. Please wait...");
            foreach (Plugin plugin in botPlugins.Values.ToArray())
            {
                try
                {
                    plugin.Restart();
                }
                catch (Exception ex)
                {
                    // log the exception
                    Console.Log(string.Format("Error occurred during plugin restart.\nPlugin Name: {0}\nException: {1}",
                        plugin.PluginName,
                        ex.ToString()));
                }
            }
            Console.Notice("All plugins have been restarted.");
        }

        /// <summary>
        /// Shuts down all registered plugins.
        /// </summary>
        private void ShutdownPlugins()
        {
            // call the close method on each of our plugins
            Console.Notice("Shutting down plugins. Please wait...");
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
            Console.Notice("All plugins have been shutdown.");
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
            DateTime lastPacket = DateTime.Now;

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
                    lastPacket = DateTime.Now;
                    Console.Debug("packet recevied: " + packet.ToString());
                    ProcessPacket(packet);
                }
                else
                {
                    // check to see if it's been too long since we've received a packet
                    if (DateTime.Now - lastPacket > packetWaitTime)
                    {
                        // process a connection closed packet
                        Console.Debug("packet wait time exceeded.");
                        dAmnServerPacket closedPacket = new dAmnServerPacket(dAmnServerPacket.Parse("disconnect\ne=socket closed\n"));
                        ProcessPacket(closedPacket);
                    }
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
                // sort event list in terms of priority
                if (EventListenerSorter != null)
                    listeners.Sort(new Comparison<KeyValuePair<Plugin,BotServerPacketEvent>>(EventListenerSorter));

                // the listeners may be modified so we'll load it up in our own copy
                Queue<KeyValuePair<Plugin, BotServerPacketEvent>> listenerQueue = new Queue<KeyValuePair<Plugin, BotServerPacketEvent>>(listeners);

                // trigger the event for each listener
                while (listenerQueue.Count > 0)
                {
                    // get the next listener
                    KeyValuePair<Plugin, BotServerPacketEvent> listener = listenerQueue.Dequeue();

                    // get listener components
                    Plugin plugin = listener.Key;
                    BotServerPacketEvent method = listener.Value;

                    // only trigger event if plugin is activated
                    if (plugin.Status == PluginStatus.On)
                    {
                        string ns = null;
                        if (!string.IsNullOrEmpty(packet.param))
                            ns = dAmn.DeformatChat(packet.param);

                        // output debug message
                        Console.Debug("executing event handler: " + method.Method.ToString());

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

            // create a new thread to check for plugins
            Thread pluginUpdateThread = new Thread(CheckForPluginUpdates);
            threadPool.Add(pluginUpdateThread);
            pluginUpdateThread.Start();

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

                    // output debug message
                    Console.Debug("IsRestarting set to true. Restarting bot.");

                    // restart plugins
                    RestartPlugins();
                }

                // start up the bot
                StartBot();
                
                // reset the restart flag
                IsRestarting = false;

                // run until we the listening thread has stopped
                if (listenThread != null && listenThread.IsAlive)
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
            bool connectSucess = false;
            try
            {
                connectSucess = dAmn.Connect(Username, Password);
            }
            catch (Exception ex)
            {
                Console.Log("Error connecting to the dAmn servers. " + ex.ToString());
            }

            // see results of trying to connect
            if (connectSucess)
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
                    if (AuthCookie == null)
                    {
                        // something happened where we couldn't retrieve the cookie...have to shut it down
                        Console.Warning("Unable to retrieve login cookie from dAmn servers.");
                        DisplayShutdownWarning();
                        return;
                    }
                    dAmn.AuthCookie = AuthCookie;
                    authTokenFromConfig = false;

                    // save new auth token to config
                    SaveConfig();

                    // try connecting again.
                    StartBot();
                }
                else
                {
                    DisplayShutdownWarning();
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

            // save our master config - if authcookie is null we never connected
            if (AuthCookie != null)
                SaveConfig();

            // save access levels
            Access.SaveAccessLevels();

            // call the close method on each of our plugins
            ShutdownPlugins();

            // wait for any extra threads to complete
            if (IsDebug)
                Console.Notice("Stopping threads in thread pool...");
            foreach (Thread t in threadPool)
            {
                if (t.IsAlive)
                {
                    if (!t.Join(threadJoinWaitTime))
                    {
                        Console.Log(string.Format("Thread '{0}' has not shut down within 1 min. Aborting execution.",
                            t.Name));
                        // abort thread and wait for it to terminate
                        t.Abort();
                        t.Join();
                    }
                }
            }
            if (IsDebug && threadPool.Count > 0)
                Console.Notice("All threads have completed.");

            // display parting message
            foreach (string str in ShutDownString)
                Console.Notice(str);

            // show that we're not running any more
            IsAlive = false;
        }

        /// <summary>
        /// Displays a shutdown warning to the user.
        /// </summary>
        private void DisplayShutdownWarning()
        {
            Console.Warning("Unable to connect to dAmn server!");
            Console.Warning("Check the bot core logs for any errors and try again.");
            Console.Warning("Bot will shutdown in 10 seconds.");
            Thread.Sleep(TimeSpan.FromSeconds(10.0));
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
                Chats[chatroomName].CloseLog();
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
        /// Changes the debug status for the bot.
        /// </summary>
        /// <param name="debugStatus">Debug status to set to.</param>
        public void ChangeDebugStatus(bool debugStatus)
        {
            IsDebug = debugStatus;
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
        /// Ensures that all required directories are present.
        /// </summary>
        private void EnsureDirectoryStructure()
        {
            // directories required for operation
            string[] requiredDirectories = 
            {
                Path.Combine(CurrentDirectory, "Plugins"),
                Path.Combine(CurrentDirectory, "Plugins\\_Off"),
                Path.Combine(CurrentDirectory, "Logs"),
                Path.Combine(CurrentDirectory, "Logs\\Core"),
                Path.Combine(CurrentDirectory, "Logs\\Chats"),
                Path.Combine(CurrentDirectory, "Config"),
            };

            try
            {
                foreach (string directory in requiredDirectories)
                {
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                }
            }
            catch (Exception ex)
            {
                Console.Warning("Error! Required directories not present and could not be created.");
                Console.Log(ex.ToString());
                throw;
            }
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

        #region Static Bootstrap Methods
        /// <summary>
        /// The running bot instance. WARNING! Use this sparingly, it introduces stronger
        /// coupling into the system. 
        /// </summary>
        public static Bot Instance
        {
            get
            {
                if (_Instance == null)
                    throw new NullReferenceException("The bot instance is null. You must call Setup() prior to using this instance.");
                return _Instance;
            }
            set
            {
                _Instance = value;
            }
        }
        private static Bot _Instance;

        /// <summary>
        /// Call this method to setup dependency injection for the Bot.
        /// </summary>
        /// <param name="container">Container to use.</param>
        public static void Setup(IUnityContainer container)
        {
            // set up
            container.RegisterType<IAccessLevel, AccessLevel>();
            container.RegisterType<IConsole, Console>(new InjectionConstructor());
            container.RegisterType<IdAmnSocket, dAmnSocket>();
            container.RegisterType<Bot>(new ContainerControlledLifetimeManager());     
            
            // save reference
            Instance = container.Resolve<Bot>();
        }        
        #endregion

        #region Update Methods
        /// <summary>
        /// Returns true if there was an update available. Otherwise, false.
        /// </summary>
        /// <returns>True if there was an update available. Otherwise, false.</returns>
        public static bool CheckForUpdate()
        {
            bool updateExists = false;

            try
            {
                // get the file contents
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(VersionUpdateUrl);
                request.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                string result = sr.ReadToEnd();
                sr.Close();
                response.Close();

                // make sure we got a valid response
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // make sure we don't have an empty string
                    if (string.IsNullOrEmpty(result))
                        throw new NullReferenceException("The file version contents were empty or null.");

                    // get the two versions
                    Version updateVersion = new Version(result.Trim());
                    Version currentVersion = new Version(Bot.Info["Version"].Trim());

                    // we have an update if the update version is higher
                    updateExists = updateVersion > currentVersion;
                }
            }
            catch { } // we'll swallow the error - we don't want a failure to get an update to crash the bot

            return updateExists;
        }

        /// <summary>
        /// Checks for new updates for existing plugins.
        /// </summary>
        public void CheckForPluginUpdates()
        {
            // get all plugins that have an update manifest url
            var plugins = from p in GetPlugins()
                          where p.IsUpdateable
                          select p;
            // create a list of manifests we've downloaded (so we don't download them multiple times)
            Dictionary<string, XDocument> downloadedManifests = new Dictionary<string, XDocument>();
            // create list of plugin names that have an update
            List<string> pluginsWithUpdates = new List<string>();

            // get all manifests for existing plugins
            foreach (var plugin in plugins)
            {
                // get xml
                try
                {
                    // get xml doc
                    string url = plugin.Manifest.UpdateManifestUrl;
                    XDocument doc = null;
                    if (downloadedManifests.ContainsKey(url))
                    {
                        doc = downloadedManifests[url]; // pull from cache if we have it
                    }
                    else
                    {
                        doc = XDocument.Load(url);  // download it and add to cache
                        downloadedManifests.Add(url, doc);
                    }

                    // create manifest using xml
                    Manifest manifest = Manifest.Create(plugin.PluginName, doc);

                    // if we have a newer version, and it is compatible with the bot, add to list
                    if (plugin.Manifest.Version < manifest.Version &&
                        manifest.IsCompatible(AssemblyVersion))
                        pluginsWithUpdates.Add(plugin.PluginName);
                }
                catch (Exception ex)
                {                    
                    Console.Log(string.Format("Error retrieving update for plugin '{0}': {1}", plugin.PluginName, ex.ToString()));
                    continue;
                }
            }

            // if there are updates, notify the user
            if (pluginsWithUpdates.Count > 0)
            {
                string pluginNames = string.Join(",", pluginsWithUpdates.ToArray());
                Console.Notice(string.Format("{0} new plugin updates founds for the plugins {1}.",
                    pluginsWithUpdates.Count,
                    pluginNames));
                Console.Notice("Run oberon.exe -plugin-update to update active plugins.");
            }
        }

        /// <summary>
        /// Updates plugins in the system using information from the plugin manifests.
        /// </summary>
        public void UpdatePlugins()
        {
            Console.Notice("Updating plugins...");

            // init variables
            WebClient client = new WebClient();
            string tempPath = Path.GetTempPath();

            // load all of our plugins
            LoadPlugins();            

            // cycle through each plugin
            List<string> folderNames = new List<string>();
            foreach (Plugin plugin in GetPlugins())
            {
                // if the plug isn't upadteable, or if we've downloaded the package for another
                // plugin in the same package, then skip
                if (!plugin.IsUpdateable || folderNames.Contains(plugin.FolderName))
                    continue;

                try
                {
                    // get new plugin manifest from the web (the manifest class will cache the url content)
                    Manifest newManifest = Manifest.Create(plugin.PluginName, plugin.Manifest.UpdateManifestUrl);

                    // if the version of the the downloaded manifest is newer, we have an update!
                    if (newManifest.Version > plugin.Manifest.Version)
                    {
                        // get path to where we want the downloaded file
                        string downloadedFile = Path.Combine(tempPath, Path.GetFileName(plugin.Manifest.UpdateUrl));

                        // download the new package
                        client.DownloadFile(plugin.Manifest.UpdateUrl, downloadedFile);

                        // extract zip to overwrite existing plugin. we can do this because our current 
                        // plugins were loaded as byte arrays from the original and shadow-copied to a 
                        // different location. 
                        using (ZipFile package = new ZipFile(downloadedFile))
                        {
                            package.ExtractAll(
                                Path.Combine(Bot.PluginPath, plugin.FolderName),
                                ExtractExistingFileAction.OverwriteSilently);
                        }

                        // we've downloaded the package, remember folder name
                        folderNames.Add(plugin.FolderName);
                    }

                    Console.Notice(string.Format("Plugin '{0}' updated successfully.", plugin.PluginName));
                }
                catch (Exception ex)
                {
                    Console.Warning(string.Format("Error updating plugin '{0}'. See bot log for more details.", plugin.PluginName));
                    Console.Log("Error updating plugin: " + ex.ToString());
                }
            }

            Console.Notice("Plugin updates complete.");
        }
        #endregion
    }
}