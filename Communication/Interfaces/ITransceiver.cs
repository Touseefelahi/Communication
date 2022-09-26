using System;

namespace Communication.Core
{
    /// <summary>
    /// Generic interface for Transceiver (UDP/TCP)
    /// </summary>
    public interface ITransceiver : ISendable
    {
        /// <summary>
        /// For Multi network interface, Set this IP to use specific network interface for communication.
        /// If null library will use any interface (if available)
        /// </summary>
        string HostIP { get; set; }

        /// <summary>
        /// IP of receiver
        /// </summary>
        string IP { get; set; }

        /// <summary>
        /// Logs the raw bytes if set
        /// </summary>
        Action<string> Log { get; set; }

        /// <summary>
        /// TCP port of receiver
        /// </summary>
        int PortTcp { get; set; }

        /// <summary>
        /// UDP Port of receiver
        /// </summary>
        int PortUdp { get; set; }
    }
}