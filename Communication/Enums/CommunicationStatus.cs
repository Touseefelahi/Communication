namespace Communication.Core
{
    /// <summary>
    /// Generic Camera feature type for All type of cameras
    /// </summary>
    public enum CommunicationStatus
    {
        /// <summary>
        /// Failed see Error
        /// </summary>
        Failed,

        /// <summary>
        /// Connection timeout (only valid for TCP socket)
        /// </summary>
        ConnectionTimeout,

        /// <summary>
        /// Waited for given RxTimeout but nothing received
        /// </summary>
        ReadTimeout,

        /// <summary>
        /// Sent successful
        /// </summary>
        Sent,

        /// <summary>
        /// Sent and reply received
        /// </summary>
        ReplyReceived,
    }
}