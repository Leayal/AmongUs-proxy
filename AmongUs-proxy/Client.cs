using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;

namespace AmongUs_proxy
{
    public static class Client
    {
        public static Task<Connection> Connect(string remoteHost, int remotePort) => Connect(remoteHost, remotePort, TimeSpan.FromSeconds(5));

        public static Task<Connection> Connect(string remoteHost, int remotePort, TimeSpan timeOut)
        {
            return Task.Run(async () =>
            {
                var tcpClient = new TcpClient();
                tcpClient.ReceiveTimeout = 5000;
                tcpClient.LingerState = new LingerOption(false, 0);
                tcpClient.SendTimeout = 5000;
                var connecting = tcpClient.ConnectAsync(remoteHost, remotePort);
                var cancellationCompletionSrc = new TaskCompletionSource<bool>();
                using (var cts = new CancellationTokenSource(timeOut))
                {
                    using (cts.Token.Register(() => cancellationCompletionSrc.TrySetResult(true)))
                    {
                        if (connecting == await Task.WhenAny(connecting, cancellationCompletionSrc.Task))
                        {
                            await connecting;
                            return new Connection(tcpClient);
                        }
                        else
                        {
                            throw new TimeoutException("Connection attempt has timed out. Cannot connect to the destination.");
                        }
                    }
                }
            });
        }
    }
}
