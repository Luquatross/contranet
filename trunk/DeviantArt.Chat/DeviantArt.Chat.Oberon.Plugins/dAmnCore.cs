using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// PLugin that handles the core system events that come from the dAmn server.
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
            RegisterEvent(dAmnPacketType.Join, new BotServerPacketEvent(Join));
            RegisterEvent(dAmnPacketType.Part, new BotServerPacketEvent(Part));
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

        private void Join(string chatroom, dAmnServerPacket packet)
        {
            if (packet.args["e"] != "ok")
                return;
            Bot.RegisterChatroom(chatroom, new ChatRoom());                
        }

        private void Part(string chatroom, dAmnServerPacket packet)
        {
            if (packet.args["e"] != "ok")
                return;
            Bot.UnregisterChatroom(chatroom);

            if (Bot.ChatroomsOpen() == 0)
            {
                Bot.Console.Warning("No longer joined to any rooms! Exiting...");
                Bot.Shutdown();
            }
        }

        private void Property(string chatroom, dAmnServerPacket packet)
        {
            ChatRoom room = Bot.GetChatroom(chatroom);
            string property = packet.args["p"];

            switch (property)
            {
                case "title":
                    room.Title = packet.body;
                    break;
                case "topic":
                    room.Topic = packet.body;
                    break;
                case "privclasses":
                    // TODO - get priv classes!
                    break;
                case "members":
                    // TODO - get users!
                    break;
            }
        }
    }
}
