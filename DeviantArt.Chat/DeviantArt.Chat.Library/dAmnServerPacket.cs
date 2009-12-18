using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Library
{
    /// <summary>
    /// The types of packets that can be received from the dAmn servers.
    /// </summary>
    public enum dAmnPacketType
    {
        Unknown,
        Chat,
        MemberList,
        Topic,
        Ping,
        Kicked,
        Disconnect,
        Title,
        PrivClasses,
        ErrorSend,
        ErrorKick        
    }

    /// <summary>
    /// Class the represents a packet sent FROM the server TO the us, the client.
    /// </summary>
    public class dAmnServerPacket : dAmnPacket
    {
        #region Public Properties
        /// <summary>
        /// Type of packet that this is.
        /// </summary>
        public dAmnPacketType PacketType
        {
            get
            {
                if (_type == dAmnPacketType.Unknown)
                    _type = ParsedAmnPacketType(this);
                return _type;
            }
        }
        private dAmnPacketType _type = dAmnPacketType.Unknown;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public dAmnServerPacket() { }

        /// <summary>
        /// Constructor. Load this packet with info from another packet.
        /// </summary>
        /// <param name="packet">Packet to copy data from.</param>
        public dAmnServerPacket(dAmnPacket packet)
        {
            this.cmd = packet.cmd;
            this.param = packet.param;
            this.args = packet.args;
            this.body = packet.body;
        }
        #endregion

        /// <summary>
        /// Parses packet header to determine what the type is.
        /// </summary>
        /// <param name="packet">Packet to parse.</param>
        /// <returns>Packet type.</returns>
        public static dAmnPacketType ParsedAmnPacketType(dAmnPacket packet)
        {       
            // get packet header
            string[] tokens = packet.raw.Split(new string[] { "\n\n" }, StringSplitOptions.None);
            string header = tokens[0];
            
            // get each line
            string[] lines = header.Split('\n');
            string pktId = lines[0];
            System.Diagnostics.Debug.Print("Header has " + lines.Length + " line(s).");
            string[] tmp = pktId.Split(new char[]{ ':' }, 2);
            if (tmp[0].ToLower() == "recv chat")
            {
                return dAmnPacketType.Chat;
            }
            else if (tmp[0].ToLower() == "recv pchat")
            {
                return dAmnPacketType.Chat;
            }
            else if (tmp[0].ToLower() == "disconnect")
            {
                return dAmnPacketType.Disconnect;
            }
            else if (tmp[0].ToLower() == "kicked chat")
            {
                return dAmnPacketType.Kicked;
            }
            else if (tmp[0].ToLower() == "kicked pchat")
            {
                return dAmnPacketType.Kicked;
            }
            else if (tmp[0].ToLower() == "send chat")
            {
                return dAmnPacketType.ErrorSend;
            }
            else if (tmp[0].ToLower() == "send pchat")
            {
                return dAmnPacketType.ErrorSend;
            }
            else if (tmp[0].ToLower() == "kick chat")
            {                
                return dAmnPacketType.ErrorKick;
            }
            else if (tmp[0].ToLower() == "kick pchat")
            {                
                return dAmnPacketType.ErrorKick;
            }
            else if (tmp[0] == "property chat")
            {                
                tmp = lines[1].Split(new char[]{ '=' }, 2);
                if (tmp[0] == "p" & tmp[1] == "members")
                {
                    return dAmnPacketType.MemberList;
                }
                else if (tmp[0] == "p" & tmp[1] == "title")
                {
                    return dAmnPacketType.Title;
                }
                else if (tmp[0] == "p" & tmp[1] == "topic")
                {
                    return dAmnPacketType.Topic;
                }
                else if (tmp[0] == "p" & tmp[1] == "privclasses")
                {
                    return dAmnPacketType.PrivClasses;
                }
                else
                {
                    return dAmnPacketType.Unknown;
                }
            }
            else if (tmp[0] == "property pchat")
            {
                tmp = lines[1].Split(new char[]{ '=' }, 2);
                if (tmp[0] == "p" & tmp[1] == "members")
                {
                    return dAmnPacketType.MemberList;
                }
                else if (tmp[0] == "p" & tmp[1] == "title")
                {
                    return dAmnPacketType.Title;
                }
                else if (tmp[0] == "p" & tmp[1] == "topic")
                {
                    return dAmnPacketType.Topic;
                }
                else if (tmp[0] == "p" & tmp[1] == "privclasses")
                {
                    return dAmnPacketType.PrivClasses;
                }
                else
                {
                    return dAmnPacketType.Unknown;
                }
            }
            else if (pktId == "ping")
            {
                return dAmnPacketType.Ping;
            }
            return dAmnPacketType.Unknown;
        }
    }
}
