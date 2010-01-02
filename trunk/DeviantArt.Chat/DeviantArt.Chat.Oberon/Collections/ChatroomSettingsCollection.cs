using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Oberon.Collections
{
    [Serializable()]
    public class ChatroomSettingsCollection<T>
    {
        /// <summary>
        /// Collection to hold all of our settings for chatrooms.
        /// </summary>
        private Dictionary<string, T> settings = new Dictionary<string, T>();

        private bool GlobalIsSet = false;
        private T _GlobalSetting;

        public T GlobalSetting
        {
            get
            {
                return _GlobalSetting;
            }
            set
            {
                _GlobalSetting = value;
                GlobalIsSet = true;
            }
        }

        public T DefaultSetting
        {
            get;
            set;
        }

        public ChatroomSettingsCollection(T defaultSetting)
        {
            DefaultSetting = defaultSetting;
        }

        public bool ContainsChatroom(string chatroom)
        {
            return settings.ContainsKey(chatroom);
        }

        public T Get(string chatroom)
        {
            if (settings.ContainsKey(chatroom))
                return settings[chatroom];
            else if (GlobalIsSet)
                return GlobalSetting;
            else
                return DefaultSetting;
        }

        public void Set(string chatroom, T value)
        {
            if (settings.ContainsKey(chatroom))
                settings[chatroom] = value;
            else
                settings.Add(chatroom, value);
        }

        public void Remove(string chatroom)
        {
            settings.Remove(chatroom);
        }

        public T this[string chatroom]
        {
            get
            {
                return Get(chatroom);
            }
            set
            {
                Set(chatroom, value);
            }
        }

        public void DeleteGlobalSetting()
        {
            DeleteGlobalSetting(true);
        }

        public void DeleteGlobalSetting(bool clearAll)
        {
            GlobalIsSet = false;
            if (clearAll)
                settings.Clear();
        }
    }
}
