using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Plugin that handles the core system events that come from the dAmn server.
    /// Enables basic functionality for the Bot (chatroom handling, users joining / leaving,
    /// chatroom logging, etc).
    /// </summary>
    public class dAmnCore : Plugin
    {
        #region Private Variables
        private string _PluginName = "dAmn Core Plugin";
        private string _FolderName = "System";
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
        /// <summary>
        /// Here we will hook up our events to make the bot work properly.
        /// </summary>
        public override void Load()
        {
            RegisterEvent(dAmnPacketType.Ping, new BotServerPacketEvent(Ping));
            RegisterEvent(dAmnPacketType.Disconnect, new BotServerPacketEvent(Disconnect));
            RegisterEvent(dAmnPacketType.Chat, new BotServerPacketEvent(CheckMessage));
            RegisterEvent(dAmnPacketType.Chat, new BotServerPacketEvent(LogMessage));
            RegisterEvent(dAmnPacketType.Action, new BotServerPacketEvent(LogAction));
            RegisterEvent(dAmnPacketType.Join, new BotServerPacketEvent(Join));
            RegisterEvent(dAmnPacketType.Part, new BotServerPacketEvent(Part));
            RegisterEvent(dAmnPacketType.MemberJoin, new BotServerPacketEvent(ChatroomJoin));
            RegisterEvent(dAmnPacketType.MemberPart, new BotServerPacketEvent(ChatroomPart));
            RegisterEvent(dAmnPacketType.Title, new BotServerPacketEvent(ChatroomTitle));
            RegisterEvent(dAmnPacketType.Topic, new BotServerPacketEvent(ChatroomTopic));            
            RegisterEvent(dAmnPacketType.PrivClasses, new BotServerPacketEvent(ChatroomPrivClasses));            
            RegisterEvent(dAmnPacketType.MemberList, new BotServerPacketEvent(ChatroomMemberList));
            RegisterEvent(dAmnPacketType.PrivChange, new BotServerPacketEvent(ChatroomPrivChange));
            RegisterEvent(dAmnPacketType.AdminCreate, new BotServerPacketEvent(AdminCreate));
            RegisterEvent(dAmnPacketType.AdminUpdate, new BotServerPacketEvent(AdminUpdate));
            RegisterEvent(dAmnPacketType.AdminRename, new BotServerPacketEvent(AdminRename));
            RegisterEvent(dAmnPacketType.AdminMove, new BotServerPacketEvent(AdminMove));
            RegisterEvent(dAmnPacketType.AdminRemove, new BotServerPacketEvent(AdminRemove));            
            RegisterEvent(dAmnPacketType.AdminError, new BotServerPacketEvent(AdminError));
        }
        #endregion

        #region Packet Event Handlers
        /// <summary>
        /// Responds to a server ping so that the server knows we are still connected.
        /// </summary>        
        private void Ping(string chatroom, dAmnServerPacket packet)
        {
            dAmn.Send(new dAmnPacket { cmd = "pong" });
        }

        /// <summary>
        /// Closes the connection to the server and restarts the bot.
        /// </summary>        
        private void Disconnect(string chatroom, dAmnServerPacket packet)
        {
            Bot.Console.Warning("Experienced an unexpected disconnect!");
            Bot.Console.Warning("Waiting before before attempting to connect again...");
            Bot.Restart();
        }

        /// <summary>
        /// This method checks the chat message to see if is a command. If it is and
        /// the user has sufficient privileges the command will be triggered in the bot.
        /// </summary>
        /// <param name="chatroom">Chatroom command came from.</param>
        /// <param name="packet">Chat packet.</param>
        private void CheckMessage(string chatroom, dAmnServerPacket packet)
        {      
            string from;
            string message;
            string target;
            string eventResponse;

            // get information from the packet
            dAmnServerPacket.SortdAmnPacket(packet,
                out from,
                out message,
                out target,
                out eventResponse);

            // if we find the trigger we have detected a command
            string trigger = Bot.Trigger;
            if (!string.IsNullOrEmpty(message) && message.StartsWith(trigger))
            {
                string command = "";
                try
                {
                    // trim trigger and get command 
                    message = message.Substring(trigger.Length);
                    int firstSpace = message.IndexOf(' ');
                    // there is no space
                    if (firstSpace == -1)
                        firstSpace = message.Length;
                    command = message.Substring(0, firstSpace);

                    // check that we have a command
                    if (string.IsNullOrEmpty(command))
                        throw new ArgumentNullException("Unable to find command.");

                    // trim the command off of the message. may have a trailing space
                    if (message.IndexOf(command + " ") == 0)
                        message = message.Substring(message.IndexOf(command + " ") + command.Length + 1);
                    else
                        message = message.Substring(message.IndexOf(command) + command.Length);
                }
                catch (ArgumentNullException ex)
                {
                    // there wasn't a command - user doesn't need to know, just continue
                    return;
                }
                catch
                {
                    Bot.Console.Warning("Invalid command. Command: " + message);
                    dAmn.Say(chatroom, string.Format("{0}: invalid command.", from));
                    return;
                }

                // check user authorization
                bool isAuthorized = Bot.Access.UserHasAccess(from, command);
                if (!isAuthorized)
                {
                    dAmn.Say(chatroom, string.Format("{0}: access to the command '{1}' is denied.", from, command));
                    return;
                }

                // get the last string in the args to determine if help command
                string lastString = message.Trim().Split(' ').LastOrDefault();
                if (lastString == "?")
                {
                    Bot.TriggerHelp(command, chatroom, from);
                }
                else
                {
                    Bot.TriggerCommand(command, chatroom, from, message);
                }
            }
        }

        /// <summary>
        /// Logs a chat message to the chat log file.
        /// </summary>
        /// <param name="chatroom">Chatroom to log for.</param>
        /// <param name="packet">Packet to log.</param>
        private void LogMessage(string chatroom, dAmnServerPacket packet)
        {
            Chat room = Bot.GetChatroom(chatroom);
            if (room != null)
            {
                dAmnPacket subPacket = packet.GetSubPacket();
                string username = subPacket.args["from"];
                room.Log(string.Format("<{0}> {1}", username, subPacket.body));
            }
        }

        /// <summary>
        /// Log a chat action tot the chat log file.
        /// </summary>
        /// <param name="chatroom">Chatroom to log for.</param>
        /// <param name="packet">Packet to log.</param>
        public void LogAction(string chatroom, dAmnServerPacket packet)
        {
            Chat room = Bot.GetChatroom(chatroom);
            dAmnPacket subPacket = packet.GetSubPacket();
            string username = subPacket.args["from"];
            room.Log(string.Format("{0} {1} {2}",room[username].Symbol, username, subPacket.body));
        }

        /// <summary>
        /// Registers chatroom with Bot.
        /// </summary>
        private void Join(string chatroom, dAmnServerPacket packet)
        {
            if (packet.args["e"] != "ok")
                return;
            Bot.Console.Notice(string.Format("*** Bot has joined {0} *", chatroom));
            if (Bot.GetChatroom(chatroom) == null)
                Bot.RegisterChatroom(chatroom, new Chat(chatroom));                
        }

        /// <summary>
        /// Removes chatroom from bot.
        /// </summary>
        private void Part(string chatroom, dAmnServerPacket packet)
        {
            if (packet.args["e"] != "ok")
                return;
            Bot.Console.Notice(string.Format("{0} left the chatroom.", chatroom));
            Bot.UnregisterChatroom(chatroom);

            if (Bot.ChatroomsOpen() == 0)
            {
                Bot.Console.Warning("No longer joined to any rooms! Exiting...");
                Bot.Shutdown();
            }
        }

        /// <summary>
        /// Adds a user to the chat room.
        /// </summary>
        private void ChatroomJoin(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: user add for a chatroom which doesn't exist.");
                return;
            }

            dAmnPacket subPacket = packet.GetSubPacket();
            string username = subPacket.param;
            // user may exist already (bot can user can sign on at same time)
            if (chat[username] == null)
            {
                chat.RegisterUser(new User { Username = username });
            }
            else
            {
                chat[username].Count++;
            }
            chat.Log(username + " joined.");
        }

        /// <summary>
        /// Removes user from chat room.
        /// </summary>
        private void ChatroomPart(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: user part for a chatroom which doesn't exist.");
                return;
            }

            dAmnPacket subPacket = packet.GetSubPacket();
            string username = subPacket.param;
            string reason = "reason unknown";
            if (subPacket.args.ContainsKey("r"))
                reason = subPacket.args["r"];
            // if more than one user is signed in with same user name, only unregister
            // them if it is the last one
            if (chat[username].Count == 0)
            {
                chat.UnregisterUser(username);
            }
            else
            {
                chat[username].Count--;
            }
            chat.Log(string.Format("** {0} has left. [{1}]", username, reason));
        }

        /// <summary>
        /// Changes a chatroom title.
        /// </summary>
        private void ChatroomTitle(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: title change for a chatroom which doesn't exist.");
                return;
            }

            string title = packet.body;
            chat.Title = title;
            chat.Notice("Room title is: " + title);
        }

        /// <summary>
        /// Changes a chatroom topic.
        /// </summary>
        private void ChatroomTopic(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: topic change for a chatroom which doesn't exist.");
                return;
            }

            string topic = packet.body;
            chat.Topic = topic;
            chat.Notice("Room topic is: " + topic);
        }

        /// <summary>
        /// Updates the chatroom priv classes.
        /// </summary>
        private void ChatroomPrivClasses(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: member add to a chatroom which doesn't exist.");
                return;
            }

            // iterate through each priv class
            string[] privClasses = packet.body.TrimEnd('\n').Split('\n');
            foreach (string privClass in privClasses)
            {
                // get priv class details
                string[] tokens = privClass.Split(':');
                string privClassName = tokens[1];
                int privClassLevel = Convert.ToInt32(tokens[0]);

                // add to room
                if (chat.PrivClasses.ContainsKey(privClassName))
                    chat.PrivClasses[privClassName] = privClassLevel;
                else 
                    chat.PrivClasses.Add(privClassName, privClassLevel);
            }
        }

        /// <summary>
        /// Adds users to the chatroom.
        /// </summary>
        private void ChatroomMemberList(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: member list for a chatroom which doesn't exist.");
                return;
            }

            // iterate through each user
            string[] subPackets = packet.body.Split(new string[] { "\n\n" }, StringSplitOptions.None);
            foreach (string subPacket in subPackets)
            {
                if (subPacket == "")
                    continue;

                // get subpacket
                dAmnPacket p = dAmnPacket.Parse(subPacket);
                if (chat[p.param] == null)
                {
                    chat.RegisterUser(new User
                    {
                        Username = p.param,
                        Realname = p.args["realname"],
                        Description = p.args["typename"],
                        PrivClass = p.args["pc"],
                        //ServerPrivClass = p.args["gpc"],
                        Symbol = p.args["symbol"]
                    });
                }
                else
                {
                    User user = chat[p.param];
                    user.Username = p.param;
                    user.Realname = p.args["realname"];
                    user.Description = p.args["typename"];
                    user.PrivClass = p.args["pc"];
                    //user.ServerPrivClass = p.args["gpc"];
                    user.Symbol = p.args["symbol"];
                    user.Count++;
                }
            }
        }

        /// <summary>
        /// Changes privilege for a user in the chatroom.
        /// </summary>
        private void ChatroomPrivChange(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: Priv change for a chatroom which doesn't exist.");
                return;
            }

            // get info from packet
            dAmnPacket subPacket = packet.GetSubPacket();
            string username = subPacket.param;
            string privClass = subPacket.args["pc"];
            string by = subPacket.args["by"];

            // update priv class if user is signed on
            if (chat.ContainsUser(username))
            {            
                chat[username].PrivClass = privClass;
            }

            chat.Log(string.Format("** {0} has been made a member of {1} by {2} *", username, privClass, by));
        }

        /// <summary>
        /// Create a new priv class.
        /// </summary>
        private void AdminCreate(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: Priv add for a chatroom which doesn't exist.");
                return;
            }

            dAmnPacket subPacket = packet.GetSubPacket();
            string privClass = subPacket.args["name"];
            string by = subPacket.args["by"];
            chat.PrivClasses.Add(privClass, 1); // TODO - figure out what really happens here

            chat.Log(string.Format("Privclass '{0}' was created by {1}.", privClass, by));
        }

        /// <summary>
        /// Updates priv class.
        /// </summary>
        private void AdminUpdate(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: Priv add for a chatroom which doesn't exist.");
                return;
            }

            dAmnPacket subPacket = packet.GetSubPacket();
            string privClass = subPacket.args["name"];
            string by = subPacket.args["by"];
            string privs = subPacket.args["privs"];

            chat.Log(string.Format("** privilege class {0} has been updated  by {1} with {2}", privClass, by, privs));
        }

        /// <summary>
        /// Renames a priv class.
        /// </summary>
        private void AdminRename(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: Priv rename for a chatroom which doesn't exist.");
                return;
            }

            dAmnPacket subPacket = packet.GetSubPacket();
            string privClass = subPacket.args["prev"];
            string newPrivClass = subPacket.args["name"];
            string by = subPacket.args["by"];
            if (chat.PrivClasses.ContainsKey(privClass))
            {
                int privClassLevel = chat.PrivClasses[privClass];
                // remove the old and add the new
                chat.PrivClasses.Remove(privClass);
                chat.PrivClasses.Add(newPrivClass, privClassLevel);                

                // update users who have this priv class
                List<User> users = (from u in chat.GetAllMembers()
                                    where u.PrivClass == privClass
                                    select u).ToList();
                // update their priv class to the new one
                foreach (User u in users)
                    u.PrivClass = newPrivClass;

                chat.Log(string.Format("The privclass '{0}' was renamed to '{!}' by {2}.", privClass, newPrivClass, by));
            }
        }

        /// <summary>
        /// Moves users from one privclass to another
        /// </summary>
        private void AdminMove(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: Priv move for a chatroom which doesn't exist.");
                return;
            }

            dAmnPacket subPacket = packet.GetSubPacket();
            string privClass = subPacket.args["prev"];
            string newPrivClass = subPacket.args["name"];
            string by = subPacket.args["by"];

            // get all users who have this priv class
            List<User> users = (from u in chat.GetAllMembers()
                                where u.PrivClass == privClass
                                select u).ToList();

            // update their priv class to the new one
            foreach (User u in users)
                u.PrivClass = newPrivClass;

            chat.Log(string.Format("{0} users moved from privclass '{1}' to '{2}' by {3}.", users.Count, privClass, newPrivClass, by));
        }

        /// <summary>
        /// Removes a priv class.
        /// </summary>
        private void AdminRemove(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: Priv delete for a chatroom which doesn't exist.");
                return;
            }

            dAmnPacket subPacket = packet.GetSubPacket();
            string by = subPacket.args["by"];
            string privClass = subPacket.args["name"];
            chat.PrivClasses.Remove(privClass);

            // get all users who have this priv class
            List<User> users = (from u in chat.GetAllMembers()
                                where u.PrivClass == privClass
                                select u).ToList();

            // update their priv class
            foreach (User u in users)
                u.PrivClass = string.Empty;

            chat.Log(string.Format("The privclass '{0}' was removed by {1}.", privClass, by));
        }

        /// <summary>
        /// When an admin command generates an error.
        /// </summary>
        private void AdminError(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: admin show command for a chatroom which doesn't exist.");
                return;
            }

            dAmnPacket subPacket = packet.GetSubPacket();
            string command = subPacket.body;
            string error = subPacket.args["e"];
            chat.Log(string.Format("Admin error. The command '{0}' returned: {1}", command, error));
        }
        #endregion
    }
}
