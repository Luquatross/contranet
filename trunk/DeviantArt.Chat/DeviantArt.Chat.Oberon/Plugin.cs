﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using DeviantArt.Chat.Library;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
            get { return System.IO.Path.Combine(Bot.PluginPath, FolderName + "\\" + FolderName + ".dat"); }
        }

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

        /// <summary>
        /// Method that is called once the plugin is loaded. Most of the plugin's initialization
        /// is handled here (for example, registering for events and commands).
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Converts the provided string into an array of arguments.
        /// The delimiter is a space.
        /// </summary>
        /// <param name="data">Data to convert.</param>
        /// <returns>Array of arguments.</returns>
        protected string[] GetArgs(string data)
        {
            return data.Trim().Split(' ');
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
        /// <param name="commandName">Command name to registere for.</param>
        /// <param name="commandMethod">Method that will be executed when command is encountered.</param>
        /// <param name="help">Help text for command.</param>
        protected void RegisterCommand(string commandName, BotCommandEvent commandMethod, string help)
        {
            Bot.AddCommandListener(commandName, this, commandMethod, help);
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
    }
}
