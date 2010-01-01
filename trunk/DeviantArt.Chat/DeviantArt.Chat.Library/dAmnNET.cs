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
using System.Runtime.Remoting.Messaging;

namespace DeviantArt.Chat.Library
{
    /// <summary>
    /// Class that encapsulates the functionality needed to send and receive
    /// communications from the dAmn chat server.
    /// </summary>
    public class dAmnNET
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the Authorization cookie received from the dAmn server after
        /// logging in. Sets the Auth Token when set.
        /// </summary>
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
        

        /// <summary>
        /// Client name.
        /// </summary>
        public string Client
        {
            get { return _Client; }
            set { _Client = value; }
        }

        /// <summary>
        /// Agent name.
        /// </summary>
        public string Agent
        {
            get { return _Agent; }
            set { _Agent = value; }
        }

        /// <summary>
        /// Owner's name.
        /// </summary>
        public string Owner
        {
            get { return _Owner; }
            set { _Owner = value; }
        }

        /// <summary>
        /// Trigger for this client.
        /// </summary>
        public string Trigger
        {
            get { return _Trigger; }
            set { _Trigger = value; }
        }
        #endregion

        #region Private Variables
        /// <summary>
        /// Information about this bot
        /// </summary>
        private HttpCookie _AuthCookie;
        private string _dAmnVersion = "0.3";
        private string _Client = "damnNET";
        private string _Agent = "damnNET/3.5";
        private string _Owner = "bigmanhaywood";
        private string _Trigger = "!";   

        private ILog Logger = LogManager.GetLogger(typeof(dAmnNET));
        private TcpClient Socket;
        private StreamReader StreamReader;
        private StreamWriter StreamWriter;
        private string AuthToken;
        private byte[] readBuffer = new byte[1024];        
        #endregion

        #region Send / Receive Methods
        /// <summary>
        /// Method that sends the packets to the dAmn server.
        /// </summary>
        /// <param name="packet">Packet to send.</param>
        public void Send(dAmnPacket packet)
        {
            StreamWriter.Write(packet.ToString());
            StreamWriter.Flush();
        }

        /// <summary>
        /// Method that sends the packets to the dAmn server.
        /// </summary>
        /// <param name="data">Data to send.</param>
        public void Send(string data)
        {            
            StreamWriter.Write(data);
            StreamWriter.Flush();            
        }

        /// <summary>
        /// Method to read packets off of the stream and returns them.
        /// </summary>
        /// <returns>String received from stream. If no data returns empty string.</returns>
        public string Read()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                int tmp;
                while (true)
                {
                    tmp = StreamReader.Read();
                    if (tmp == 0 || tmp == -1)
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
        
        /// <summary>
        /// Method to read packets off of the stream and return them in an arary.
        /// </summary>
        /// <returns>String array received from stream.</returns>
        public string[] ReadLines()
        {
            return Read().Split('\n');
        }

        /// <summary>
        /// Method to read packets off of the stream and return it.
        /// </summary>
        /// <returns>dAmnPacket. Null if no data is present.</returns>
        public dAmnPacket ReadPacket()
        {
            string rawPacket = Read();
            if (string.IsNullOrEmpty(rawPacket))
            {
                return null;
            }
            else
            {
                dAmnServerPacket p = new dAmnServerPacket(dAmnPacket.Parse(rawPacket));
                p.raw = rawPacket;
                return p;
            }
        }
        #endregion

        #region Build Packet
        /// <summary>
        /// Builds a packet to send to the dAmn server.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <returns>Constructed packet.</returns>
        protected string BuildPacket(string command)
        {
            return BuildPacket(command, null, null, null);
        }

        /// <summary>
        /// Builds a packet to send to the dAmn server.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <param name="param">Parameter for the command.</param>
        /// <returns>Constructed packet.</returns>
        protected string BuildPacket(string command, string param)
        {
            return BuildPacket(command, param, null, null);
        }

        /// <summary>
        /// Builds a packet to send to the dAmn server.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <param name="param">Parameter for the command.</param>
        /// <param name="arg">Any additional args.</param>
        /// <returns>Constructed packet.</returns>
        protected string BuildPacket(string command, string param, string arg)
        {
            return BuildPacket(command, param, arg, null);
        }

        /// <summary>
        /// Builds a packet to send to the dAmn server.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <param name="param">Parameter for the command.</param>
        /// <param name="arg">Any additional args.</param>
        /// <param name="body">Body of rhe packet.</param>
        /// <returns>Constructed packet.</returns>
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
        #endregion              

        #region Public Methods
        /// <summary>
        /// Get an authentication cookie from the dAmn server over SSL.
        /// </summary>
        /// <param name="username">Username to use to login.</param>
        /// <param name="password">Password to use to login.</param>
        /// <returns>Cookie read from the SSL response.</returns>
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

                // Create an SSL stream so that we can send and receive securely
                SslStream s1 = new SslStream(tcpClient.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null
                );

                // Authenticate the server's certificate and send the request
                s1.AuthenticateAsClient("www.deviantart.com");
                s1.Write(Encoding.UTF8.GetBytes(request.ToString()));
                s1.Flush();

                // Now that we have sent our data, we need to read the response. 
                string response = ReadSslMessage(s1);
                s1.Close();
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
                // If we get here, then everything failed
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

        /// <summary>
        /// Connects to the dAmn server. If no cookie has been set, gets
        /// one using the provided username and password. Then performs a
        /// handshake with the server and logs in.
        /// </summary>
        /// <param name="username">Username to use to login.</param>
        /// <param name="password">Password to use to login.</param>
        /// <returns>True if connected and logged in successfully. Otherwise false.</returns>
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
            if (Login(username, AuthToken))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the dAmn server. Shuts down the socket and streams.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                Socket.Close();
            }
            catch { }
            try
            {
                StreamReader.Close();
            }
            catch { }
            try
            {
                StreamWriter.Close();
            }
            catch { }
        }
        #endregion

        #region dAmn Helper Methods
        /// <summary>
        /// Performs a handshake with the server so it knows about this client.
        /// </summary>
        protected void PerformHandshake()
        {
            // Now we make our handshake packet. Here we send information about the bot/client 
            // to the dAmn server.
            StringBuilder data = new StringBuilder();
            data.Append("dAmnClient " + _dAmnVersion + "\n");
            data.Append("agent=" + Agent + "\n");
            data.Append("bot=" + Client + "\n");
            data.Append("owner=" + Owner + "\n");
            data.Append("trigger=" + Trigger + "\n");
            data.Append("creator=bigmanhaywood/deviant@thehomeofjon.net\n\0");

            Send(data.ToString());
            string result = Read(); // toss out un-needed data
        }

        /// <summary>
        /// Logs in to the dAmn server. use
        /// </summary>
        /// <param name="username">Username to use to log in.</param>
        /// <param name="authToken">AuthToken to use to log in.</param>
        /// <returns>True if login was successful. Otherwise fals.</returns>
        protected bool Login(string username, string authToken)
        {
            Send(BuildPacket("login", username, "pk=" + authToken));
            System.Threading.Thread.Sleep(300);
            string loginResponse = Read();
            if (loginResponse.ToLower().Contains("e=ok"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Deformats a chat string.
        /// </summary>
        /// <returns>Chat string de-formatted from a packet.</returns>
        public string DeformatChat(string chat)
        {
            return DeformatChat(chat, null);
        }

        /// <summary>
        /// Deformats a chat string.
        /// </summary>
        /// <returns>Chat string de-formatted from a packet.</returns>
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

        /// <summary>
        /// Formats the chat string.
        /// </summary>        
        /// <returns>Chat string formatted to be used in a packet</returns>
        public string FormatChat(string chat)
        {
            return FormatChat(chat, null);
        }

        /// <summary>
        /// Formats the chat string.
        /// </summary>
        /// <returns>Chat string formatted to be used in a packet.</returns>
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
        #endregion

        #region dAmn Commands
        public void Join(string channel)
        {                        
            Send(new dAmnPacket { cmd = "join", param = FormatChat(channel) });
        }

        public void Part(string channel)
        {            
            Send(new dAmnPacket { cmd = "part", param = FormatChat(channel) });
        }

        public void Say(string[] nss, string message)
        {
            foreach (string ns in nss)
                Say(ns, message);            
        }

        public void Say(string ns, string message)
        {            
            // determine message type
            string type = "msg";
            if (message.StartsWith("/me"))
            {
                type = "action";
                message = message.Substring("/me ".Length); // strip /me from the string
            }
            else if (message.StartsWith("/npmsg"))
            {
                type = "npmsg";
                message = message.Substring("/npmsg ".Length); // strip /npmsg from the string
            }

            // replace illegal characters
            message = message.Replace("&lt;", "<");
            message = message.Replace("&gt;", ">");
            message = message.Trim();

            // get chatroom name
            ns = FormatChat(ns);

            // send packet
            dAmnPacket packet = new dAmnPacket
            {
                cmd = "send",
                param = ns,
                body = type + " main\n\n" + message
            };
            Send(packet);             
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
            Promote(ns, user, null);
        }

        public void Promote(string ns, string user, string privClass)
        {
            if (!string.IsNullOrEmpty(privClass))
                privClass = "\n\n" + privClass;   // pc is optional. if it's there add it

            Send(new dAmnPacket
            {
                cmd = "send",
                param = FormatChat(ns),
                body = "promote " + user + privClass
            });            
        }

        public void Demote(string ns, string user)
        {
            Demote(ns, user, null);
        }

        public void Demote(string ns, string user, string privClass)
        {
            if (!string.IsNullOrEmpty(privClass))
                privClass = "\n\n" + privClass;   // pc is optional. if it's there add it

            Send(new dAmnPacket
            {
                cmd = "send",
                param = FormatChat(ns),
                body = "demote " + user + privClass
            });   
        }

        public void Kick(string ns, string user)
        {
            Kick(ns, user, string.Empty);
        }

        public void Kick(string ns, string user, string r)
        {
            if (!string.IsNullOrEmpty(r))
                r = "\n\n" + r;

            Send(new dAmnPacket
            {
                cmd = "kick",
                param = FormatChat(ns),
                args = new Dictionary<string,string>{ { "u", user } },
                body = r
            });
        }

        public void Ban(string ns, string user)
        {            
            Send(new dAmnPacket { cmd = "send", param = FormatChat(ns), body = "ban " + user });
        }

        public void UnBan(string ns, string user)
        {
            Send(new dAmnPacket { cmd = "send", param = FormatChat(ns), body = "unban " + user });
        }

        public void Get(string ns, string property)
        {            
            Send(new dAmnPacket
            {
                cmd = "get",
                param = FormatChat(ns),
                args = new Dictionary<string, string> { { "p", property } }
            });
        }

        public void Set(string ns, string property, string value)
        {
            Send(new dAmnPacket
            {
                cmd = "get",
                param = FormatChat(ns),
                args = new Dictionary<string, string> { { "p", property } },
                body = value
            });
        }

        public void Admin(string ns, string command)
        {            
            Send(new dAmnPacket { cmd = "send", param = FormatChat(ns), body = "admin\n\n" + command });
        }

        public void Close()
        {
            Send(new dAmnPacket { cmd = "disconnect" });
        }

        public void Pong()
        {
            Send(new dAmnPacket { cmd = "pong" });
        }

        public void UserInfo(string user)
        {
            Send(new dAmnPacket
            {
                cmd = "get",
                param = "login:" + user,
                args = new Dictionary<string, string> { { "p", "info" } }
            });
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Determines if the string is a channel or not.
        /// </summary>
        /// <param name="ns">Channel to test.</param>
        /// <returns>True if a channel, otherwise false.</returns>
        public bool IsChannel(string ns)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encodes a string to be used in a URL.
        /// </summary>
        /// <param name="str">String to encode.</param>
        /// <returns>Encoded string.</returns>
        private string UrlEncode(string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        /// <summary>
        /// Decodes a string that was used in a URL.
        /// </summary>
        /// <param name="str">String to decode.</param>
        /// <returns>Decoded string.</returns>
        private string UrlDecode(string str)
        {
            return HttpUtility.UrlDecode(str);
        }

        /// <summary>
        /// Reads a message from an SSL Stream.
        /// </summary>
        /// <param name="sslStream">Stream to read from.</param>
        /// <returns>String response found in the stream.</returns>
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

        #region Static Methods
        /// <summary>
        /// Validates the certificate provided by the secure dAmn server.
        /// </summary>
        /// <returns>True if valid, otherwise false.</returns>
        private static bool ValidateServerCertificate(
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
        #endregion
    }
}
