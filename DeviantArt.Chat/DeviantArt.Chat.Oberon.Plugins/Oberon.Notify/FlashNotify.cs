using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DeviantArt.Chat.Library;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Plugin that will flash the taskbar button when a user receives a message.
    /// </summary>
    public class FlashNotify : Plugin
    {
        #region Private Variables
        private string _PluginName = "Flash Notify";
        private string _FolderName = "Notify";
        private int _NumberOfTimesToFlash = 10;

        /// <summary>
        /// Determines if the flash is turned on or not.
        /// </summary>
        private bool IsFlashOn
        {
            get
            {
                if (!Settings.ContainsKey("IsFlashOn"))
                    Settings["IsFlashOn"] = false;
                return (bool)Settings["IsFlashOn"];
            }
            set
            {
                Settings["IsFlashOn"] = value;
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
            RegisterCommand("flashnotify", new BotCommandEvent(FlashNotifyManage), new CommandHelp(
                "Flashes the desktop taskbar when a message has been sent to the bot's username.",
                "flashnotify [on | off] - turns flash notify on or off"), (int)PrivClassDefaults.Owner);

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
            if (!IsFlashOn)
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

                // flash the window
                FlashWindow(_NumberOfTimesToFlash);
            }
        }

        private void FlashNotifyManage(string ns, string from, string message)
        {            
            if (string.IsNullOrEmpty(message))
            {
                ShowHelp(ns, from, "flashnotify");
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
                bool flashStatus = message == "on" ? true : false;
                IsFlashOn = flashStatus;
                Say(ns, string.Format("** FlashNotify is <b>{0}</b> *", message));
            }
        }
        #endregion

        #region Flash Window Interop Code
        /// <summary>
        /// Flashes the window the specified number of times.
        /// </summary>
        /// <remarks>
        /// http://stackoverflow.com/questions/73162/how-to-make-the-taskbar-blink-my-application-like-messenger-does-when-a-new-messa
        /// </remarks>
        /// <param name="numberOfTimes">Number of times to flash the window.</param>
        /// <returns>The return value specifies the window's state before the call to the FlashWindowEx function. 
        /// If the window caption was drawn as active before the call, the return value is true. Otherwise, 
        /// the return value is zero.</returns>
        private bool FlashWindow(int numberOfTimes)
        {
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = Bot.ConsoleWindow;
            fInfo.dwFlags = FLASHW_ALL;
            fInfo.uCount = Convert.ToUInt32(numberOfTimes);
            fInfo.dwTimeout = 0;

            return FlashWindowEx(ref fInfo);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        public const UInt32 FLASHW_ALL = 3;
        #endregion
    }    
}
