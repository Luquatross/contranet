#region Copyright (c) 2002-2010, Bas Geertsema, Xih Solutions (http://www.xihsolutions.net), Thiago.Sayao, Pang Wu, Ethem Evlice
/*
Copyright (c) 2002-2010, Bas Geertsema, Xih Solutions
(http://www.xihsolutions.net), Thiago.Sayao, Pang Wu, Ethem Evlice.
All rights reserved. http://code.google.com/p/msnp-sharp/

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice,
  this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.
* Neither the names of Bas Geertsema or Xih Solutions nor the names of its
  contributors may be used to endorse or promote products derived from this
  software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 'AS IS'
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace MSNPSharp.Core
{
    /// <summary>
    /// NS payload message class, such as ADL and FQY
    /// <para>The format of these mseeages is: COMMAND TRANSID [PARAM1] [PARAM2] .. PAYLOADLENGTH\r\nPAYLOAD</para>
    /// <remarks>
    /// DONOT pass the payload length as command value, the payload length will be calculated automatically
    /// <para>
    /// <list type="bullet">
    /// List of NS payload commands:
    /// <item>
    /// RML
    /// <description>Remove contact </description>
    /// </item>
    /// <item>
    /// ADL
    /// <description>Add users to your contact lists.</description>
    /// </item>
    /// <item>
    /// FQY
    /// <description>Query client's network types except PassportMember</description>
    /// </item>
    /// <item>
    /// QRY
    /// <description>Response to CHL by client </description>
    /// </item>
    /// <item>NOT</item>
    /// <item>UBX</item>
    /// <item>GCF</item>
    /// <item>
    /// UBM
    /// <description>Yahoo messenger message command. You can also send the command as <see cref="YIMMessage"/></description>
    /// </item>
    /// <item>IPG</item>
    /// <item>UUX</item>
    /// <item>MSG</item>
    /// <item>UBN</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// </summary>
    [Serializable()]
    public class NSPayLoadMessage : NSMessage
    {
        private string payLoad = string.Empty;

        public string PayLoad
        {
            get
            {
                return payLoad;
            }
            set
            {
                payLoad = value;
            }
        }

        public NSPayLoadMessage()
            : base()
        {

        }

        public NSPayLoadMessage(string command, ArrayList commandValues, string payload)
            : base(command, commandValues)
        {
            payLoad = payload;
        }

        public NSPayLoadMessage(string command, string[] commandValues, string payload)
            : base(command, new ArrayList(commandValues))
        {
            payLoad = payload;
        }

        public NSPayLoadMessage(string command, string payload)
            : base(command)
        {
            payLoad = payload;
        }

        public override byte[] GetBytes()
        {
            StringBuilder cmdBuilder = new StringBuilder();
            cmdBuilder.Append(Command);
            cmdBuilder.Append(' ');
            cmdBuilder.Append(TransactionID.ToString(System.Globalization.CultureInfo.InvariantCulture));

            foreach (string val in CommandValues)
            {
                cmdBuilder.Append(' ');
                cmdBuilder.Append(val);
            }

            cmdBuilder.Append(' ');

            cmdBuilder.Append(System.Text.Encoding.UTF8.GetBytes(payLoad).Length);
            cmdBuilder.Append("\r\n");
            cmdBuilder.Append(payLoad);

            return System.Text.Encoding.UTF8.GetBytes(cmdBuilder.ToString());
        }
    }
};
