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

namespace DeviantArt.Chat.Contra
{
    public class Bot
    {
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

        private dAmnNET dAmn;
        private Thread listenThread;
        private bool authTokenFromConfig = true;

        private string ConfigPath
        {
            get
            {
                string currentDirectory = System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location);
                return System.IO.Path.Combine(currentDirectory, "Config\\Bot.config");
            }
        }

        public Bot()
        {
            // Generate a session ID code.
            this.Session = Security.SHA1(DateTime.Now.Ticks.ToString());

            // Our start time is here.
            this.Start = DateTime.Now;

            // System information string
            this.SystemString = Security.GetOperatingSystemVersion();

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

            // Now we're ready to get some work done!
            Console.Notice("Ready!");
        }

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

        protected void GetConfigDataFromUser()
        {
            Console.Notice("You don't have a config file! Creating a new one...");
            Console.Message("Please enter the following information.");

            // get values from the user
            Username = Console.GetValue("Bot Username: ");
            Password = Console.GetValue("Bot Password: ");
            Trigger = Console.GetValue("Bot Trigger: ");
            Owner = Console.GetValue("Bot Owner: ");

            // get about string
            AboutString = "This is an about string.";

            // get auto-join channels
            Console.Message("What channels would you like your bot to join? Separate with commas.");
            AutoJoin = Console.GetValue("").Split(',');

            // set debug
            IsDebug = false;
        }

        private void Listen()
        {
            try
            {
                while (true)
                {
                    string packet = string.Empty;
                    try
                    {
                        packet = dAmn.Read();
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
                    // raise event
                    // process packet
                    if (!string.IsNullOrEmpty(packet))
                        Console.Notice(packet);
                }
            }
            catch (ThreadAbortException ex)
            {
                return;
            }
        }

        public void Run()
        {
            listenThread = new Thread(new ThreadStart(Listen));
            // try to connect!
            if (dAmn.Connect(Username, Password))
            {
                foreach (string channel in AutoJoin)
                {
                    dAmn.Join(channel);
                    dAmn.Say(channel, "Wassup!");
                    dAmn.Action(channel, "is happy the bot is coming along nicely.");
                }
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
    }
}
