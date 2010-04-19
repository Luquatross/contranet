using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Oberon.Collections
{
    /// <summary>
    /// Collection that manages chatroom settings hierarchically. That is, you can store
    /// a setting and have it apply globally, to a just a chatroom, to a privclass in a
    /// chatroom, or to a specific individual in a chatroom. That setting can be a string,
    /// a boolean, or whatever you want. You can call the <see cref="Set"/> method to set
    /// the a setting. Then when calling <see cref="Get"/> and (<see cref="GetIndividual"/>)
    /// it will automatically traverse the hierarchy and return the value you want. Since
    /// it is hierarchical, a user's setting overrides that of a priv class, and a priv class
    /// overrides that of a rooom, etc etc. 
    /// </summary>
    /// <typeparam name="T">Type of data to store for chatroom.</typeparam>
    [Serializable()]
    public class RoomSettingCollection<T>
    {
        #region Private Variables
        /// <summary>
        /// Collection to hold all of our settings for chatrooms.
        /// </summary>
        private Dictionary<string, T> Settings = new Dictionary<string, T>();

        /// <summary>
        /// Default setting for the collection. Typically a non-value, such as null or -1.
        /// </summary>
        private T DefaultSetting;

        /// <summary>
        /// Allow accessing of individual settings.
        /// </summary>
        public Dictionary<string, bool> AllowIndividualSetting = new Dictionary<string, bool>();

        /// <summary>
        /// Allow accessing of priv class settings.
        /// </summary>
        public Dictionary<string, bool> AllowPrivClassSettings = new Dictionary<string, bool>();
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="defaultSetting">Default setting to return when there are no matches.</param>
        public RoomSettingCollection(T defaultSetting)
        {
            DefaultSetting = defaultSetting;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets a value first for the room. If not set, checks the global setting.
        /// If not, returns the default setting.
        /// </summary>
        /// <param name="chatroom">Chatroom to look for.</param>
        /// <returns>Setting.</returns>
        public T Get(string chatroom)
        {
            if (Settings.ContainsKey("room_" + chatroom))
                return Settings["room_" + chatroom];
            else if (Settings.ContainsKey("all"))
                return Settings["all"];
            else
                return DefaultSetting;
        }

        /// <summary>
        /// Gets a value first for the user, then privclass, then room, then
        /// global setting. If none exist along that chain the default setting
        /// is returned.
        /// </summary>
        /// <param name="chatroom">Chatroom to look for.</param>
        /// <param name="username">Username setting might belong to.</param>
        /// <returns>Setting.</returns>
        public T Get(string chatroom, string username)
        {
            // get the user 
            User user = Bot.Instance.GetChatroom(chatroom)[username];

            // get room string
            string room = "room_" + chatroom + "_";

            if (Settings.ContainsKey(room + "user_" + username) && DoesAllowIndividualSettings(chatroom))
                return Settings[room + "user_" + username];
            if (Settings.ContainsKey(room + "priv_" + user.PrivClass) && DoesAllowPrivClassSettings(chatroom))
                return Settings[room + "priv_" + user.PrivClass];
            else 
                return Get(chatroom);
        }

        /// <summary>
        /// Gets global setting. Doesn't use heirarchy, returns DefaultSetting if not set.
        /// </summary>
        /// <param name="chatroom">Chatroom.</param>
        /// <param name="username">Username.</param>
        /// <returns>Setting.</returns>
        public T GetIndividualSetting(string chatroom, string username)
        {
            string key = "room_" + chatroom + "_" + "user_" + username;
            if (Settings.ContainsKey(key))
                return Settings[key];
            else
                return DefaultSetting;
        }

        /// <summary>
        /// Gets global setting. Doesn't use heirarchy, returns DefaultSetting if not set.
        /// </summary>
        /// <param name="chatroom">Chatroom.</param>
        /// <param name="privClass">PrivClass.</param>
        /// <returns>Setting.</returns>
        public T GetPrivClassSetting(string chatroom, string privClass)
        {
            string key = "room_" + chatroom + "_" + "priv_" + privClass;
            if (Settings.ContainsKey(key))
                return Settings[key];
            else
                return DefaultSetting;
        }

        /// <summary>
        /// Gets chatroom setting. Doesn't use heirarchy, returns DefaultSetting if not set.
        /// </summary>
        /// <param name="chatroom">Chatroom.</param>
        /// <returns>Setting.</returns>
        public T GetChatroomSetting(string chatroom)
        {
            string key = "room_" + chatroom;
            if (Settings.ContainsKey(key))
                return Settings[key];
            else
                return DefaultSetting;
        }

        /// <summary>
        /// Gets global setting. Doesn't use heirarchy, returns DefaultSetting if not set.
        /// </summary>
        /// <returns>Setting.</returns>
        public T GetGlobalSetting()
        {
            if (Settings.ContainsKey("all"))
                return Settings["all"];
            else
                return DefaultSetting;
        }

        /// <summary>
        /// Sets a global setting.
        /// </summary>
        /// <param name="setting">Setting.</param>
        public void Set(T setting)
        {
            if (Settings.ContainsKey("all"))
                Settings["all"] = setting;
            else
                Settings.Add("all", setting);
        }

        /// <summary>
        /// Set a chatroom value.
        /// </summary>
        /// <param name="chatroom">Chatroom setting will apply to.</param>
        /// <param name="setting">Setting.</param>
        public void Set(string chatroom, T setting)
        {
            if (Settings.ContainsKey("room_" + chatroom))
                Settings["room_" + chatroom] = setting;
            else
                Settings.Add("room_" + chatroom, setting);
        }

        /// <summary>
        /// Sets a priv class value for a chatroom.
        /// </summary>
        /// <param name="chatroom">Chatroom privclass is part of.</param>
        /// <param name="privClass">Privclass setting will apply to.</param>
        /// <param name="setting">Setting.</param>
        public void Set(string chatroom, string privClass, T setting)
        {
            // get room string
            string room = "room_" + chatroom + "_";

            if (Settings.ContainsKey(room + "priv_" + privClass))
                Settings[room + "priv_" + privClass] = setting;
            else
                Settings.Add(room + "priv_" + privClass, setting);
        }

        /// <summary>
        /// Sets an individual for a chatroom.
        /// </summary>
        /// <param name="chatroom">Chatroom user is a part of.</param>
        /// <param name="username">Username setting will apply to.</param>
        /// <param name="setting">Setting.</param>
        public void SetIndividual(string chatroom, string username, T setting)
        {
            // get room string
            string room = "room_" + chatroom + "_";

            if (Settings.ContainsKey(room + "user_" + username))
                Settings[room + "user_" + username] = setting;
            else
                Settings.Add(room + "user_" + username, setting);
        }

        /// <summary>
        /// Clear out all settings.
        /// </summary>
        public void Clear()
        {
            Settings.Clear();
        }

        /// <summary>
        /// Clears out all settings for just a chatroom.
        /// </summary>
        /// <param name="chatroom"></param>
        public void ClearChatroom(string chatroom)
        {
            List<string> keys = Settings.Keys.Where(k => k.Contains("room_" + chatroom)).ToList();
            foreach (string key in keys)
                Settings.Remove(key);
        }

        /// <summary>
        /// True if chatroom allows individual settings.
        /// </summary>
        /// <param name="chatroom">Chatroom to check.</param>
        /// <returns>True if chatroom allows individual settings.</returns>
        public bool DoesAllowIndividualSettings(string chatroom)
        {
            if (AllowIndividualSetting.ContainsKey(chatroom))
                return AllowIndividualSetting[chatroom];
            else
                return true;
        }

        /// <summary>
        /// True if chatroom allows priv class settings.
        /// </summary>
        /// <param name="chatroom">Chatroom to check.</param>
        /// <returns>True if chatroom allows privclass settings.</returns>
        public bool DoesAllowPrivClassSettings(string chatroom)
        {
            if (AllowPrivClassSettings.ContainsKey(chatroom))
                return AllowPrivClassSettings[chatroom];
            else
                return true;
        }

        /// <summary>
        /// Set the allow individual setting property.
        /// </summary>
        /// <param name="chatroom">Chatroom to set property for.</param>
        /// <param name="value">Value.</param>
        public void SetAllowIndividualSettings(string chatroom, bool value)
        {
            if (AllowIndividualSetting.ContainsKey(chatroom))
                AllowIndividualSetting[chatroom] = value;
            else
                AllowIndividualSetting.Add(chatroom, value);
        }

        /// <summary>
        /// Set the allow privclass property.
        /// </summary>
        /// <param name="chatroom">Chatroom to set property for.</param>
        /// <param name="value">Value.</param>
        public void SetAllowPrivClassSettings(string chatroom, bool value)
        {
            if (AllowPrivClassSettings.ContainsKey(chatroom))
                AllowPrivClassSettings[chatroom] = value;
            else
                AllowPrivClassSettings.Add(chatroom, value);
        }
        #endregion
    }
}
