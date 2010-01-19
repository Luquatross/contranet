using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSNPSharp;

namespace DeviantArt.Chat.Oberon.Plugins
{
    public class MsnNotify : ImNotifyBase
    {
        #region Private Variables
        private string _PluginName = "MSN Notify";

        /// <summary>
        /// MSN Messenger manager object.
        /// </summary>
        private Messenger Messenger;

        /// <summary>
        /// Local cache of conversations, so we don't have to create one each time. Key is dA username, 
        /// value is Conversation.
        /// </summary>
        private Dictionary<string, Conversation> Conversations = new Dictionary<string, Conversation>();
        #endregion

        #region Public Properties
        public override string PluginName
        {
            get { return _PluginName; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public MsnNotify()
        {
            // create messenger object
            Messenger = new Messenger();
            Messenger.Nameserver.BotMode = true;
            Messenger.Nameserver.AutoSynchronize = false;
            Messenger.Nameserver.SignedIn += new EventHandler<EventArgs>(SignedIn);
        }
        #endregion

        #region Messenger Methods
        /// <summary>
        /// Sets the credentials for the Messenger object.
        /// </summary>
        private void SetMsnCredentials()
        {
            if (!IsConfigured)
                return;

            // if we're connected already, sign out so we can use our new credentials
            if (Messenger.Connected)
                Messenger.Disconnect();

            // set new credentials            
            Messenger.Credentials = new Credentials(Username, Password, MsnProtocol.MSNP18);
        }

        /// <summary>
        /// Connects to MSN network.
        /// </summary>
        private void Connect()
        {
            if (IsConfigured && !Messenger.Connected)
            {
                SetMsnCredentials();
                Messenger.Connect();
            }
        }

        /// <summary>
        /// Disconnects from the MSN network.
        /// </summary>
        private void Disconnect()
        {
            if (Messenger.Connected)
                Messenger.Disconnect();
        }

        /// <summary>
        /// Method that is run when Messenger gets signed in.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SignedIn(object sender, EventArgs e)
        {
            // Set initial status
            Messenger.Owner.Status = PresenceStatus.Online;

            // Set display name and personal message
            Messenger.Owner.Name = "MyBot";
            Messenger.Owner.PersonalMessage = new PersonalMessage("I like MSNPSharp", MediaType.None, null, NSMessageHandler.MachineGuid);

            // Set display picture
            //DisplayImage displayImage = new DisplayImage();
            //displayImage.Image = Image.FromFile("avatar.png");
            //Messenger.Owner.DisplayImage = displayImage;
        }

        /// <summary>
        /// Get a cached conversation if there is one. If not, creates one.
        /// </summary>
        /// <param name="username">Username to get cache for.</param>
        /// <returns>Conversation.</returns>
        private Conversation GetConversation(string username)
        {
            if (Conversations.ContainsKey(username))
            {
                Conversation conv = Conversations[username];
                // if the conversation has ended somehow or no one is there, 
                // generate a new conversation.
                if (conv.Ended || conv.Expired || conv.Contacts.Count == 1)
                {
                    Conversations.Remove(username);
                    return GetConversation(username);
                }
                else
                {
                    return conv;
                }
            }
            else
            {
                Conversation conv = Messenger.CreateConversation();
                conv.AutoKeepAlive = true;
                conv.Invite(username, ClientType.None);
                Conversations.Add(username, conv);
                return Conversations[username];
            }
        }
        
        /// <summary>
        /// Send message to MSN contact.
        /// </summary>        
        protected override bool Send(string username, string chatroom, string from, string message)
        {
            // can't send message if we're not connected or signed in
            if (!Messenger.Connected || !Messenger.Nameserver.IsSignedIn)
                return false;

            // send message
            try
            {
                // format message to send to user
                message = string.Format("#{0} <{1}> {2}", chatroom, from, message);

                Conversation conv = GetConversation(username);                
                conv.SendTextMessage(new TextMessage(message));

                return true;
            }
            catch (Exception ex)
            {
                Bot.Console.Log("Error sending MSN notification. " + ex.ToString());
                return false;
            }
        }
        #endregion

        #region Plugin Methods
        public override void Load()
        {
            // call base load method
            base.Load();            
            
            // register commands
            RegisterCommand("msnnotify", new BotCommandEvent(MessengerNotify), new CommandHelp(
                "Allows a user to be notified via MSN when they are tabbed in a chatroom.",
                "msnnotify add [msn username] - sets that username to be notified when you are tabbed<br />" +
                "msnnotify off - turn off notifications for yourself<br />" +
                "msnnotify mystatus - shows whether your notifications are on or off"), 
                (int)PrivClassDefaults.Members);
            
            RegisterCommand("msnadmin", new BotCommandEvent(MessengerAdmin), new CommandHelp(
                "Manage msnnotify configuration.",
                "msnadmin set [username] [password] - configures username and password bot will use to sign in<br />" +
                "msnadmin connect - connect to msn network<br />" +
                "msnadmin disconnect - disconnects from msn network<br />" +
                "msnadmin status - show connection status for plugin"),
                (int)PrivClassDefaults.Operators);
        }

        public override void Activate()
        {
            Connect();
        }

        public override void Deactivate()
        {
            Disconnect();
        }
        #endregion

        #region Command Handlers
        private void MessengerNotify(string ns, string from, string message)
        {
            string[] args = GetArgs(message);

            if (args.Length == 2 && args[0] == "add")
            {
                if (IsUserInList(from))
                {
                    Respond(ns, from, "your msn notifications are already turned on.");
                    return;
                }
                else
                {
                    AddNotification(from, args[1]);
                    Respond(ns, from, "your msn notifications have now been turned on.");
                }
            }
            else if (args.Length == 1 && args[0] == "off")
            {
                if (IsUserInList(from))
                    RemoveNotification(from);
                Respond(ns, from, "msn notifications are turned off.");
            }
            else if (args.Length == 1 && args[0] == "mystatus")
            {
                if (IsUserInList(from))
                    Respond(ns, from, "your msn notifcations are turned on.");
                else
                    Respond(ns, from, "your msn notifcations are turned off.");
            }
            else
            {
                ShowHelp(ns, from, "msnnotify");
            }
        }

        private void MessengerAdmin(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string subCommand = GetArg(args, 0);

            switch (subCommand)
            {
                case "set":
                    string username = GetArg(args, 1);
                    string password = GetArg(args, 2);

                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        Respond(ns, from, "You must provide a username and password.");
                        return;
                    }

                    Username = username;
                    Password = password;
                    Respond(ns, from, "MSN Notify has been configured successfully.");
                    break;
                case "connect":
                    if (Messenger.Connected)
                    {
                        Respond(ns, from, "MSN Notify is already connected.");
                        return;
                    }
                    Connect();
                    Respond(ns, from, "Connection has started. Check the status in a second to see if it was successful.");
                    break;
                case "disconnect":
                    if (!Messenger.Connected)
                    {
                        Respond(ns, from, "MSN Notify is already disconnected.");
                        return;
                    }
                    Disconnect();
                    Respond(ns, from, "MSN Notify has disconnected.");
                    break;
                case "status":
                    if (!IsConfigured)
                        Say(ns, "** msnnotify has not yet been configured *");
                    else if (Messenger.Nameserver.IsSignedIn)
                        Say(ns, "** msnnotify is signed in and connected *");
                    else if (Messenger.Connected)
                        Say(ns, "** msnnotify is not signed in, but is connected *");
                    else
                        Say(ns, "** msnnotify is not signed in or connected. *");
                    break;
                default:
                    ShowHelp(ns, from, "msnadmin");
                    break;
            }
        }
        #endregion                    
    }
}
