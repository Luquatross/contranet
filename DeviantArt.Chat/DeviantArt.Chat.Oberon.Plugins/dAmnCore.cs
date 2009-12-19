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

        public override void Load()
        {
            RegisterEvent(dAmnPacketType.Ping, new BotServerPacketEvent(Ping));
        }

        private void Ping(string chatroom, dAmnServerPacket packet)
        {
            dAmn.Send(new dAmnPacket { cmd = "pong" });
        }
    }
}
