using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Communication.Core
{
    /// <summary>
    /// General service for Persistent TCP client with retry auto connect on disconnection
    /// </summary>
    public class TcpClientPersistent : ITcpClientPersistent, INotifyPropertyChanged
    {
        private TcpClient clientTcp;
        private Thread dataReceptionThread;
        private byte[] endPacketIdentifier;
        private ThreadPriority priority = ThreadPriority.Normal;

        public TcpClientPersistent()
        {
        }

        public TcpClientPersistent(string serverIP, int port) : this()
        {
            if (NetworkService.ValidateIPv4(serverIP) is false)
            {
                throw new Exception("Invalid Server IP for TCP Persistent server");
            }
            ServerIP = serverIP;
            Port = port;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool AutoRetryConnect { get; set; }
        public EventHandler<Reply> DataEvent { get; set; }
        public EventHandler<Exception> ExceptionHandler { get; set; }
        public bool IsConnected { get; private set; }
        public bool IsRetrying { get; private set; }
        public bool IsUsingEndPacketIdentifier { get; }
        public int Port { get; set; }

        public ThreadPriority Priority
        {
            get => priority;
            set
            {
                priority = value;
                dataReceptionThread.Priority = value;
            }
        }

        public string ServerIP { get; set; }

        public bool ConnectToServer()
        {
            if (IsConnected)
            {
                return IsConnected;
            }
            clientTcp = new TcpClient() { ReceiveTimeout = 1000, SendTimeout = 1000 };
            try
            {
                IAsyncResult result = clientTcp.BeginConnect(ServerIP, Port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(200, true);
                IsConnected = clientTcp.Connected;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
                if (!IsConnected)
                {
                    RetryConnectionAsync();
                    return IsConnected;
                }
            }
            catch (Exception ex)
            {
                RetryConnectionAsync();
                ExceptionHandler?.Invoke(this, ex);
                return IsConnected;
            }

            dataReceptionThread = new Thread(StartReceptionRoutine)
            {
                IsBackground = true,
                Priority = Priority,
            };
            dataReceptionThread.Name = $"Prs. TCP {ServerIP}:{Port} Thread";
            dataReceptionThread.Start();
            return IsConnected;
        }

        public bool Disconnect()
        {
            try
            {
                IsConnected = false;
                clientTcp?.Close();
            }
            catch (Exception ex)
            {
                ExceptionHandler?.Invoke(this, ex);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
            return IsConnected;
        }

        public ReadOnlySpan<byte> GetEndPacketIdentifier()
        {
            return endPacketIdentifier;
        }

        public void RemoveEndPacketIdentifier()
        {
            endPacketIdentifier = null;
        }

        public bool SendDataAsync(Memory<byte> command)
        {
            if (clientTcp is null || clientTcp.Connected is false)
            {
                return false;
            }
            _ = clientTcp.GetStream().WriteAsync(command);
            return true;
        }

        public void SetEndPacketIdentifier(byte[] endPacketIdentifier)
        {
            this.endPacketIdentifier = endPacketIdentifier;
        }

        private async void RetryConnectionAsync()
        {
            if (IsRetrying)
            {
                return;
            }
            IsRetrying = true;
            do
            {
                await Task.Delay(1000).ConfigureAwait(false);
                ConnectToServer();
            } while (IsConnected is not true);
            IsRetrying = false;
        }

        private void StartReceptionRoutine()
        {
            try
            {
                NetworkStream stream = clientTcp.GetStream();
                Reply replyPacket = new();

                IsConnected = true;
                while (clientTcp.Connected)
                {
                    if (!IsConnected)
                    {
                        break;
                    }

                    while (stream.DataAvailable)
                    {
                        try
                        {
                            byte[] packet = new byte[clientTcp.Available];
                            stream.Read(packet);
                            replyPacket.SetReply(packet);
                            DataEvent?.Invoke(this, replyPacket);
                        }
                        catch (Exception ex)
                        {
                            ExceptionHandler?.Invoke(this, ex);
                        }
                    }
                    Thread.Sleep(5);
                    if (clientTcp.Client is not null && clientTcp.Client.Poll(1, SelectMode.SelectRead) && !stream.DataAvailable)
                    {
                        //This means we were connected to the server and server disconnected without our intention,
                        //we need to retry connection
                        Disconnect();
                        RetryConnectionAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                RetryConnectionAsync();
                ExceptionHandler?.Invoke(this, ex);
            }
            finally
            {
            }
        }
    }
}