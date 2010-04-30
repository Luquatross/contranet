using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Plugin that checks the proper spelling of a word.
    /// </summary>
    public class SpellCheck : Plugin
    {
        #region Private Variables
        private string _PluginName = "Spell Checker";
        private string _FolderName = "Extras";
        private string _SpellCheckUrl = "http://www.spellcheck.net/cgi-bin/spell.exe?action=CHECKWORD&string=";
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
        public string GetWebPage(string word)
        {
            string url = _SpellCheckUrl + HttpUtility.UrlEncode(word);

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
                // if it was a time out, rethrow it, as we have no response
                if (ex.Status == WebExceptionStatus.Timeout)
                    throw new TimeoutException("The spell checker is being slow right now. Try again in a second.", ex);
                else
                    throw;
            }
        }
        #endregion

        #region Plugin Methods
        public override void Load()
        {
            // register command
            RegisterCommand("spell", new BotCommandEvent(Spell), new CommandHelp(
                "Run spell check on a word.", "spell [word]"), (int)PrivClassDefaults.Guests);
        }
        #endregion

        #region Command Handlers
        public void Spell(string ns, string from, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                Respond(ns, from, "Please give a word to check.");
                return;
            }

            try
            {
                string word = message;
                string page = GetWebPage(word);
                Match match = Regex.Match(page, string.Format("<font color=#006600><B>\"{0}\" is spelled correctly.<\\/B><\\/font>", word));
                if (match != null && match.Success)
                {
                    Respond(ns, from, string.Format("\"<b>{0}</b>\" is spelt correctly.", word));
                }
                else
                {
                    string[] bo = page.Split(new string[] { "BLOCKQUOTE>" }, StringSplitOptions.None);
                    string bo1 = bo[1];
                    bo1 = DeviantArt.Chat.Library.StringHelper.StripTags(bo1);
                    bo1 = bo1.Replace("</", "");
                    bo1 = bo1.Replace("\r", "");
                    bo1 = bo1.Replace("\n", ", ");
                    bo1 = bo1.Trim().Trim(',').Trim();
                    Respond(ns, from, string.Format(
                        "\"<b>{0}</b>\" is spelt incorrectly.<br>Suggested words<br><sub>{1}</sub>",
                        word,
                        bo1));
                }
            }
            catch (TimeoutException ex)
            {
                // spell checker is taking too long - let user know
                Respond(ns, from, ex.Message);
            }
        }
        #endregion
    }
}
