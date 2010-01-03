using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Library
{
    /// <summary>
    /// Class that represents a packet sent FROM the sersver TO us. Contains 
    /// only relevant information for sending command data.
    /// </summary>
    public class dAmnCommandPacket
    {
        #region Public Properties
        /// <summary>
        /// Who the packet was sent from.
        /// </summary>
        public string From { get; private set; }

        /// <summary>
        /// Data payload.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Who the packet was intended for.
        /// </summary>
        public string Target { get; private set; }

        /// <summary>
        /// Response to an event. 
        /// </summary>
        public string EventResponse { get; private set; }

        /// <summary>
        /// The original packet.
        /// </summary>
        public dAmnServerPacket Packet { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor. Takes original packet to parse information.
        /// </summary>
        /// <param name="packet">Packet containing command data.</param>
        public dAmnCommandPacket(dAmnServerPacket packet)
        {
            // init variables
            string from;
            string message;
            string target;
            string eventResponse;

            // get details about the packet
            dAmnServerPacket.SortdAmnPacket(packet, out from, out message, out target, out eventResponse);

            // save to our properties
            From = from;
            Message = message;
            Target = target;
            EventResponse = eventResponse;

            // save original packet
            Packet = packet;
        }        
        #endregion
    }
}
