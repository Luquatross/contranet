using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// PLugin that lets a user test a regex pattern.
    /// </summary>
    public class Regexes : Plugin
    {
        #region Private Variables
        private string _PluginName = "Regex";
        private string _FolderName = "Extras";        
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

        public override void Load()
        {
            // register our comment with bot
            RegisterCommand("regex", new BotCommandEvent(Regex), new CommandHelp(
                "Tests regular expressions.",
                "regex [pattern] [source]<br />" +
                "<b>Example:</b> !regex /^def/ abcdef"), (int)PrivClassDefaults.Guests);
        }

        private void Regex(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string pattern = GetArg(args, 0);
            string source = ParseArg(message, 1);

            if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(source))
            {
                Respond(ns, from, "You must give an expression and subject to search.");
                return;
            }

            // run regex and see if we have results
            MatchCollection results = System.Text.RegularExpressions.Regex.Matches(source, pattern);
            if (results.Count == 0)
            {
                Respond(ns, from, "Found 0 matches.");
                return;
            }

            // if there are results, display them
            StringBuilder say = new StringBuilder(string.Format(
                "<b><u>Regex Results for {0}</u></b><sub><ul>", pattern));
            foreach (Match m in results)
                say.AppendFormat("<li>{0}</li>", m.Value);
            say.Append("</ul></sub>");
            Say(ns, say.ToString());
        }
    }
}
