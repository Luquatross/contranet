using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace DeviantArt.Chat.Library
{
    /// <summary>
    /// Methods a socket class must implement to be used to connect to the dAmn server.
    /// </summary>
    public interface IdAmnSocket
    {
        /// <summary>
        /// True if socket is connected.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// The time in milliseconds to wait to receive data.
        /// </summary>
        int ReceiveTimeout { set; }

        /// <summary>
        /// Returns the underlying stream.
        /// </summary>
        /// <returns>The underlying stream.</returns>
        NetworkStream GetStream();

        /// <summary>
        /// Opens the socket.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the socket.
        /// </summary>
        void Close();
    }

    /// <summary>
    /// Concrete dAmn socket class.
    /// </summary>
    public class dAmnSocket : IdAmnSocket
    {
        #region Private Variables
        /// <summary>
        /// Internal Socket.
        /// </summary>
        private TcpClient Socket;
        #endregion

        #region IdAmnSocket Members

        public bool Connected
        {
            get { return Socket.Connected; }
        }

        public int ReceiveTimeout
        {
            set { Socket.ReceiveTimeout = value; }
        }

        public NetworkStream GetStream()
        {
            return Socket.GetStream();
        }

        public void Open()
        {
            Socket = new TcpClient("chat.deviantart.com", 3900);
        }

        public void Close()
        {
            Socket.Close();
        }

        #endregion
    }
}
