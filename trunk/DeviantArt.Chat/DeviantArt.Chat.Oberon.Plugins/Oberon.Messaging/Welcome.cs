using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Oberon.Collections;
using DeviantArt.Chat.Library;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Welcome plugin to allow welcome message for a chatroom.
    /// </summary>
    public class Welcome : Plugin
    {
        #region Private Variables
        private string _PluginName = "Welcome Message";
        private string _FolderName = "Messaging";

        /// <summary>
        /// Welcome messages we will be storing.
        /// </summary>
        private RoomSettingCollection<string> WelcomeMessages
        {
            get
            {
                // retrieve welcome messages from settings so that why are stored when 
                // the bot shuts down. if they haven't been created already, add them.
                if (!Settings.ContainsKey("WelcomeMessages"))
                {
                    Settings["WelcomeMessages"] = new RoomSettingCollection<string>(null);
                }
                return (RoomSettingCollection<string>)Settings["WelcomeMessages"];
            }
        }
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

        #endregion

        #region Plugin Methods
        public override void Load()
        {
            // tie into the join event so we can detect when a user has entered the chatroom
            RegisterEvent(dAmnPacketType.MemberJoin, new BotServerPacketEvent(UserJoined));

            LoadSettings();
        }

        public override void Close()
        {
            SaveSettings();
        }
        #endregion

        #region Event & Command Handlers
        private void UserJoined(string chatroom, dAmnServerPacket packet)
        {
            
        }

        private void WelcomeManage(string ns, string from, string message)
        {

        }
        #endregion
    }
}
