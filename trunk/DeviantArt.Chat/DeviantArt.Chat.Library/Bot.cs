using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace DeviantArt.Chat.Library
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
        public HttpCookie Cookie { get; private set; }
        public Console Console { get; private set; }
        public string SystemString { get; private set; }
        public Timer Timer { get; private set; }

        public string Session { get; private set; }

        public string[] ShutDownString = new string[]
        {
            "Bot has quit.",
            "Bye bye!"
        };

        private string ConfigPath
        {
            get
            {
                string currentDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
                return System.IO.Path.Combine(currentDirectory, "\\Config\\Bot.config");
            }
        }

        public Bot(bool debugMode)
        {
            // Generate a session ID code.
            this.Session = Security.SHA1(DateTime.Now.Ticks.ToString());

            // Our start time is here.
            this.Start = DateTime.Now;

            // System information string
            this.SystemString = Security.GetOperatingSystemVersion();

            // Get a new console interface
            this.Console = new Console();

            // Tell the console the session code for logging purposes.
            this.Console.Session = this.Session;

            // Get a new timer class.
            this.Timer = new Timer(this);

            // Some introduction messages! We've already done quite a bit but only introduce things here...
            this.Console.Notice("Hey thar!");
            this.Console.Notice(string.Format("Loading {0} {1}{2} by {3}",
                this.Info["Name"],
                this.Info["Version"],
                this.Info["Status"],
                this.Info["Author"]));
            if (debugMode)
            {
                // This is for when we're running in debug mode.
                Console.Notice("Running in debug mode!");
                Console.Notice("Session ID: " + Session);
            }

            if (debugMode) Console.Notice("Loading bot config file.");
            // Loading the config file
            LoadConfig();
            if (debugMode) Console.Notice("Config loaded, loading events system.");
        }

        public void LoadConfig()
        {
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

            // get about string
            AboutString = root.SelectSingleNode("about").InnerText;

            // get the auto joins
            List<string> autoJoins = new List<string>();
            XmlNodeList autoJoinNodes = root.SelectNodes("autoJoins/add");
            foreach (XmlNode joinNode in autoJoinNodes)
                autoJoins.Add(joinNode["channel"].Value);
            AutoJoin = autoJoins.ToArray();

            // get the cookie
            Cookie = new HttpCookie("BotCookie");
            XmlNodeList cookieData = root.SelectNodes("cookieData/add");
            foreach (XmlNode data in cookieData)
                Cookie.Values.Add(data["name"].Value, data["value"].Value);
        }

        public void SaveConfig()
        {
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
                            new XElement("add", new XAttribute("key", "owner"), new XAttribute("value", Owner))
                        ),
                        new XElement("about", new XCData(AboutString)),
                        new XElement("autoJoins", autoJoinsElements.ToArray()),
                        new XElement("cookieData",
                            new XElement("add", new XAttribute("key", "uniqueid"), new XAttribute("value", Cookie.Values["uniqueid"])),
                            new XElement("add", new XAttribute("key", "visitcount"), new XAttribute("value", Cookie.Values["visitcount"])),
                            new XElement("add", new XAttribute("key", "visittime"), new XAttribute("value", Cookie.Values["visittime"])),
                            new XElement("add", new XAttribute("key", "username"), new XAttribute("value", Cookie.Values["username"])),
                            new XElement("add", new XAttribute("key", "authtoken"), new XAttribute("value", Cookie.Values["authtoken"])),
                        )
                    )
                );
            botConfig.Save(ConfigPath);
        }

        public void Network(bool sec)
        {

        }
    }
}
