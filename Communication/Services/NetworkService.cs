using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Communication.Core
{
    /// <summary>
    /// Address Information
    /// </summary>
    /// <param name="IP">IP address</param>
    /// <param name="MaxMtu">Max MTU size in bytes</param>
    /// <param name="Speed">Speed in Mbps</param>
    public record AddressInfo(string IP, int MaxMtu, int Speed)
    {
        public override string ToString()
        {
            return $"IP: {IP}, MaxMTU: {MaxMtu}, Speed {Speed}";
        }

        /// <summary>
        /// Gets complete Network Adapter information, Use GetIPProperties GetIPv4Properties GetIPv6Properties for more information about NetworkIn
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.net.networkinformation.networkinterface.getipproperties?view=net-6.0"/>
        /// </summary>
        /// <returns><see cref="NetworkInterface"/></returns>
        public NetworkInterface GetInterfaceInfo()
        {
            return NetworkService.GetInterfaceInfo(IP);
        }
    }

    public static class NetworkService
    {
        /// <summary>
        /// Get all ethernet interfaces for the device
        /// </summary>
        /// <param name="ipVersion4Only"></param>
        /// <param name="skipWireless"></param>
        /// <param name="allowedMask"></param>
        /// <returns></returns>
        public static IEnumerable<AddressInfo> GetAllInterfaces(bool ipVersion4Only = true, bool skipWireless = false, string allowedMask = "255.255.255.0")
        {
            List<AddressInfo> interfaces = new();
            foreach (var network in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (network.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                if (network.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                if (skipWireless)
                {
                    if (network.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        continue;
                    }
                }
                IPInterfaceProperties properties = network.GetIPProperties();
                var ipProperties = properties.GetIPv4Properties();
                foreach (UnicastIPAddressInformation address in properties.UnicastAddresses)
                {
                    if (IPAddress.IsLoopback(address.Address))
                    {
                        continue;
                    }
                    var addresses = properties.GatewayAddresses;
                    if ((addresses == null) || (addresses.Count == 0))
                    {
                        var isValid = IPAddress.TryParse(allowedMask, out IPAddress validMask);
                        if (isValid is false)
                        {
                            continue;
                        }

                        if (address.IPv4Mask.Equals(validMask) is false)
                        {
                            continue;
                        }
                    }
                    if (ipVersion4Only)
                    {
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                            interfaces.Add(new AddressInfo(address.Address.ToString(), ipProperties.Mtu, (int)(network.Speed / 1000_000)));
                    }
                    else
                    {
                        interfaces.Add(new AddressInfo(address.Address.ToString(), ipProperties.Mtu, (int)(network.Speed / 1000_000)));
                    }
                }
            }
            return interfaces;
        }

        /// <summary>
        /// Gets complete Network Adapter information, Use GetIPProperties GetIPv4Properties GetIPv6Properties for more information about NetworkIn
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.net.networkinformation.networkinterface.getipproperties?view=net-6.0"/>
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns><see cref="NetworkInterface"/></returns>
        public static NetworkInterface GetInterfaceInfo(AddressInfo address)
        {
            return GetInterfaceInfo(address.IP);
        }

        /// <summary>
        /// Gets complete Network Adapter information, Use GetIPProperties GetIPv4Properties GetIPv6Properties for more information about NetworkIn
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.net.networkinformation.networkinterface.getipproperties?view=net-6.0"/>
        /// </summary>
        /// <param name="ip">IP</param>
        /// <returns><see cref="NetworkInterface"/></returns>
        public static NetworkInterface GetInterfaceInfo(string ip)
        {
            foreach (var network in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (network.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                IPInterfaceProperties properties = network.GetIPProperties();
                foreach (UnicastIPAddressInformation address in properties.UnicastAddresses)
                {
                    if (string.Equals(address.Address.ToString(), ip))
                    {
                        return network;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get the System IP (For multi-network Static IP will be prefered by default)
        /// </summary>
        /// <returns></returns>
        public static string GetMyIp(bool preferStaticIP = true, bool skipWireless = true, string allowedMask = "255.255.255.0")
        {
            UnicastIPAddressInformation mostSuitableIp = null;

            foreach (var network in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (network.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }
                IPInterfaceProperties properties = network.GetIPProperties();
                if (skipWireless)
                {
                    if (network.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        continue;
                    }
                }

                foreach (UnicastIPAddressInformation address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    if (IPAddress.IsLoopback(address.Address))
                    {
                        continue;
                    }

                    if (properties.GatewayAddresses.Count == 0)
                    {
                        var isValid = IPAddress.TryParse(allowedMask, out IPAddress validMask);
                        if (isValid)
                        {
                            continue;
                        }

                        if (address.IPv4Mask.Equals(validMask) is false)
                        {
                            continue;
                        }
                    }

                    if (!address.IsDnsEligible)
                    {
                        mostSuitableIp ??= address;

                        continue;
                    }

                    //I know this logic is stupid, but its simple
                    if (preferStaticIP)
                    {
                        if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                        {
                            if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                            {
                                mostSuitableIp = address;
                            }

                            continue;
                        }
                    }
                    else
                    {
                        if (address.PrefixOrigin == PrefixOrigin.Dhcp)
                        {
                            if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                            {
                                mostSuitableIp = address;
                            }

                            continue;
                        }
                    }

                    return address.Address.ToString();
                }
            }

            return mostSuitableIp != null
                ? mostSuitableIp.Address.ToString()
                : "";
        }

        /// <summary>
        /// Validates the IPv4
        /// </summary>
        /// <param name="ipString">Input IP</param>
        /// <returns></returns>
        public static bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }
    }
}

//This is to solve the bug it's only for .netcoreApp3.x -solution here: https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    { }
}