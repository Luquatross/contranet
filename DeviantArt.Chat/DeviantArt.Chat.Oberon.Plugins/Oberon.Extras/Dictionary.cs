using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Oberon.Plugins.DictService;
using System.ServiceModel;
using System.Text.RegularExpressions;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Plugin that lets a user find the definition of a word.
    /// </summary>
    public class Dictionary : Plugin
    {
        #region Private Variables
        private string _PluginName = "Dictionary";
        private string _FolderName = "Extras";
        private string _DictionaryId = "wn"; // use the wordnet dictionary - the most sensible one

        /// <summary>
        /// Dictionary service object. Used to get definitions.
        /// </summary>
        private DictService.DictService DictionaryClient = new DeviantArt.Chat.Oberon.Plugins.DictService.DictService();

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
        /// Retrieves the definition from the web service.
        /// </summary>
        /// <param name="word">Word to find.</param>
        /// <returns>Definition of the word. If there isn't one, null.</returns>
        private string GetDefinition(string word)
        {            
            WordDefinition definition = DictionaryClient.DefineInDict(_DictionaryId, word);
            if (definition.Definitions.Length == 0)
                return null;
            else
                return definition.Definitions[0].WordDefinition;
        }

        /// <summary>
        /// Parses the response so that it is in a chat-friendly format.
        /// </summary>
        /// <param name="definition">Definition to parse.</param>
        /// <returns>Chat-friendly definition.</returns>
        private string ParseDefinition(string definition)
        {           
            // replace each number with li tag
            definition = Regex.Replace(definition, "[0-9]: ", "<li>");
            
            // get each of the tokens
            List<string> tokens = new List<string>(definition.Split(
                new string[] { "<li>" }, StringSplitOptions.RemoveEmptyEntries));
            
            // start generating output
            StringBuilder output = new StringBuilder();            
            output.Append("<ol>");

            // first token is the word plus noun or verb, etc. Remove it.
            tokens.RemoveAt(0);

            // rest is definitions
            foreach (string token in tokens)
                output.AppendFormat("<li>{0}</li>", token);
            output.Append("</ol>");

            return output.ToString();
        }
        #endregion

        #region Plugin Methods
        public override void Load()
        {
            RegisterCommand("define", new BotCommandEvent(Define), new CommandHelp(
                "Displays the definition of a word.",
                "define [word] - displays a word's definition."), (int)PrivClassDefaults.Guests);
        }
        #endregion

        #region Command Handlers
        private void Define(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            if (args.Length != 1)
            {
                ShowHelp(ns, from, "define");
                return;
            }

            // get the definition of the word
            string word = message;
            string definition = GetDefinition(word);

            if (string.IsNullOrEmpty(definition))
            {
                Say(ns, string.Format("<b>No definition was found for <u>{0}</u>.</b>", word));
            }
            else
            {
                string say = "<b><u>" + word + "</u></b>:<br /><sub>" + ParseDefinition(definition) + "</sub>";
                Say(ns, say);
            }
        }
        #endregion
    }
}
