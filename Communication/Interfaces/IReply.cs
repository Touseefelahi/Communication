using System;

namespace Communication.Core
{
    /// <summary>
    /// Generic interface Reply
    /// </summary>
    public interface IReply
    {
        /// <summary>
        /// Error to display if any
        /// </summary>
        string Error { get; set; }

        /// <summary>
        /// IP address of the sender
        /// </summary>
        string SenderIP { get; set; }

        /// <summary>
        /// Command Status
        /// </summary>
        CommunicationStatus Status { get; set; }

        /// <summary>
        /// Sending Port
        /// </summary>
        int SenderPort { get; set; }

        /// <summary>
        /// Raw reply packet
        /// </summary>
        Memory<byte> RawBytes { get; set; }

        /// <summary>
        /// It sets the <see cref="RawBytes"/>
        /// </summary>
        /// <param name="rawBytes"></param>
        void SetReply(Memory<byte> rawBytes);
    }
}