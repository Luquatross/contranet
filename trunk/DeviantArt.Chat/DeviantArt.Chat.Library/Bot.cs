using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.Security.Cryptography;

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
        public string Session { get; private set; }

        protected string[] ShutDownString = new string[]
        {
            "Bot has quit.",
            "Bye bye!"
        };

        public Bot()
        {
            // Generate a session ID code.
            this.Session = Utils.SHA1(DateTime.Now.Ticks.ToString());

            // Our start time is here.
            this.Start = DateTime.Now;
        }

        protected void LoadConfig(XmlDocument config)
        {
            XmlNode root = config.DocumentElement;

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
    }
}
