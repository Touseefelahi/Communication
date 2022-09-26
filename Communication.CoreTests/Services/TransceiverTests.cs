namespace Communication.Core.Tests
{
    [TestClass()]
    public class TransceiverTests
    {
        /// <summary>
        /// Tested with packet sender
        /// </summary>
        /// <returns></returns>
        [TestMethod()]
        public async Task SendTcpAsyncTest()
        {
            Transceiver transceiver = new()
            {
                IP = "192.168.10.146",
                PortUdp = 50447,
                PortTcp = 59794,
                HostIP = "192.168.10.227",
            };
            byte[] command = new byte[] { 170, 3, 107, 249, 0, 0, 0, 0, 1, 255, 1, 0, 0, 0, 0, 0, 0, 13, 199 };
            var reply = await transceiver.SendTcpAsync(command);
        }

        /// <summary>
        /// Tested with packet sender
        /// </summary>
        /// <returns></returns>
        [TestMethod()]
        public async Task SendUdpAsyncTest()
        {
            Transceiver transceiver = new()
            {
                IP = "192.168.10.227",
                PortUdp = 50447,
                PortTcp = 59794,
                // HostIP = "192.168.10.146",
            };
            byte[] command = new byte[] { 170, 3, 107, 249, 0, 0, 0, 0, 1, 255, 1, 0, 0, 0, 0, 0, 0, 13, 199 };
            var reply = await transceiver.SendUdpAsync(command);
        }
    }
}