using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// Status of the plugin. Can be either on or off.
    /// </summary>
    public enum PluginStatus
    {
        On, Off
    }

    public abstract class Plugin
    {
        /// <summary>
        /// Reference to the Bot instance that is running
        /// </summary>
        protected Bot Bot = Bot.Instance;

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
        /// 
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="commandMethod"></param>
        protected void RegisterCommand(string commandName, BotCommandEvent commandMethod)
        {
            Bot.AddCommandListener(commandName, this, commandMethod);
        }
    }
}
