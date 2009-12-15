using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Library
{    
    /// <summary>
    /// Class that represents a dAmn text packet. Packet has the following format:
    /// 
    ///     packetname mainparam
    ///     some=variables
    ///     somemore=variables (Any characters can be used here)
    ///     
    ///     Main content is placed here.
    /// 
    /// If you used \n instead of line breaks the example packet would look like this:
    /// 
    ///     packetname mainparam\nsome=variables\nsomemore=variables (Any characters can be used here)\n\nMain content is placed here.
    ///     
    /// </summary>
    /// <seealso cref="http://botdom.com/wiki/DAmn#Basic_packet_format"/>
    /// <seealso cref="http://botdom.com/wiki/DAmn/SampleParser"/>
    public class dAmnPacket
    {
        #region Args and Body parts of packet
        private class dAmnArgs
        {
            private string _body = "";
            private Dictionary<string, string> _args = new Dictionary<string, string>();

            public Dictionary<string, string> args
            {
                get { return _args; }
                set { _args = value; }
            }
            public string body
            {
                get { return _body; }
                set { _body = value; }
            }
            public static dAmnArgs getArgsNData(string data)
            {
                dAmnArgs p = new dAmnArgs();
                p.args = new Dictionary<string, string>();
                p.body = null;
                while (true)
                {
                    if (data.Length == 0 || data[0] == '\n')
                        break;
                    int i = data.IndexOf('\n');
                    int j = data.IndexOf('=');
                    if (j > i)
                        break;
                    p.args[data.Substring(0, j)] = data.Substring(j + 1, i - (j + 1));
                    data = data.Substring(i + 1);
                }
                if (data != null && data.Length > 0)
                    p.body = data.Substring(1);
                else
                    p.body = "";
                return p;
            }
        }
        #endregion

        #region Private Instance Variables
        private string _cmd = "";
        private string _param = "";
        private dAmnArgs argsAndBody = new dAmnArgs();
        #endregion

        #region Public Properties
        public string cmd
        {
            get { return _cmd; }
            set { _cmd = value; }
        }

        public string param
        {
            get { return _param; }
            set { _param = value; }
        }

        public string body
        {
            get { return argsAndBody.body; }
            set { argsAndBody.body = value; }
        }

        public Dictionary<string, string> args
        {
            get { return argsAndBody.args; }
            set { argsAndBody.args = value; }
        }
        #endregion

        /// <summary>
        /// Convers a dAmnPacket into a string.
        /// </summary>
        /// <returns>String representing packet.</returns>
        public override string ToString()
        {
            StringBuilder rval = new StringBuilder();
            rval.Append(cmd);
            if (!string.IsNullOrEmpty(param))
                rval.Append(" " + param);
            rval.Append("\n");
            if (args != null)
            {
                foreach (KeyValuePair<string,string> entry in args)
                    rval.Append(entry.Key.ToString() + "=" + entry.Value.ToString() + Environment.NewLine);
            }                
            if (!string.IsNullOrEmpty(body))
                rval.Append("\n" + body);
            rval.Append('\0');

            return rval.ToString();
        }

        /// <summary>
        /// Converts a string into a useable dAmn packet.
        /// </summary>
        /// <param name="data">Data to parse.</param>
        /// <returns>dAmnPacket.</returns>
        public static dAmnPacket Parse(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            string orig_data = data;
            try
            {
                dAmnPacket p = new dAmnPacket();
                int i = data.IndexOf('\n');
                if (i < 0)
                    throw new Exception("Parser error, No line break.");
                string tmp = data.Substring(0, i);
                p.cmd = tmp.Split(' ')[0];
                int j = tmp.IndexOf(' ');
                if (j > 0)
                    p.param = tmp.Substring(j + 1);
                p.argsAndBody = dAmnArgs.getArgsNData(data.Substring(i + 1));
                return p;
            }
            catch (Exception e)
            {
                throw e;
            }            
        }
    }
}
