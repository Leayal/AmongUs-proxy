using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace AmongUs_proxy
{
    static class Constants
    {
        public static readonly IPAddress LanIP;

        static Constants()
        {
            LanIP = null;
            var networkinterfaces = NetworkInterface.GetAllNetworkInterfaces();
            
            // Prefer 192.168.*.*
            foreach (var networkinterface in networkinterfaces)
            {
                if (networkinterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet || networkinterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    if (networkinterface.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        foreach (var ip in networkinterface.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                var ip_raw = ip.Address.GetAddressBytes();
                                if (ip_raw[0] == 192 && ip_raw[1] == 168)
                                {
                                    LanIP = ip.Address;
                                }
                            }
                        }
                    }
                }
            }

            if (LanIP == null)
            {
                // In case the LAN is in another network.
                foreach (var networkinterface in networkinterfaces)
                {
                    if (networkinterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet || networkinterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        if (networkinterface.Supports(NetworkInterfaceComponent.IPv4))
                        {
                            foreach (var ip in networkinterface.GetIPProperties().UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    LanIP = ip.Address;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
