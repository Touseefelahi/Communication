using System;
using System.Threading.Tasks;

namespace Communication.Core
{
    /// <summary>
    /// Generic interface for any data send able (UDP/TCP)
    /// </summary>
    public interface ISendable
    {
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
        Task<IReply> SendTcpAsync(byte[] bytes2Send, bool isReplyRequired = true, int txTimeout = 1000, int rxTimeout = 1000, int connectionTimeout = 1000, Action<bool> fireOnSent = null);

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
        Task<IReply> SendUdpAsync(byte[] bytes2Send, bool isReplyRequired = true, int txTimeout = 1000, int rxTimeout = 1000, Action<bool> fireOnSent = null);
    }
}