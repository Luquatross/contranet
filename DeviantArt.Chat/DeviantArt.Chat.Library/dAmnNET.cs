using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using log4net;
using System.IO;

namespace DeviantArt.Chat.Library
{
    public class dAmnNET
    {
        private const int Version = 4;
        private Dictionary<string, string> ChatSettings = new Dictionary<string, string>
        {
            { "Host", "chat.deviantart.com" },
            { "Version", "0.3" },
            { "Port", "3900" }
        };
        private Dictionary<string, string> LoginSettings = new Dictionary<string, string>
        {
            { "Transport", "ss://" },
            { "Host", "www.deviantart.com" },
            { "File", "/users/login" },
            { "Port", "443" }
        };
        public const string Client = "damnNET";
        public const string Agent = "damnNET/3.5";
        public const string Owner = "bigmanhaywood";
        public string Trigger = "!";

        private bool IsConnecting = false;
        private TcpClient TcpClient;
        private ILog Logger = LogManager.GetLogger(typeof(dAmnNET));

        public dAmnNET()
        {

        }

        /// <summary>
        /// This method creates our dAmn connection!
        /// </summary>
        /// <returns>True if connection succeeded, otherwise false.</returns>
        public bool Connect()
        {
            bool result;

            // First thing we do is create a stream using the server config data.
            TcpClient = new TcpClient();
            try
            {
                // Connect!
                this.TcpClient.Connect(ChatSettings["Host"], ChatSettings["Port"]);

                // Get stream from socket
                Stream s = TcpClient.GetStream();
                StreamWriter sw = new StreamWriter(s);
                sw.AutoFlush = true;

                // Now we make our handshake packet. Here we send information about the bot/client 
                // to the dAmn server.
                StringBuilder data = new StringBuilder();
                data.AppendLine("dAmnClient " + ChatSettings["Version"]);
                data.AppendLine("agent=" + Agent);
                data.AppendLine("bot=" + Client);
                data.AppendLine("owner=" + Owner);
                data.AppendLine("trigger=" + Trigger);
                data.AppendLine("creator=bigmanhaywood/deviant@thehomeofjon.net");
                // This is were we actually send the packet! Quite simple really.
                sw.Write(data.ToString());
                // Now we have to raise a flag! This tells everything that we are currently trying to connect through a handshake!
                IsConnecting = true;
                result = true;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not open connection with " + ChatSettings["Host"], ex);
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Here's the actual send function which sends the packets.
        /// </summary>
        /// <param name="data">Data to send</param>
        public void Send(string data)
        {
            Stream s = TcpClient.GetStream();
            new StreamWriter(s).WriteLine(data);
        }

        /// <summary>
        /// his is the important one. It reads packets off of the stream and returns them in 
        /// an array! 
        /// </summary>
        public string[] Read()
        {
            try
            {
                Stream s = TcpClient.GetStream();
                StreamReader sr = new StreamReader(s);
                string data = sr.ReadToEnd();
                if (string.IsNullOrEmpty(data))
                    return new string[] { "disconnect\ne=socket closed\n\n" };
                else
                    return data.Split('\n');
            }
            catch (Exception ex)
            {
                Logger.Warn("Unable to read from connection.", ex);
                return new string[] { "disconnect\ne=socket closed\n\n" };
            }
        }

        /// <summary>
        /// Need to send a login packet? I'm your man!
        /// </summary>
        public void Login(string userName, string authToken)
        {
            Send(string.Format("login {0}\npk={1}\n\0", userName, authToken);
        }
    }
}
