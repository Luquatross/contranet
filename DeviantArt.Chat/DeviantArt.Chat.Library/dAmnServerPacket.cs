using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DeviantArt.Chat.Library
{
    /// <summary>
    /// The types of packets that can be received from the dAmn servers.
    /// </summary>
    public enum dAmnPacketType
    {
        Unknown,
        Handshake,
        Login,
        Join,
        Part,
        Action,        
        Chat,
        MemberJoin,
        MemberPart,
        MemberList,
        MemberKick,
        PrivChange,
        Topic,
        Ping,
        Kicked,
        Disconnect,
        Title,
        PrivClasses,
        ErrorSend,
        ErrorKick,
        ErrorSet,
        ErrorGet,
        ErrorKill,
        Whois,
        AdminCreate,
        AdminUpdate,
        AdminRename,
        AdminMove,
        AdminRemove,
        AdminShow,
        AdminError
    }

    /// <summary>
    /// Class the represents a packet sent FROM the server TO the us, the client.
    /// </summary>
    public class dAmnServerPacket : dAmnPacket
    {
        #region TabLumps
        /// <summary>
        /// Tablumps are a strings that come as part of a chat message. They are encoded html and deviantart
        /// specific html-ish entities that are separated and encoded with tabs. Strings from the HtmlMatch
        /// list will be replaced with strings from HtmlReplace list. Likewise, strings from the DevMatch list
        /// will be replaced with strings from the DevReplace list.
        /// </summary>
        private static Dictionary<string, string[]> TabLumps = new Dictionary<string, string[]>
        {
            // Regex expressiions to match html present in the tablimps
            { "HtmlMatch", new string[] {
                "&b\t",  "&/b\t",    "&i\t",    "&/i\t", "&u\t",   "&/u\t", "&s\t",   "&/s\t",    "&sup\t",    "&/sup\t", "&sub\t", "&/sub\t", "&code\t", "&/code\t",
			    "&br\t", "&ul\t",    "&/ul\t",  "&ol\t", "&/ol\t", "&li\t", "&/li\t", "&bcode\t", "&/bcode\t",
			    "&/a\t", "&/acro\t", "&/abbr\t", "&p\t", "&/p\t", "&lt;", "&gt;" }
            },

            // Html strings that will replace the correspnding regex matches
            { "HtmlReplace", new string[] {
                "<b>",  "</b>",       "<i>",     "</i>", "<u>",   "</u>", "<s>",   "</s>",    "<sup>",    "</sup>", "<sub>", "</sub>", "<code>", "</code>",
			    "\n",   "<ul>",       "</ul>",   "<ol>", "</ol>", "<li>", "</li>", "<bcode>", "</bcode>",
			    "</a>", "</acronym>", "</abbr>", "<p>",  "</p>\n", "<", ">" }
            },

            { "DevMatch", new string[] {
                "&emote\t([^\t]+)\t([^\t]+)\t([^\t]+)\t([^\t]+)\t([^\t]+)\t",
			    "&a\t([^\t]+)\t([^\t]*)\t",
			    "&link\t([^\t]+)\t&\t",
			    "&link\t([^\t]+)\t([^\t]+)\t&\t",
			    "&dev\t[^\t]\t([^\t]+)\t",
			    "&avatar\t([^\t]+)\t[0-9]\t",
			    "&thumb\t([0-9]+)\t([^\t]+)\t([^\t]+)\t([^\t]+)\t([^\t]+)\t([^\t]+)\t([^\t]+)\t",
			    "&img\t([^\t]+)\t([^\t]*)\t([^\t]*)\t",
			    "&iframe\t([^\t]+)\t([0-9%]*)\t([0-9%]*)\t&\\/iframe\t",
			    "&acro\t([^\t]+)\t",
			    "&abbr\t([^\t]+)\t" }
            },

            { "DevReplace", new string[] {
                "$1",
			    "<a href=\"$1\" title=\"$2\">",
			    "$1",
			    "$1 ($2)",
			    ":dev$1:",
			    ":icon$1:",
			    ":thumb$1:",
			    "<img src=\"$1\" alt=\"$2\" title=\"\\3\" />",
			    "<iframe src=\"$1\" width=\"$2\" height=\"\\3\" />",
			    "<acronym title=\"$1\">",
			    "<abbr title=\"$1\">" }
            }
        };
        #endregion

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

        #region Public Methods
        /// <summary>
        /// Parses the subpacket for this packet.
        /// </summary>
        /// <returns>Parsed packet.</returns>
        public dAmnPacket GetSubPacket()
        {
            if (string.IsNullOrEmpty(this.body))
                return null;
            else
                return Parse(this.body);
        }
        #endregion

        #region Static Methods
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
            string[] tmp = pktId.Split(new char[]{ ':' }, 2);
            if (tmp[0].ToLower() == "recv chat")
            {
                dAmnPacket subPacket = dAmnServerPacket.Parse(packet.body);
                if (subPacket.cmd == "msg")
                {
                    return dAmnPacketType.Chat;
                }
                else if (subPacket.cmd == "action")
                {
                    return dAmnPacketType.Action;
                }
                else if (subPacket.cmd == "join")
                {
                    return dAmnPacketType.MemberJoin;
                }
                else if (subPacket.cmd == "part")
                {
                    return dAmnPacketType.MemberPart;
                }
                else if (subPacket.cmd == "privchg")
                {
                    return dAmnPacketType.PrivChange;
                }
                else if (subPacket.cmd == "kicked")
                {
                    return dAmnPacketType.MemberKick;
                }
                else if (subPacket.cmd == "admin")
                {
                    if (subPacket.param == "create")
                        return dAmnPacketType.AdminCreate;
                    else if (subPacket.param == "update")
                        return dAmnPacketType.AdminUpdate;
                    else if (subPacket.param == "rename")
                        return dAmnPacketType.AdminRename;
                    else if (subPacket.param == "move")
                        return dAmnPacketType.AdminMove;
                    else if (subPacket.param == "remove")
                        return dAmnPacketType.AdminRemove;
                    else if (subPacket.param == "show")
                        return dAmnPacketType.AdminShow;
                    else if (subPacket.param == "privclass")
                        return dAmnPacketType.AdminError;
                    else
                        return dAmnPacketType.Unknown;
                }
                else
                {
                    return dAmnPacketType.Unknown;
                }
            }
            else if (tmp[0].ToLower() == "recv pchat")
            {
                return dAmnPacketType.Chat;
            }
            else if (tmp[0].ToLower() == "disconnect")
            {
                return dAmnPacketType.Disconnect;
            }
            else if (tmp[0].ToLower() == "join chat")
            {
                return dAmnPacketType.Join;
            }
            else if (tmp[0].ToLower() == "part chat")
            {
                return dAmnPacketType.Part;
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
            else if (tmp[0].ToLower() == "get pchat")
            {
                return dAmnPacketType.ErrorGet;
            }
            else if (tmp[0].ToLower() == "set pchat")
            {
                return dAmnPacketType.ErrorSet;
            }
            else if (tmp[0].ToLower() == "kill login")
            {
                return dAmnPacketType.ErrorKill;
            }
            else if (tmp[0].ToLower() == "property login")
            {
                return dAmnPacketType.Whois;
            }
            else if (tmp[0] == "property chat")
            {
                tmp = lines[1].Split(new char[] { '=' }, 2);
                if (tmp[0] == "p" & tmp[1] == "topic")
                {
                    return dAmnPacketType.Topic;
                }
                else if (tmp[0] == "p" & tmp[1] == "title")
                {
                    return dAmnPacketType.Title;
                }
                else if (tmp[0] == "p" & tmp[1] == "privclasses")
                {
                    return dAmnPacketType.PrivClasses;
                }
                else if (tmp[0] == "p" & tmp[1] == "members")
                {
                    return dAmnPacketType.MemberList;
                }
                else
                {
                    return dAmnPacketType.Unknown;
                }
            }
            else if (tmp[0] == "property pchat")
            {
                tmp = lines[1].Split(new char[] { '=' }, 2);
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

        /// <summary>
        /// Gets data about the packet that is useful to the plugin system.
        /// </summary>
        /// <param name="packet">Packet to convert.</param>
        /// <param name="from">User who generated this event.</param>
        /// <param name="message">Content of packet.</param>
        /// <param name="target">Target of the packet command.</param>
        /// <param name="eventResponse">Response to event.</param>
        public static void SortdAmnPacket(dAmnServerPacket packet, out string from, out string message, out string target, out string eventResponse)
        {
            // initialize variables
            from = null;
            message = null;
            target = null;
            eventResponse = null;

            switch (packet.PacketType)
            {
                case dAmnPacketType.Handshake:
                    // nothing to do
                    break;
                case dAmnPacketType.Login:
                    eventResponse = packet.args["e"];
                    break;
                case dAmnPacketType.Join:
                case dAmnPacketType.Part:
                    eventResponse = packet.args["e"];
                    if (packet.args.ContainsKey("r"))
                        message = packet.args["r"];
                    break;
                case dAmnPacketType.Topic:
                case dAmnPacketType.Title:
                case dAmnPacketType.PrivClasses:
                case dAmnPacketType.MemberList:
                    eventResponse = packet.args["p"];
                    from = packet.args["by"];
                    break;                               
                case dAmnPacketType.Chat:
                case dAmnPacketType.MemberJoin:
                case dAmnPacketType.MemberPart:
                case dAmnPacketType.MemberKick:
                case dAmnPacketType.PrivChange:
                case dAmnPacketType.AdminCreate:
                case dAmnPacketType.AdminError:
                case dAmnPacketType.AdminMove:
                case dAmnPacketType.AdminRemove:
                case dAmnPacketType.AdminRename:
                case dAmnPacketType.AdminShow:
                case dAmnPacketType.AdminUpdate:
                    // get sub packet
                    dAmnPacket subPacket = dAmnPacket.Parse(packet.body);
                    switch (subPacket.cmd)
                    {
                        case "msg":
                        case "action":
                            from = subPacket.args["from"];
                            message = subPacket.body;
                            break;
                        case "join":
                        case "part":
                            from = subPacket.param;
                            if (subPacket.args.ContainsKey("r"))
                                message = subPacket.args["r"];
                            break;
                        case "privchg":
                        case "kicked":
                            from = subPacket.param;
                            target = subPacket.args["by"];
                            if (subPacket.cmd == "privchg")
                                message = subPacket.args["pc"];
                            if (!string.IsNullOrEmpty(subPacket.cmd))
                                message = subPacket.body;
                            break;
                        case "admin":
                            // TODO!
                            break;
                    }
                    break;
                case dAmnPacketType.Kicked:
                    from = packet.args["by"];
                    if (!string.IsNullOrEmpty(packet.body))
                        message = packet.body;
                    break;
                case dAmnPacketType.Ping:
                    // nothing to do
                    break;
                case dAmnPacketType.Disconnect:
                    eventResponse = packet.args["e"];
                    break;
                case dAmnPacketType.ErrorSend:
                case dAmnPacketType.ErrorKick:
                case dAmnPacketType.ErrorGet:
                case dAmnPacketType.ErrorSet:
                    // TODO!
                    break;
                case dAmnPacketType.ErrorKill:
                    eventResponse = packet.args["e"];
                    break;
            }
        }

        /// <summary>
        /// Removes/replaces tablumps in the dAmn packet.
        /// </summary>
        /// <param name="data">Data to process.</param>
        /// <returns>String with all tablumps handled.</returns>
        public static string ProcessTabLumps(string data)
        {
            // first do a simple string replacement
            for (int i = 0; i < TabLumps["HtmlMatch"].Length; i++)
                data = data.Replace(TabLumps["HtmlMatch"][i], TabLumps["HtmlReplace"][i]);

            // do regex replacement
            for (int i = 0; i < TabLumps["DevMatch"].Length; i++)
                data = Regex.Replace(data, TabLumps["DevMatch"][i], TabLumps["DevReplace"][i]);

            // now fix stray html tags
            data = Regex.Replace(data, "/<([^>]+) (width|height|title|alt)=\"\"([^>]*?)>/", "<$1\\3>");

            return data;
        }
        #endregion
    }
}
