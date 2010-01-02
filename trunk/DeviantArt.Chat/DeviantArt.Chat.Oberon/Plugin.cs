using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using DeviantArt.Chat.Library;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Resources;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// Status of the plugin. Can be either on or off.
    /// </summary>
    public enum PluginStatus
    {
        On, Off
    }

    /// <summary>
    /// Base class for all plugins.
    /// </summary>
    public abstract class Plugin
    {
        #region Private Variables
        /// <summary>
        /// Resource Manager for this plugin.
        /// </summary>
        private ResourceManager ResourceManager;
        #endregion

        #region Protected Variables & Properties
        /// <summary>
        /// Reference to the Bot instance that is running
        /// </summary>
        protected Bot Bot = Bot.Instance;        

        /// <summary>
        /// Settings for this plugin.
        /// </summary>
        protected Hashtable Settings = new Hashtable();        

        /// <summary>
        /// Gets the file that the settings are saved and retrieved from.
        /// </summary>
        protected string SettingsFile
        {
            get { return System.IO.Path.Combine(Bot.PluginPath, FolderName + "\\" + GetPluginKey() + ".dat"); }
        }

        /// <summary>
        /// The path to the folder where this plugin resides.
        /// </summary>
        protected string PluginPath
        {
            get { return Path.Combine(Bot.PluginPath, FolderName); }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Reference to the dAmn library, so we can send and received from the servers.
        /// </summary>
        public dAmnNET dAmn
        {
            get;
            set;
        }

        /// <summary>
        /// Status of the plugin. 
        /// </summary>
        public PluginStatus Status
        {
            get;
            set;
        }

        /// <summary>
        /// Retrieves the name for the plugin.
        /// </summary>
        public abstract string PluginName
        {
            get;
        }

        /// <summary>
        /// Folder name for this module
        /// </summary>
        public abstract string FolderName
        {
            get;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public Plugin()
        {
            // load the resource manager
            ResourceManager = GetResourceManager();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Method that is called once the plugin is loaded. Most of the plugin's initialization
        /// is handled here (for example, registering for events and commands).
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// This method is called when the bot is shutting down. Override this 
        /// method to perform logic when during shutdown (for example, saving
        /// plugin settings to the filesystem).
        /// </summary>
        public virtual void Close()
        {
        }        

        /// <summary>
        /// Get a unique key for this plugin. Is the plugin name with all spaces and special 
        /// characters removed.
        /// </summary>
        /// <returns>Plugin key.</returns>
        public string GetPluginKey()
        {
            return System.Text.RegularExpressions.Regex.Replace(PluginName, "[^a-zA-Z_]", "");
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Converts the provided string into an array of arguments.
        /// The delimiter is a space.
        /// </summary>
        /// <param name="data">Data to convert.</param>
        /// <returns>Array of arguments.</returns>
        protected string[] GetArgs(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return new string[] { };
            }
            else
            {
                // remove empty strings from list if there are any
                List<string> result = new List<string>(data.Trim().Split(' '));
                result.RemoveAll(s => string.IsNullOrEmpty(s));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Show help message to the user for a particular command.
        /// </summary>
        /// <param name="ns">Chatroom.</param>
        /// <param name="from">User to send message to.</param>
        /// <param name="command">Command to show help for.</param>
        protected void ShowHelp(string ns, string from, string command)
        {
            Bot.TriggerHelp(command, ns, from);
        }

        /// <summary>
        /// Send a response to a user.
        /// </summary>
        /// <param name="ns">Chatroom.</param>
        /// <param name="user">User to respond to.</param>
        /// <param name="message">Message to send.</param>
        protected void Respond(string ns, string user, string message)
        {
            dAmn.Say(ns, user + ": " + message);
        }

        /// <summary>
        /// This method lets you tie into a command that is received from a user. There are no
        /// preset commands. Any new commands are registered with this method. Only one method
        /// can be mapped for one command.
        /// </summary>
        /// <param name="commandName">Command name to register for.</param>
        /// <param name="commandMethod">Method that will be executed when command is encountered.</param>
        /// <param name="help">Help text for command.</param>
        /// <param name="defaultAccessLevel">Default access level for this command.</param>
        protected void RegisterCommand(string commandName, BotCommandEvent commandMethod, CommandHelp help, int defaultAccessLevel)
        {
            Bot.AddCommandListener(commandName, this, commandMethod, help, defaultAccessLevel);
        }

        /// <summary>
        /// Updates the help text for a registered command.
        /// </summary>
        /// <param name="commandName">Command name to update.</param>
        /// <param name="help">Help text for the command.</param>
        protected void RegisterCommandHelp(string commandName, CommandHelp help)
        {
            Bot.UpdateCommandHelp(commandName, help);
        }

        /// <summary>
        /// This method lets you hook into an event that occurs when a packet is received
        /// from the dAmn server.
        /// </summary>
        /// <param name="packetType">Type of packet to register for.</param>
        /// <param name="eventMethod">Method that will be executed when the packet type is encountered.</param>
        protected void RegisterEvent(dAmnPacketType packetType, BotServerPacketEvent eventMethod)
        {
            Bot.AddEventListener(packetType, this, eventMethod);
        }

        /// <summary>
        /// Load settings from the file system.
        /// </summary>
        protected void LoadSettings()
        {
            if (!File.Exists(SettingsFile))
                return;

            FileStream fs = new FileStream(SettingsFile, FileMode.Open, FileAccess.Read);
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                Settings = (Hashtable)bf.Deserialize(fs);
            }
            finally
            {
                fs.Close();
            }
        }

        /// <summary>
        /// Save settings to the file system.
        /// </summary>
        protected void SaveSettings()
        {
            if (Settings.Count == 0)
                return;

            FileStream fs = new FileStream(SettingsFile, FileMode.OpenOrCreate, FileAccess.Write);
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, Settings);
            }
            finally
            {
                fs.Close();
            }
        }

        /// <summary>
        /// Gets the resource manager for this plugin. Override in plugin class to return
        /// the actual resource manager. Default method returns null.
        /// </summary>
        /// <returns>Resource Manager.</returns>
        protected virtual ResourceManager GetResourceManager()
        {
            return null;
        }        

        /// <summary>
        /// Gets the command help from the Resource file using the provided keys.
        /// </summary>
        /// <param name="summaryKey">Key for the summary string.</param>
        /// <param name="usageKey">Key for the usage string.</param>
        /// <returns>CommandHelp object filled from the resource file.</returns>
        protected CommandHelp GetCommandHelp(string summaryKey, string usageKey)
        {
            if (ResourceManager == null)
                return null;

            return new CommandHelp(
                ResourceManager.GetString(summaryKey),
                ResourceManager.GetString(usageKey));
        }
        #endregion
    }
}
