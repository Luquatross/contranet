using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using info.lundin.Math;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Class the performs simple math calculations.
    /// </summary>
    public class Calc : Plugin
    {
        #region Private Variables
        private string _PluginName = "Calc";
        private string _FolderName = "Extras";

        /// <summary>
        /// Expression parser for the class.
        /// </summary>
        private ExpressionParser Parser = new ExpressionParser();
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

        #region Plugin Methods
        public override void Load()
        {
            // register our command with the bot
            RegisterCommand("calc", new BotCommandEvent(Calculate), new CommandHelp(
                "Solves simple math problems.",
                "calc [math expression]<br />" +
                "<b>Example:</b> calc 2 * 3 + 4"), (int)PrivClassDefaults.Guests);
        }
        #endregion

        #region Command Handlers
        private void Calculate(string ns, string from, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                Say(ns, "You must give a math problem to calculate.");
                return;
            }

            try
            {
                double result = Parser.Parse(message, new System.Collections.Hashtable());
                Respond(ns, from, "<b>Calculation Result:</b><code> " + result.ToString() + "</code>");
            }
            catch (Exception ex)
            {
                Respond(ns, from, "<b>Calc Error:</b> " + ex.Message);
            }
        }
        #endregion
    }
}
