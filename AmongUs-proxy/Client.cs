using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections;
using System.Threading.Tasks;

namespace AmongUs_proxy
{
    public static class Client
    {
        public static Task<Connection> Connect(string remoteHost, int remotePort)
        {
            return Task.Run(async () =>
            {
                var tcpClient = new TcpClient();
                tcpClient.ReceiveTimeout = 5000;
                tcpClient.LingerState = new LingerOption(false, 0);
                tcpClient.SendTimeout = 5000;
                await tcpClient.ConnectAsync(remoteHost, remotePort);
                return new Connection(tcpClient);
            });
        }
    }
}
