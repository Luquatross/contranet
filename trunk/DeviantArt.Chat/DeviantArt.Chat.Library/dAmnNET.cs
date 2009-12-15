using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using log4net;
using System.IO;
using System.Web;
using System.Net.Security;
using System.Collections.Specialized;
using System.Security.Authentication;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Collections;

namespace DeviantArt.Chat.Library
{
    public class dAmnNET
    {
        public HttpCookie AuthCookie {
            get
            {
                return _AuthCookie;
            }
            set
            {
                _AuthCookie = value;
                AuthToken = _AuthCookie.Values["authtoken"];
            }
        }
        private HttpCookie _AuthCookie;

        private const int Version = 4;
        private Dictionary<string, string> ChatSettings = new Dictionary<string, string>
        {
            { "Host", "chat.deviantart.com" },
            { "Version", "0.3" },
            { "Port", "3900" }
        };
        private Dictionary<string, string> LoginSettings = new Dictionary<string, string>
        {
            { "Transport", "ssl://" },
            { "Host", "www.deviantart.com" },
            { "File", "/users/login" },
            { "Port", "443" }
        };
        public const string Client = "damnNET";
        public const string Agent = "damnNET/3.5";
        public const string Owner = "bigmanhaywood";
        public string Trigger = "!";

        private ILog Logger = LogManager.GetLogger(typeof(dAmnNET));
        private TcpClient Socket;
        private StreamReader StreamReader;
        private StreamWriter StreamWriter;
        private string AuthToken;        
        private Thread ChatThread;

        public dAmnNET()
        {

        }

        /// <summary>
        /// Here's the actual send function which sends the packets.
        /// </summary>
        /// <param name="data">Data to send</param>
        public void Send(string data)
        {            
            StreamWriter.Write(data);
            StreamWriter.Flush();            
        }

        /// <summary>
        /// This is the important one. It reads packets off of the stream and returns them in 
        /// an array! 
        /// </summary>
        public string Read()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                int tmp;
                while (true)
                {
                    tmp = StreamReader.Read();
                    if (tmp == 0)
                        break;
                    else
                        sb.Append((char)tmp);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Unable to read from connection.", ex);
                return "disconnect\ne=socket closed\n";
            }
            return sb.ToString();
        }

        public string[] ReadLines()
        {
            return Read().Split('\n');
        }

        protected string BuildPacket(string command)
        {
            return BuildPacket(command, null, null, null);
        }

        protected string BuildPacket(string command, string param)
        {
            return BuildPacket(command, param, null, null);
        }

        protected string BuildPacket(string command, string param, string arg)
        {
            return BuildPacket(command, param, arg, null);
        }

        protected string BuildPacket(string command, string param, string arg, string body)
        {
            StringBuilder rval = new StringBuilder();
            rval.Append(command);
            if (!string.IsNullOrEmpty(param))
                rval.Append(" " + param);
            rval.Append(Environment.NewLine);
            if (!string.IsNullOrEmpty(arg))
                rval.Append(arg + Environment.NewLine);
            if (!string.IsNullOrEmpty(body))
                rval.Append(Environment.NewLine + body);
            rval.Append('\0');

            return rval.ToString();
        }

        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            
            System.Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }        

        /// <summary>
        /// Retrieve a Cookie.
        /// </summary>
        public HttpCookie GetAuthCookie(string username, string password)
        {
            // We need to use a query on deviantArt. 
            StringBuilder post = new StringBuilder();
            post.AppendFormat("ref={0}", UrlEncode("http://www.deviantart.com/")); // First value is the referrer
            post.AppendFormat("&username={0}", UrlEncode(username));    // second we have the username
            post.AppendFormat("&password={0}", UrlEncode(password));    // then the password
            post.AppendFormat("&action=Login");
            post.Append("&reusetoken=1\0"); // And finally reusetoken... this means we don't get a new authtoken.

            // Socket and stream object
            TcpClient tcpClient = null;
            StreamReader sr = null;
            StreamWriter sw = null;

            try
            {
                // first, open an SSL connection with our host
                tcpClient = new TcpClient("www.deviantart.com", 443);
                //sr = new StreamReader(new SslStream(tcpClient.GetStream()));
                //sw = new StreamWriter(new SslStream(tcpClient.GetStream()));

                // create request
                StringBuilder request = new StringBuilder();
                request.AppendLine("POST /users/login HTTP/1.1");
                request.AppendLine("Host: www.deviantart.com"); // Now we determine the host URL. Don't ask why it's in this order.
                request.AppendLine("User-Agent: " + Agent); // The we need to determine what User-Agent is being used. This is a string contain information about the client.
                request.AppendLine("Accept: text/html"); // We only want to accept text and/or html in response to our query!
                request.AppendLine("Referer: http://www.deviantart.com/"); // set referrer
                request.AppendLine("Cookie: skipintro=1"); // This cookie tells our host to skip the intro...
                request.AppendLine("Content-Type: application/x-www-form-urlencoded"); // Finally we show the content type of the our data.
                request.Append("Content-Length: " + post.Length + "\n\n" + post.ToString()); // Then the length of our query, and the query itself.

                // test
                SslStream s1 = new SslStream(tcpClient.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null
                );

                s1.AuthenticateAsClient("www.deviantart.com");
                s1.Write(Encoding.UTF8.GetBytes(request.ToString()));
                s1.Flush();

                string response = ReadSslMessage(s1);
                s1.Close();

                // And now we send our header and post data. First we declare the method as POST, select our file and protocol.                
                //sw.Write(request.ToString());
                //sw.Flush();

                // Now that we have sent our data, we need to read the response. 
                //string response = sr.ReadToEnd();
                //sr.Close();
                //sw.Close();
                tcpClient.Close();

                // And now we do the normal stuff, like checking if the response was empty or not.
                if (!string.IsNullOrEmpty(response))
                {
                    // Decode the returned data!
                    response = UrlDecode(response);
                    // We need to find the cookie! So get rid of everything before it!
                    response = (from s in response.Split('\n')
                                where s.Contains("Set-Cookie") && s.Contains("authtoken")
                                select s).Single();
                    response = response.Remove(response.IndexOf(";};") + 2).Substring(response.IndexOf('=') + 1);
                    
                    // convert serialized data into a usable hashtable
                    Hashtable values = (Hashtable)StringHelper.Deserialize(response);

                    // create cookie and add values
                    HttpCookie cookie = new HttpCookie("Userinfo");
                    foreach (DictionaryEntry entry in values)
                        cookie.Values.Add(entry.Key.ToString(), entry.Value.ToString());

                    return cookie;
                }
                // If we get here, then everything failed, so yeah...return Null
                return null;
            }
            catch (Exception ex)
            {
                Logger.Warn("Error reading cookie from SSL stream.", ex);
                return null;
            }
            finally
            {
                if (sr != null) sr.Close();
                if (sw != null) sw.Close();
                if (tcpClient != null) tcpClient.Close();
            }
        }

        public bool Connect(string username, string password)
        {
            // get auth token if we don't have one
            if (AuthCookie == null)
            {
                AuthCookie = GetAuthCookie(username, password);
            }

            // connect to the server
            Socket = new TcpClient("chat.deviantart.com", 3900);

            // create writers
            StreamWriter = new StreamWriter(Socket.GetStream());
            StreamReader = new StreamReader(Socket.GetStream());

            // do handshake
            PerformHandshake();

            // do a login
            if (Login(username))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void PerformHandshake()
        {
            // Now we make our handshake packet. Here we send information about the bot/client 
            // to the dAmn server.
            StringBuilder data = new StringBuilder();
            data.Append("dAmnClient " + ChatSettings["Version"] + "\n");
            data.Append("agent=" + Agent + "\n");
            data.Append("bot=" + Client + "\n");
            data.Append("owner=" + Owner + "\n");
            data.Append("trigger=" + Trigger + "\n");
            data.Append("creator=bigmanhaywood/deviant@thehomeofjon.net\n\0");

            Send(data.ToString());
            string result = Read(); // toss out un-needed data
        }

        protected bool Login(string username)
        {
            Send(BuildPacket("login", username, "pk=" + AuthToken));
            System.Threading.Thread.Sleep(300);
            string loginResponse = Read();
            if (loginResponse.ToLower().Contains("e=ok"))
                return true;
            else
                return false;
        }

        public string DeformatChat(string chat)
        {
            return DeformatChat(chat, null);
        }

        public string DeformatChat(string chat, string discard)
        {
            if (chat.Substring(0, 5) == "chat:")            
                return '#' + chat.Replace("chat:", "");            

            if (chat.Substring(0, 6) == "pchat:")
            {
                if (string.IsNullOrEmpty(discard))
                    return chat;
                chat = chat.Replace("pchat:", "");
                string chat1 = chat.Substring(0, chat.IndexOf(":"));
                string chat2 = chat.Substring(chat.IndexOf(":") + 1);
                if (chat1.ToLower() == discard.ToLower())
                    return "@" + chat1;
                else
                    return "@" + chat2;
            }

            return (chat.StartsWith("#")) ? chat : (chat.StartsWith("@") ? chat : "#" + chat);
        }

        public string FormatChat(string chat)
        {
            return FormatChat(chat, null);
        }

        public string FormatChat(string chat, string chat2)
        {
            chat = chat.Replace("#", "");
            if (!string.IsNullOrEmpty(chat2))
            {
                chat = chat.Replace("@", "");
                chat2 = chat2.Replace("@", "");
                if (chat.ToLower() == chat2.ToLower())
                {
                    string channel = "pchat:";
                    string[] users = new string[] { chat, chat2 };
                    Array.Sort(users);
                    return channel + users[0] + ":" + users[1];
                }
            }
            return (chat.StartsWith("chat:")) ? chat : (chat.StartsWith("pchat:") ? chat : "chat:" + chat);
        }

        #region dAmn Commands
        public void Join(string channel)
        {            
            Send(BuildPacket("join", channel));
        }

        public void Part(string channel)
        {
            Send("part " + channel + "\n");
        }

        public void Say(string[] nss, string message)
        {
            // WE CAN SEND MESSAGES TO A PLETHORA OF CHANNELS!
            foreach (string ns in nss)
                Say(ns, message);            
        }

        public void Say(string ns, string message)
        {
            // The type of message is easily changeable.
            string type = (message.StartsWith("/me ") ? "action" : (message.StartsWith("/npmsg ") ? "npmsg" : "msg"));
            message = (type == "action") ? message.Substring(4) : ((type == "npmsg") ? message.Substring(7) : message);
            message = message.Replace("&lt;", "<");
            message = message.Replace("&gt;", ">");
            message = message.Trim();
            Send("send " + ns + "\n\n" + type + " main\n\n" + message);
        }

        public void Action(string ns, string message)
        {
            Say(ns, "/me " + message);
        }

        public void NpMsg(string ns, string message)
        {
            Say(ns, "/npmsg " + message);
        }

        public void Promote(string ns, string user)
        {
            Promote(ns, user, string.Empty);
        }

        public void Promote(string ns, string user, string pc)
        {            
            Send("send " + ns + "\n\npromote " + user + "\n\n" + pc);
        }

        public void Demote(string ns, string user)
        {
            Demote(ns, user, string.Empty);
        }

        public void Demote(string ns, string user, string pc)
        {
            Send("send " + ns + "\n\ndemote " + user + "\n\n" + pc);
        }

        public void Kick(string ns, string user)
        {
            Kick(ns, user, string.Empty);
        }

        public void Kick(string ns, string user, string r)
        {
            if (!string.IsNullOrEmpty(r))
                r = "\n" + r + "\n";
            Send("kick " + ns + "\nu=" + user + "\n" + r);
        }

        public void Ban(string ns, string user)
        {
            Send("send " + ns + "\n\nban " + user + "\n");
        }

        public void UnBan(string ns, string user)
        {
            Send("send " + ns + "\n\nunban " + user + "\n");
        }

        public void Get(string ns, string property)
        {
            Send("get " + ns + "\np=" + property + "\n");
        }

        public void Set(string ns, string property, string value)
        {
            Send("set " + ns + "\np=" + property + "\n\n" + value + "\n");
        }

        public void Admin(string ns, string command)
        {
            Send("send " + ns + "\n\nadmin\n\n" + command);
        }

        public void Disconnect()
        {
            Send("disconnect\n");
        }
        #endregion

        #region Helper Methods
        public bool IsChannel(string ns)
        {
            throw new NotImplementedException();
        }

        private string UrlEncode(string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        private string UrlDecode(string str)
        {
            return HttpUtility.UrlDecode(str);
        }

        private string ReadSslMessage(SslStream sslStream)
        {
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                bytes = sslStream.Read(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF.
                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }

        #endregion
    }
}
