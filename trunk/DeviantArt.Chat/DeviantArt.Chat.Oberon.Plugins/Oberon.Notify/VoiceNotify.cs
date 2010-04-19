using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;
using System.Speech.Synthesis;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Plugin that will speak the notification.
    /// </summary>
    public class VoiceNotify : Plugin
    {
        #region Private Variables
        private string _PluginName = "Voice Notify";
        private string _FolderName = "Notify";
        private SpeechSynthesizer Synthesizer = new SpeechSynthesizer { Rate = 1, Volume = 100 };

        /// <summary>
        /// Determines if the notify voice is turned on or not.
        /// </summary>
        private bool IsVoiceOn
        {
            get
            {
                if (!Settings.ContainsKey("IsVoiceOn"))
                    Settings["IsVoiceOn"] = false;
                return (bool)Settings["IsVoiceOn"];
            }
            set
            {
                Settings["IsVoiceOn"] = value;
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

        #region Plugin Methods
        public override void Load()
        {
            // tie into the chat event so we can detect messages sent to a user
            RegisterEvent(dAmnPacketType.Chat, new BotServerPacketEvent(ChatReceived));

            // register commands
            RegisterCommand("voicenotify", new BotCommandEvent(VoiceNotifyManage), new CommandHelp(
                "Uses text-to-speech to speak a message that has been sent to the bot's username.",
                "voicenotify [on | off] - turns voice notify on or off"), (int)PrivClassDefaults.Owner);

            // load settings
            LoadSettings();
        }

        public override void Close()
        {
            SaveSettings();
        }
        #endregion

        #region Event & Command Handlers
        private void ChatReceived(string chatroom, dAmnServerPacket packet)
        {
            // if flash isn't on, don't bother processing
            if (!IsVoiceOn)
                return;

            // get relevant data about the packet
            dAmnCommandPacket commandPacket = new dAmnCommandPacket(packet);
            string from = commandPacket.From;
            string message = commandPacket.Message;
            string username = Bot.Username;

            // make sure we didn't send a message!
            if (from.ToLower() == username.ToLower())
                return;

            // see if message is to us or not
            if (Utility.IsMessageToUser(message, username, MsgUsernameParse.Lazy))
            {
                // send message to console
                Bot.Console.Notice(string.Format("[{0}] <{1}> {2}", chatroom, from, message));

                // speak text
                Speak(from, message);
            }
        }

        private void VoiceNotifyManage(string ns, string from, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                ShowHelp(ns, from, "voicenotify");
                return;
            }
            else
            {
                // get the message
                message = message.Trim().ToLower();
                if (message != "on" && message != "off")
                {
                    Respond(ns, from, "please specify either on or off.");
                    return;
                }

                // set whether the flash is on or not
                bool voiceStatus = message == "on" ? true : false;
                IsVoiceOn = voiceStatus;
                Say(ns, string.Format("** VoiceNotify is <b>{0}</b> *", message));
            }
        }
        #endregion

        #region Text-to-Speecch Code
        private void Speak(string from, string message)
        {
            // remove any html
            message = StringHelper.StripTags(message);

            // get string to send
            string stringToSpeak = string.Format("{0} says, {1}", from, message);
            
            Synthesizer.SpeakAsync(stringToSpeak);
        }
        #endregion
    }
}
