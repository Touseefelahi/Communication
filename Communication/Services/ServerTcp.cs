using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Communication.Core
{
    /// <summary>
    /// This is general Server for TCP communication. if started this service it will start
    /// listening to the port provided.
    /// </summary>
    public class ServerTcp : IListener
    {
        private const int RxTimeout = 1000;
        private readonly IMemoryOwner<byte> MemoryOwner;
        private TcpListener listener;

        public ServerTcp()
        {
            MemoryOwner = MemoryPool<byte>.Shared.Rent(512);
        }

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
            listener = new TcpListener(IPAddress.Any, Port);

            try
            {
                listener.Start();
                IsListening = true;
            }
            catch (SocketException ex)
            {
                IsListening = false;
                ExceptionHandler?.Invoke(null, ex);
                return IsListening;
            }
            _ = Task.Run(async () =>
              {
                  while (IsListening)
                  {
                      try
                      {
                          TcpClient clientTCP = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                          _ = Task.Run(() => ReceiveData(clientTCP));
                      }
                      catch (SocketException ex)
                      {
                          ExceptionHandler?.Invoke(this, ex);
                      }
                  }
                  listener.Server.Close();
              });

            return IsListening;
        }

        public bool StopListener()
        {
            try
            {
                listener.Server.Close();
                IsListening = false;
            }
            catch (SocketException ex)
            {
                ExceptionHandler?.Invoke(null, ex);
                return IsListening;
            }
            return IsListening;
        }

        private void ReceiveData(TcpClient clientTCP)
        {
            IReply replyPacket = new Reply
            {
                SenderPort = ((IPEndPoint)clientTCP.Client.RemoteEndPoint).Port,
                SenderIP = ((IPEndPoint)clientTCP.Client.RemoteEndPoint).Address.ToString()
            };
            NetworkStream stream = clientTCP.GetStream();
            stream.ReadTimeout = RxTimeout;
            try
            {
                int firstByte = stream.ReadByte();
                if (firstByte is not -1)
                {
                    replyPacket.RawBytes = new Memory<byte>(new byte[clientTCP.Available + 1]);
                    _ = stream.Read(replyPacket.RawBytes.Span[1..]);
                    replyPacket.RawBytes.Span[0] = (byte)firstByte;
                    replyPacket.Status = CommunicationStatus.ReplyReceived;
                    Log?.Invoke("Rx: " + StringHelper.GetHexString(replyPacket.RawBytes));
                    DataEvent?.Invoke(this, replyPacket);
                }
            }
            catch { }
            finally
            {
                stream.Close();
                clientTCP.Close();
            }
        }
    }
}