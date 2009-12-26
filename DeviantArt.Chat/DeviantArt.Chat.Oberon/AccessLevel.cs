using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace DeviantArt.Chat.Oberon
{
    public enum PrivClassDefaults
    {
        Owner = 100,
        Operators = 99,
        Moderators = 75,
        Members = 50,
        Guests = 25,
        Banned = 1
    }

    public class PrivClass
    {
        /// <summary>
        /// Key for this priv class.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Priv class name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Access level associated with this priv class.
        /// </summary>
        public int AccessLevel { get; set; }

        /// <summary>
        /// Create a new priv class.
        /// </summary>
        /// <param name="name">Name for priv class.</param>
        /// <param name="level">Access level.</param>
        /// <returns>New privclass.</returns>
        public static PrivClass Create(string name, int level)
        {
            return new PrivClass { Key = name, Name = name, AccessLevel = level };
        }
    }

    public class AccessLevel
    {
        /// <summary>
        /// Holds list of user access levels.
        /// </summary>
        private Dictionary<string, int> UserAccessLevel = new Dictionary<string, int>();

        /// <summary>
        /// Holds list of command access levels.
        /// </summary>
        private Dictionary<string, int> CommandAccessLevel = new Dictionary<string, int>();

        /// <summary>
        /// File that holds user and command access.
        /// </summary>
        private string AccessFile
        {
            get { return System.IO.Path.Combine(Bot.CurrentDirectory, "Config\\Access.config"); }
        }

        /// <summary>
        /// Reference to bot.
        /// </summary>
        private Bot Bot;

        /// <summary>
        /// The default user access level when a user isn't registered with the bot.
        /// </summary>
        public int DefaultUserAccessLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Priv classes available. Below are the default values but these can be changed.
        /// </summary>
        public List<PrivClass> PrivClasses = new List<PrivClass>
        {
            new PrivClass { Key = "Owner", Name = "Owner", AccessLevel = 100 },
            new PrivClass { Key = "Operators", Name = "Operators", AccessLevel = 99 },
            new PrivClass { Key = "Moderators", Name = "Moderators", AccessLevel = 75 },
            new PrivClass { Key = "Members", Name = "Members", AccessLevel = 50 },
            new PrivClass { Key = "Guest", Name = "Guest", AccessLevel = 25 },
            new PrivClass { Key = "Banned", Name = "Banned", AccessLevel = 1 }
        };

        /// <summary>
        /// Constructor.
        /// </summary>
        public AccessLevel(Bot bot)
        {
            Bot = bot;

            // set bot owner to have maximum privileges no matter what
            SetUserLevel(Bot.Owner, (int)PrivClassDefaults.Owner);
        }

        /// <summary>
        /// Gets the user access level.
        /// </summary>
        public int GetUserLevel(string username)
        {
            if (UserAccessLevel.ContainsKey(username))
                return UserAccessLevel[username];
            else
                return DefaultUserAccessLevel;
        }

        /// <summary>
        /// Sets the user access level. Must be between 100 and 0.
        /// </summary>
        public void SetUserLevel(string username, int value)
        {
            if (value <= 100 && value >= 0)
            {
                if (UserAccessLevel.ContainsKey(username))
                    UserAccessLevel[username] = value;
                else
                    UserAccessLevel.Add(username, value);
            }
        }

        /// <summary>
        /// Sets the user access level using the named priv classes.
        /// </summary>        
        public void SetUserLevel(string username, string privClass)
        {
            int level = GetPrivClassAccessLevel(privClass);
            SetUserLevel(username, level);
        }

        /// <summary>
        /// Returns true if the priv class exists. Otherwise false.
        /// </summary>        
        public bool HasUserLevel(string privClass)
        {
            return (GetPrivClassAccessLevel(privClass) != 0);
        }

        /// <summary>
        /// Removes a user's access level.
        /// </summary>
        /// <param name="username">Username to remove.</param>
        public void RemoveUserLevel(string username)
        {
            UserAccessLevel.Remove(username);
        }

        /// <summary>
        /// Gets the command access level.
        /// </summary>
        public int GetCommandLevel(string command)
        {
            if (CommandAccessLevel.ContainsKey(command))
                return CommandAccessLevel[command];
            else
                return -1;
        }

        /// <summary>
        /// Sets the command access level.
        /// </summary>
        public void SetCommandLevel(string command, int value)
        {
            if (CommandAccessLevel.ContainsKey(command))
                CommandAccessLevel[command] = value;
            else
                CommandAccessLevel.Add(command, value);
        }

        /// <summary>
        /// Get the access level for a priv class.
        /// </summary>
        /// <param name="privClass">Priv Class to retrieve.</param>
        /// <returns>Access level.</returns>
        public int GetPrivClassAccessLevel(string privClass)
        {
            var accessLevel = from p in PrivClasses
                              where p.Name.ToLower() == privClass.ToLower()
                              select p.AccessLevel;
            return accessLevel.SingleOrDefault();
        }

        /// <summary>
        /// Updates a Priv Class's access level and updates all commands
        /// which also have that access level. 
        /// </summary>
        /// <param name="privClass"></param>
        /// <param name="value"></param>
        public void UpdatePrivClassAccessLevel(string privClass, int value)
        {
            // get the old access level and the priv class to update
            int oldAccessLevel = GetPrivClassAccessLevel(privClass);
            PrivClass priv = (from p in PrivClasses
                              where p.Name.ToLower() == privClass.ToLower()
                              select p).SingleOrDefault();             
            if (priv != null)
            {
                // update the priv class value
                priv.AccessLevel = value;

                string[] keys;

                // update commands that had this value
                keys = (from c in CommandAccessLevel
                        where c.Value == oldAccessLevel
                        select c.Key).ToArray();
                foreach (string key in keys)
                    CommandAccessLevel[key] = value;

                // update users who have this value
                keys = (from u in UserAccessLevel
                        where u.Value == oldAccessLevel
                        select u.Key).ToArray();
                foreach (string key in keys)
                    UserAccessLevel[key] = value;
            }
        }

        /// <summary>
        /// Add a new priv class.
        /// </summary>        
        public void AddPrivClass(PrivClass privClass)
        {
            PrivClasses.Add(privClass);
        }

        /// <summary>
        /// Removes a priv class.
        /// </summary>
        /// <param name="privClass">Priv class to remove.</param>
        public void RemovePrivClass(string privClassName)
        {
            PrivClass p = (from priv in PrivClasses
                           where priv.Name.ToLower() == privClassName.ToLower()
                           select priv).SingleOrDefault();
            if (p != null)
            {
                // don't delete owner or guest account
                if (p.Key == "Owner" || p.Key == "Guest")
                    return;
                PrivClasses.Remove(p);
            }
        }

        /// <summary>
        /// Load settings from config file.
        /// </summary>
        public void LoadAccessLevels()
        {
            // if file doesn't exist, create one with the current settings
            if (!File.Exists(AccessFile))
                return;            

            // create xml document
            XmlDocument configDoc = new XmlDocument();
            configDoc.Load(AccessFile);

            // get root element
            XmlNode root = configDoc.DocumentElement;

            // clear current data
            UserAccessLevel.Clear();
            CommandAccessLevel.Clear();

            // load user data
            XmlNodeList userDataNodes = root.SelectNodes("userData/add");
            foreach (XmlNode userNode in userDataNodes)
                UserAccessLevel.Add(
                    userNode.Attributes["key"].Value,
                    Convert.ToInt32(userNode.Attributes["value"].Value
                ));

            // load command data
            XmlNodeList commandDataNodes = root.SelectNodes("commandData/add");
            foreach (XmlNode commandNode in commandDataNodes)
                CommandAccessLevel.Add(
                    commandNode.Attributes["key"].Value,
                    Convert.ToInt32(commandNode.Attributes["value"].Value
                ));
        }

        /// <summary>
        /// Save settings to config file.
        /// </summary>
        public void SaveAccessLevels()
        {
            // if directory doesn't exist, create it
            string configDirectory = System.IO.Path.GetDirectoryName(AccessFile);
            if (!System.IO.Directory.Exists(configDirectory))
                System.IO.Directory.CreateDirectory(configDirectory);

            // get user data
            List<XElement> userData = new List<XElement>();
            foreach (KeyValuePair<string, int> item in UserAccessLevel)
                userData.Add(new XElement(
                    "add", 
                    new XAttribute("key", item.Key), 
                    new XAttribute("value", item.Value)));

            // get command data
            List<XElement> commandData = new List<XElement>();
            foreach (KeyValuePair<string, int> item in CommandAccessLevel)
                commandData.Add(new XElement(
                    "add",
                    new XAttribute("key", item.Key),
                    new XAttribute("value", item.Value)));

            // construct document in linq to xml
            XDocument accessConfig = new XDocument(
                new XDeclaration("1.0", "utf-8", ""),
                    new XElement("privilegeSettings",
                        new XElement("userData", userData),
                        new XElement("commandData", commandData)
                    )
                );
    
            accessConfig.Save(AccessFile);
        }

        /// <summary>
        /// Determins if a user has access to a command/
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <param name="command">Command to check.</param>
        /// <returns>True if user has access to command, otherwise false.</returns>
        public bool UserHasAccess(string username, string command)
        {
            return (GetUserLevel(username) >= GetCommandLevel(command));
        }

        /// <summary>
        /// Gets all user acess levels.
        /// </summary>
        /// <returns>Dictionary of users and access levels.</returns>
        public Dictionary<string, int> GetAllUserAccessLevels()
        {
            return UserAccessLevel;
        }
    }
}
