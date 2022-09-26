using System;

namespace Communication.Core
{
    public interface IListener
    {
        /// <summary>
        /// Whenever it receives packet this event fires up
        /// </summary>
        EventHandler<IReply> DataEvent { get; set; }

        /// <summary>
        /// This event fires up whenever there's an exception
        /// </summary>
        EventHandler<Exception> ExceptionHandler { get; set; }

        /// <summary>
        /// Is server is enabled/listening
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Listening port
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Logs the raw bytes if set
        /// </summary>
        Action<string> Log { get; set; }

        /// <summary>
        /// Start the server
        /// </summary>
        /// <returns></returns>
        bool StartListener();

        /// <summary>
        /// Starts the server with the given port
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool StartListener(int port) { Port = port; return StartListener(); }

        /// <summary>
        /// Stops the server/listener
        /// </summary>
        /// <returns></returns>
        bool StopListener();
    }
}