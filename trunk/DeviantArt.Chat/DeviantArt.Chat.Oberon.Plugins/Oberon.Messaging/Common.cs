using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Class that holds methods that are useful for messaging applications.
    /// </summary>
    internal static class Common
    {
        /// <summary>
        /// Strings that may follow or precede the username in a sentence
        /// </summary>
        public static string[] SentenceStrings = new string[] { 
            ",", ".", "<b>", "</b>", "<i>", "</i>", ":", "!", "?", "\"", "'", "(", ")", "=", "-", "~", "`", "^", "$", "[", "]" 
        };

        /// <summary>
        /// Returns true if the references the username provided. Checks using sentenceStrings
        /// variable so that we don't reply whenever the name appears.
        /// </summary>
        /// <param name="username">Username to use.</param>
        /// <param name="message">Message to check.</param>
        /// <returns>True if to a user, otherwise false.</returns>
        public static bool IsToUser(string username, string message)
        {
            foreach (string str in SentenceStrings)
            {
                if (message.Contains(username + str) || message.Contains(str + username))
                    return true;
            }
            return false;
        }
    }
}
