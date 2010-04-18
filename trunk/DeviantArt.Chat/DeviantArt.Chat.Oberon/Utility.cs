using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// Utility class to hold static functions that don't belong to any particular object.
    /// </summary>
    public static class Utility
    {
        #region Helper Methods
        /// <summary>
        /// SHA Encrypts a string.
        /// </summary>
        /// <param name="str">String to encrypt.</param>
        /// <returns>SHA1 Encrypted string.</returns>
        public static string SHA1(string str)
        {
            return BitConverter.ToString(SHA1Managed.Create().ComputeHash(Encoding.Default.GetBytes(str))).Replace("-", "");
        }

        /// <summary>
        /// Gets the operating system full version as a string.
        /// </summary>
        /// <returns>OS version.</returns>
        public static string GetOperatingSystemVersion()
        {
            System.OperatingSystem os = System.Environment.OSVersion;
            string osName = "Unknown";

            switch (os.Platform)
            {
                case System.PlatformID.Win32Windows:
                    switch (os.Version.Minor)
                    {
                        case 0:
                            osName = "Windows 95";
                            break;
                        case 10:
                            osName = "Windows 98";
                            break;
                        case 90:
                            osName = "Windows ME";
                            break;
                    }
                    break;
                case System.PlatformID.Win32NT:
                    switch (os.Version.Major)
                    {
                        case 3:
                            osName = "Windws NT 3.51";
                            break;
                        case 4:
                            osName = "Windows NT 4";
                            break;
                        case 5:
                            if (os.Version.Minor == 0)
                                osName = "Windows 2000";
                            else if (os.Version.Minor == 1)
                                osName = "Windows XP";
                            else if (os.Version.Minor == 2)
                                osName = "Windows Server 2003";
                            break;
                        case 6:
                            osName = "Windows Vista";
                            if (os.Version.Minor == 0)
                                osName = "Windows Vista";
                            else if (os.Version.Minor == 1)
                                osName = "Windows 7";

                            break;

                    }
                    break;
            }

            return osName + ", " + os.Version.ToString();
        }

        /// <summary>
        /// Get all files in directory recursively.
        /// </summary>
        /// <param name="dirInfo">Directory to search.</param>
        /// <param name="searchPattern">Search pattern to use.</param>
        /// <returns>Files found.</returns>
        public static IEnumerable<FileInfo> GetFilesRecursive(DirectoryInfo dirInfo, string searchPattern)
        {
            foreach (DirectoryInfo di in dirInfo.GetDirectories())
                foreach (FileInfo fi in GetFilesRecursive(di, searchPattern))
                    yield return fi;

            foreach (FileInfo fi in dirInfo.GetFiles(searchPattern))
                yield return fi;
        }

        /// <summary>
        /// Strings that may follow or precede the username in a sentence
        /// </summary>
        private static string[] SentenceStrings = new string[] { 
            ",", ".", "<b>", "</b>", "<i>", "</i>", ":", "!", "?", "\"", "'", "(", ")", "=", "-", "~", "`", "^", "$", "[", "]" 
        };

        /// <summary>
        /// Returns true if the references the username in list.
        /// </summary>
        /// <param name="message">Message to check.</param>
        /// <param name="usernames"></param>
        /// <param name="parseType">Way to parse usernames.</param>
        /// <param name="foundUsername">User that was found, if any.</param>
        /// <returns>True if references user in list, otherwise false.</returns>
        public static bool IsMessageToUserInList(string message, List<string> usernames, MsgUsernameParse parseType, out string foundUsername)
        {
            foundUsername = null;
            foreach (string username in usernames)
            {
                if (IsMessageToUser(message, username, parseType))
                {
                    foundUsername = username;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the references the username provided.
        /// </summary>
        /// <param name="message">Message to check.</param>
        /// <param name="username">Username to use.</param>
        /// <param name="parseType">Way to parse username.</param>
        /// <returns>True if references user, otherwise false.</returns>
        public static bool IsMessageToUser(string message, string username, MsgUsernameParse parseType)
        {
            bool result = false;
            switch (parseType)
            {
                case MsgUsernameParse.Lazy:
                    result = message.Contains(username);
                    break;
                case MsgUsernameParse.Smart:
                    foreach (string str in SentenceStrings)
                    {
                        if (message.Contains(username + str) || message.Contains(str + username))
                        {
                            result = true;
                            break;
                        }
                    }
                    break;
                case MsgUsernameParse.Strict:
                    result = message.StartsWith(username + ":");
                    break;
            }

            return result;
        }

        /// <summary>
        /// Determines if the path contains the provided directory.
        /// </summary>
        /// <param name="path">Path to examine.</param>
        /// <param name="directory">Directory to check for.</param>
        /// <returns>True if the path containst the provided directory. Otherwise false.</returns>
        public static bool PathContainsDirectory(string path, string directory)
        {
            List<string> directories = new List<string>(path.Split(Path.DirectorySeparatorChar));
            return directories.Contains(directory);
        }

        /// <summary>
        /// Replaces the tokens {channel}, {ns}, {from} with their substitutions.
        /// </summary>
        /// <param name="message">Message to format.</param>
        /// <param name="chatroom">Chatroom.</param>
        /// <param name="from">Message is from.</param>
        /// <returns>Formatted string with tokens replaced.</returns>
        public static string FormatMessage(string message, string chatroom, string from)
        {
            message = message.Replace("{channel}", chatroom);
            message = message.Replace("{ns}", chatroom);
            message = message.Replace("{from}", from);

            return message;
        }
        #endregion

        #region Extension Methods
        /// <summary>
        /// Finds the value associated with key. If it doesn't exist returns the default value for the type.
        /// </summary>
        /// <typeparam name="K">Key type for the dictionary.</typeparam>
        /// <typeparam name="V">Value type for the the dictionary.</typeparam>
        /// <param name="dic">Dictionary object.</param>
        /// <param name="key">Key to look for.</param>
        /// <returns>Value if found, otherwise default value for the type.</returns>
        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dic, K key)
        {
            return dic.GetValueOrDefault(key, default(V));
        }

        /// <summary>
        /// Finds the value associated with key. If it doesn't exist returns the default value.
        /// </summary>
        /// <typeparam name="K">Key type for the dictionary.</typeparam>
        /// <typeparam name="V">Value type for the the dictionary.</typeparam>
        /// <param name="dic">Dictionary object.</param>
        /// <param name="key">Key to look for.</param>
        /// <param name="defaultVal">Default value to return if key is not found.</param>
        /// <returns>Value if found, otherwise default value.</returns>
        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dic, K key, V defaultVal)
        {
            V ret;
            bool found = dic.TryGetValue(key, out ret);
            if (found)
                return ret;
            else
                return defaultVal;
        }
        #endregion
    }

    /// <summary>
    /// Enum to determine how to parse the username in a message.
    /// </summary>
    public enum MsgUsernameParse
    {
        /// <summary>
        /// Lazy, if the username appears anywhere in the string.
        /// </summary>
        Lazy,

        /// <summary>
        /// Checks for user name followed by punctuation.
        /// </summary>
        Smart,

        /// <summary>
        /// Checks for username followed by :
        /// </summary>
        Strict
    }
}
