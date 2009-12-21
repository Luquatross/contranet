using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Plugin that handles the core system events that come from the dAmn server.
    /// </summary>
    public class dAmnCore : Plugin
    {
        private string _PluginName = "dAmn Core Plugin";
        private string _FolderName = "System";

        public override string PluginName
        {
            get { return _PluginName; }
        }

        public override string FolderName
        {
            get { return _FolderName; }
        }

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
            RegisterEvent(dAmnPacketType.Title, new BotServerPacketEvent(ChatroomTitle));
            RegisterEvent(dAmnPacketType.Topic, new BotServerPacketEvent(ChatroomTopic));
            RegisterEvent(dAmnPacketType.PrivClasses, new BotServerPacketEvent(ChatroomPrivClasses));
            RegisterEvent(dAmnPacketType.MemberList, new BotServerPacketEvent(ChatroomMemberList));
        }

        private void Ping(string chatroom, dAmnServerPacket packet)
        {
            dAmn.Send(new dAmnPacket { cmd = "pong" });
        }

        private void Disconnect(string chatroom, dAmnServerPacket packet)
        {
            Bot.Console.Warning("Experienced an unexpected disconnect!");
            Bot.Console.Warning("Waiting before before attempting to connect again...");
            Bot.Restart();
        }

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
                // TODO : check user access!                

                // trim trigger and get command 
                message = message.Substring(trigger.Length);
                string command = message.Substring(0, message.IndexOf(' '));

                // send command!
                Bot.TriggerCommand(command, chatroom, from, message);
            }
        }

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

        public void LogAction(string chatroom, dAmnServerPacket packet)
        {
            Chat room = Bot.GetChatroom(chatroom);
            dAmnPacket subPacket = packet.GetSubPacket();
            string username = subPacket.args["from"];
            room.Log(string.Format("{0} {1} {2}",room[username].Symbol, username, subPacket.body));
        }

        private void Join(string chatroom, dAmnServerPacket packet)
        {
            if (packet.args["e"] != "ok")
                return;
            Bot.Console.Notice(string.Format("*** Bot has joined {0} *", chatroom));
            Bot.RegisterChatroom(chatroom, new Chat(chatroom));                
        }

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

        private void ChatroomJoin(string chatroom, dAmnServerPacket packet)
        {
            Chat chat = Bot.GetChatroom(chatroom);
            if (chat == null)
            {
                Bot.Console.Log("Error: user add for a chatroom which doesn't exist.");
                return;
            }

            string username = packet.body.Split(' ').ElementAt(1);
            chat.RegisterUser(new User { Username = username });
            chat.Log(username + " joined.");
        }

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
                chat.PrivClasses.Add(privClassName, privClassLevel);
            }
        }

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

            // update priv class
            chat[username].PrivClass = privClass;
        }
    }
}
