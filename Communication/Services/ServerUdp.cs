using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Communication.Core
{
    /// <summary>
    /// This is general Server for UDP communication. if started this service it will start
    /// listening to the port provided.
    /// </summary>
    public class ServerUdp : IListener
    {
        private UdpClient listener;
        public EventHandler<IReply> DataEvent { get; set; }
        public EventHandler<Exception> ExceptionHandler { get; set; }
        public bool IsListening { get; private set; }
        public int Port { get; set; }

        /// <summary>
        /// Logs the raw bytes if set
        /// </summary>
        public Action<string> Log { get; set; }

        public bool StartListener()
        {
            if (IsListening)
            {
                return IsListening;
            }

            IPEndPoint groupEP = new(IPAddress.Any, Port);
            try
            {
                listener = new UdpClient(Port);
                IsListening = true;
            }
            catch (SocketException ex)
            {
                ExceptionHandler?.Invoke(null, ex);
                IsListening = false;
                return IsListening;
            }

            Task.Run(() =>
            {
                try
                {
                    while (IsListening)
                    {
                        byte[] bytes = listener.Receive(ref groupEP);
                        IReply replyPacket = new Reply() { SenderIP = groupEP.Address.ToString(), SenderPort = groupEP.Port };
                        replyPacket.SetReply(bytes);
                        DataEvent?.Invoke(this, replyPacket);
                        Log?.Invoke("Rx: " + BitConverter.ToString(bytes).Replace('-', ' '));
                    }
                }
                catch (SocketException ex)
                {
                    ExceptionHandler?.Invoke(null, ex);
                    IsListening = false;
                }
            });
            return IsListening;
        }

        public bool StopListener()
        {
            try
            {
                IsListening = false;
                listener.Close();
            }
            catch (SocketException ex)
            {
                ExceptionHandler?.Invoke(null, ex);
            }
            return IsListening;
        }
    }
}