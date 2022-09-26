using System;
using System.Threading;

namespace Communication.Core
{
    /// <summary>
    /// Persistent TCP client with retry auto connect on disconnection
    /// </summary>
    public interface ITcpClientPersistent
    {
        EventHandler<Reply> DataEvent { get; set; }
        EventHandler<Exception> ExceptionHandler { get; set; }
        bool IsConnected { get; }
        string ServerIP { get; set; }
        int Port { get; set; }
        bool IsUsingEndPacketIdentifier { get; }
        void SetEndPacketIdentifier(byte[] endPacketIdentifier);
        ReadOnlySpan<byte> GetEndPacketIdentifier();
        void RemoveEndPacketIdentifier();
        bool ConnectToServer();
        bool Disconnect();
        bool AutoRetryConnect { get; set; }
        ThreadPriority Priority { get; set; }
        bool SendDataAsync(Memory<byte> command);
    }
}
