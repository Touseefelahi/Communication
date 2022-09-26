using System;

namespace Communication.Core
{
    /// <summary>
    /// General Reply of all type of Ethernet communication
    /// </summary>
    public class Reply : IReply
    {
        /// <summary>
        /// Error to display if any
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// IP address of the sender
        /// </summary>
        public string SenderIP { get; set; }

        /// <summary>
        /// Sending Port
        /// </summary>
        public int SenderPort { get; set; }

        /// <summary>
        /// Command Status
        /// </summary>
        public CommunicationStatus Status { get; set; } = CommunicationStatus.ConnectionTimeout;

        /// <summary>
        /// Raw reply packet
        /// </summary>
        public Memory<byte> RawBytes { get; set; }

        /// <summary>
        /// It sets the list of byte
        /// </summary>
        /// <param name="rawBytes"></param>
        public void SetReply(Memory<byte> rawBytes)
        {
            RawBytes = rawBytes;
            Status = CommunicationStatus.ReplyReceived;
        }
    }
}