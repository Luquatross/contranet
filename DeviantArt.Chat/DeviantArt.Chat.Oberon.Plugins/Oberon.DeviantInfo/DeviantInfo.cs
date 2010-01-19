using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Oberon;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace Oberon.DeviantInfo
{
    /// <summary>
    /// Plugin that gets different info from DeviantArt website.
    /// </summary>
    public class DeviantInfo : Plugin
    {
        #region Private Variables
        private string _PluginName = "dA";
        private string _FolderName = "dA";        
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
        /// <summary>
        /// Gets the content of a page.
        /// </summary>
        /// <param name="url">URL to retrieve.</param>
        /// <returns>Content of a page at the given URL</returns>
        /// <exception cref="TimeoutEception">Thrown when it takes longer than 30 seconds to retrieve the page.</exception>
        private string GetPageContents(string url)
        {
            // create request and get response
            try
            {
                WebRequest request = HttpWebRequest.Create(url);
                request.Timeout = 30000; // 30 seconds
                WebResponse response = request.GetResponse();

                // get string from response
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (WebException ex)
            {
                // if it was a time out, rethrow it, as we have no xml
                if (ex.Status == WebExceptionStatus.Timeout)
                    throw new TimeoutException("It's taking a while to find the information. Try again in a second.", ex);

                // if it was a bad request, throw the exception
                throw;
            }
        }
        #endregion

        #region Plugin Methods
        public override void Load()
        {
            // register commands
            RegisterCommand("rdeviant", new BotCommandEvent(RandomDeviant), new CommandHelp("Displays a random deviant", "rdeviant"), (int)PrivClassDefaults.Guests);
            RegisterCommand("rdev", new BotCommandEvent(RandomDeviation), new CommandHelp("Displays a random deviation.", "rdev"), (int)PrivClassDefaults.Guests);
            RegisterCommand("thumb", new BotCommandEvent(ThumbInfo), new CommandHelp("Displays information on a deviation using the thumb number as input.", "thumb [thumbnumber]"), (int)PrivClassDefaults.Guests);
            RegisterCommand("devinfo", new BotCommandEvent(DevInfo), new CommandHelp("View information on a deviant from their dA page.", "devinfo [dA username]"), (int)PrivClassDefaults.Guests);
        }
        #endregion

        #region Command Handlers
        private void RandomDeviation(string ns, string from, string message)
        {

        }

        private void ThumbInfo(string ns, string from, string message)
        {

        }

        private void RandomDeviant(string ns, string from, string message)
        {
            string content = GetPageContents("http://www.deviantart.com/random/deviant");

            // get user
            string who = Regex.Match(content, "<title>(.*) on deviantART<\\/title>").Result("$1");

            // get page contents. strip out anything around dev info stuff
            content = content.Substring(content.IndexOf("Devious Info", StringComparison.InvariantCultureIgnoreCase));
            content = content.Substring(0, content.IndexOf("</div>", content.LastIndexOf("</ul>")));

            MatchCollection matches = Regex.Matches(content, "<li class=\"f(\\sa)?\">(.*?)<\\/li>");
            StringBuilder info = new StringBuilder(string.Format("<b>[<u>Random Deviant</u>]</b><br>:icon{0}: :dev{0}:<sup><ul>", who));
            foreach (Match m in matches)
            {
                // fix links to instant messengers
                string infoLine = Regex.Replace(m.Value, "\\sclass=\"(.*?)\"", "");
                infoLine = Regex.Replace(infoLine, "<a\\shref=\"mailto:(.*?)\">(.*?)</a>", "$1");
                infoLine = Regex.Replace(infoLine, "<a\\shref=\"aim:(.*?)\">(.*?)</a>", "$2");
                infoLine = Regex.Replace(infoLine, "<a\\shref=\"skype:(.*?)\">(.*?)</a>", "$2");

                info.Append(infoLine);
            }
            info.Append("</ul></sup>");
            Say(ns, info.ToString());
        }

        private void DevInfo(string ns, string from, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                ShowHelp(ns, from, "devinfo");
                return;
            }

            string who = Regex.Replace(message, @"[^\w\d\-]", "");

            // get page contents. strip out anything around dev info stuff
            string content = GetPageContents(string.Format("http://{0}.deviantart.com/", who));
            content = content.Substring(content.IndexOf("Devious Info", StringComparison.InvariantCultureIgnoreCase));
            content = content.Substring(0, content.IndexOf("</div>", content.LastIndexOf("</ul>")));

            MatchCollection matches = Regex.Matches(content, "<li class=\"f(\\sa)?\">(.*?)<\\/li>");
            if (matches.Count > 0)
            {
                StringBuilder info = new StringBuilder(string.Format(":icon{0}: :dev{0}:<sup><ul>", who));
                foreach (Match m in matches)
                {
                    // fix links to instant messengers
                    string infoLine = Regex.Replace(m.Value, "\\sclass=\"(.*?)\"", "");
                    infoLine = Regex.Replace(infoLine, "<a\\shref=\"mailto:(.*?)\">(.*?)</a>", "$1");
                    infoLine = Regex.Replace(infoLine, "<a\\shref=\"aim:(.*?)\">(.*?)</a>", "$2");
                    infoLine = Regex.Replace(infoLine, "<a\\shref=\"skype:(.*?)\">(.*?)</a>", "$2");

                    info.Append(infoLine);
                }
                info.Append("</ul></sup>");
                Say(ns, info.ToString());
            }
            else
            {
                Respond(ns, from, string.Format("User {0} does not exist.", who));
            }
        }
        #endregion
    }
}
