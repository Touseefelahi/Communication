using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Communication.Core
{
    /// <summary>
    /// Encapsulates the UDP/TCP sockets for transmission and reception
    /// </summary>
    public class Transceiver : ITransceiver
    {
        private string hostIP;
        private bool isUsingSpecificInterface;

        public Transceiver()
        {
        }

        /// <summary>
        /// Constructor with initialization of properties
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="portTcp"></param>
        /// <param name="portUdp"></param>
        public Transceiver(string ip, int portTcp, int portUdp)
        {
            IP = ip ?? throw new ArgumentNullException(nameof(ip));
            PortTcp = portTcp;
            PortUdp = portUdp;
        }

        /// <summary>
        /// For Multi network interface, Set this IP to use specific network interface for communication.
        /// If null library will use any interface
        /// </summary>
        public string HostIP
        {
            get => hostIP;
            set
            {
                hostIP = value;
                isUsingSpecificInterface = NetworkService.ValidateIPv4(hostIP);
            }
        }

        /// <summary>
        /// Receiver IP
        /// </summary>
        public string IP { get; set; }

        public Action<string> Log { get; set; }

        /// <summary>
        /// Receiver TCP Port
        /// </summary>
        public int PortTcp { get; set; }

        /// <summary>
        /// Receiver UDP Port
        /// </summary>
        public int PortUdp { get; set; }

        /// <summary>
        /// This is generic function which will send TCP command to specific IP, port and return the
        /// reply if required
        /// </summary>
        /// <param name="bytes2Send">Command to be sent</param>
        /// <param name="isReplyRequired">Wait for reply?</param>
        /// <param name="txTimeout">Transmission timeout (ms)</param>
        /// <param name="rxTimeout">Receive timeout (ms)</param>
        /// <param name="connectionTimeout">Connection timeout (ms)</param>
        /// <param name="fireOnSent">Fires event on packet sent</param>
        /// <returns>Reply packet</returns>
        public async Task<IReply> SendTcpAsync(byte[] bytes2Send, bool isReplyRequired = true, int txTimeout = 1000, int rxTimeout = 1000, int connectionTimeout = 1000, Action<bool> fireOnSent = null)
        {
            IReply reply = new Reply() { SenderIP = IP, SenderPort = PortTcp };
            if (bytes2Send == null)
            {
                reply.Error = "Bytes to send is null";
                return reply;
            }
            try
            {
                //using Socket tcpSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                using TcpClient tcpSocket = isUsingSpecificInterface ? new(new IPEndPoint(IPAddress.Parse(HostIP), 0)) : new();
                tcpSocket.Client.ReceiveTimeout = rxTimeout;
                tcpSocket.Client.SendTimeout = txTimeout;
                IAsyncResult connectResult = tcpSocket.BeginConnect(IP, PortTcp, null, null);
                Task<bool> connectionTask = new(() =>
                connectResult.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(connectionTimeout)));
                connectionTask.Start();
                if (await connectionTask.ConfigureAwait(false))
                {
                    using NetworkStream stream = tcpSocket.GetStream();
                    await stream.WriteAsync(bytes2Send.AsMemory(0, bytes2Send.Length)).ConfigureAwait(false);
                    stream.Flush();
                    reply.Status = CommunicationStatus.Sent;
                    fireOnSent?.Invoke(true);
                    Log?.Invoke("Tx: " + BitConverter.ToString(bytes2Send).Replace('-', ' '));

                    if (isReplyRequired)
                    {
                        await Task.Run(() => ReceiveData(reply, tcpSocket, stream, rxTimeout)).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                reply.Error = ex.Message;
            }
            return reply;
        }

        /// <summary>
        /// This is generic function which will send UDP command to specific IP, port and return the
        /// reply if required
        /// </summary>
        /// <param name="bytes2Send"></param>
        /// <param name="isReplyRequired"></param>
        /// <param name="txTimeout"></param>
        /// <param name="rxTimeout"></param>
        /// <param name="fireOnSent">Fires event on packet sent</param>
        /// <returns>Reply packet</returns>
        public async Task<IReply> SendUdpAsync(byte[] bytes2Send, bool isReplyRequired = true, int txTimeout = 1000, int rxTimeout = 1000, Action<bool> fireOnSent = null)
        {
            IReply reply = new Reply() { SenderIP = IP, SenderPort = PortUdp };
            if (bytes2Send is null)
            {
                reply.Error = "Bytes to send is null";
                return reply;
            }
            try
            {
                using UdpClient udpSocket = isUsingSpecificInterface ? new(new IPEndPoint(IPAddress.Parse(HostIP), 0)) : new();
                udpSocket.Client.SendTimeout = txTimeout;
                udpSocket.Client.ReceiveTimeout = rxTimeout;
                udpSocket.Send(bytes2Send, bytes2Send.Length, IP, PortUdp);
                reply.Status = CommunicationStatus.Sent;
                fireOnSent?.Invoke(true);
                Log?.Invoke("Tx: " + BitConverter.ToString(bytes2Send).Replace('-', ' '));
                if (isReplyRequired)
                {
                    reply.Status = CommunicationStatus.ReadTimeout; //It will be changed if data received
                    Task<UdpReceiveResult> taskRecievePacket = udpSocket.ReceiveAsync();
                    if (await Task.WhenAny(taskRecievePacket, Task.Delay(rxTimeout)).ConfigureAwait(false) == taskRecievePacket)
                    {
                        reply.SetReply(taskRecievePacket.Result.Buffer);
                        Log?.Invoke("Rx: " + BitConverter.ToString(taskRecievePacket.Result.Buffer).Replace('-', ' '));
                    }
                }
            }
            catch (Exception ex)
            {
                reply.Error = ex.Message;
            }
            return reply;
        }

        private void ReceiveData(IReply reply, TcpClient tcpSocket, NetworkStream stream, int rxTimeout)
        {
            reply.Status = CommunicationStatus.ReadTimeout; //It will be changed if data received
            stream.ReadTimeout = rxTimeout;
            try
            {
                int firstByte = stream.ReadByte();
                if (firstByte is not -1)
                {
                    reply.RawBytes = new Memory<byte>(new byte[tcpSocket.Available + 1]);
                    _ = stream.Read(reply.RawBytes.Span[1..]);
                    reply.RawBytes.Span[0] = (byte)firstByte;
                    reply.Status = CommunicationStatus.ReplyReceived;
                    Log?.Invoke("Rx: " + StringHelper.GetHexString(reply.RawBytes));
                }
            }
            catch { }
        }
    }
}