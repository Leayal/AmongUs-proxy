using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AmongUs_proxy
{
    class UdpProxy
    {
        private ConcurrentDictionary<IPEndPoint, UdpClientEx> clients;
        private Task _cleanupHangClients;
        private UdpClient server;
        private bool isStarted;
        private bool Verbose { get; set; }

        public UdpProxy()
        {
            this.clients = new ConcurrentDictionary<IPEndPoint, UdpClientEx>();
            this.isStarted = false;
            this.Verbose = false;
        }

        public void Stop()
        {
            if (!this.isStarted) return;
            this.isStarted = false;

            this.server.Close();
            this.server.Dispose();
        }

        public async Task Start(string remoteServerIp, ushort remoteServerPort, ushort localPort, string localIp = null)
        {
            if (this.isStarted) return;
            this.isStarted = true;

            this.server = new UdpClient(AddressFamily.InterNetworkV6);
            this.server.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

            IPAddress localIpAddress = string.IsNullOrEmpty(localIp) ? IPAddress.IPv6Any : IPAddress.Parse(localIp);
            server.Client.Bind(new IPEndPoint(localIpAddress, localPort));
            if (this.Verbose)
            {
                Console.WriteLine($"[PROXY] UDP Proxy started: {localIpAddress}:{localPort} -> {remoteServerIp}:{remoteServerPort}");
            }
            while (this.isStarted)
            {
                try
                {
                    var message = await server.ReceiveAsync();
                    var endpoint = message.RemoteEndPoint;
                    var client = clients.GetOrAdd(endpoint, (ep) =>
                    {
                        this.CleanupHangClients();
                        return new UdpClientEx(this, endpoint, new IPEndPoint(IPAddress.Parse(remoteServerIp), remoteServerPort));
                    });
                    await client.SendToServer(message.Buffer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PROXY] An exception occurred on recieving a client datagram: {ex}");
                }
            }
        }

        private void CleanupHangClients()
        {
            if (this._cleanupHangClients == null)
            {
                this._cleanupHangClients = this.CleanupHangClients2();
            }
        }

        private async Task CleanupHangClients2()
        {
            await Task.Delay(1000).ConfigureAwait(false);
            int count = clients.Count;
            if (count == 0)
            {
                this._cleanupHangClients = null;
                return;
            }
            var buffer = new UdpClientEx[count];
            clients.Values.CopyTo(buffer, 0);
            foreach (var udpClient in buffer)
            {
                if (udpClient.lastActivity + TimeSpan.FromSeconds(60) < DateTime.UtcNow)
                {
                    if (clients.TryRemove(udpClient.ClientEndpoint, out var c))
                    {
                        udpClient.Stop();
                    }
                }
            }
            await CleanupHangClients2();
        }

        class UdpClientEx : IDisposable
        {
            private readonly UdpProxy _server;
            public UdpClientEx(UdpProxy server, IPEndPoint clientEndpoint, IPEndPoint remoteServer)
            {
                _server = server;

                _isRunning = true;
                RemoteServer = remoteServer;
                ClientEndpoint = clientEndpoint;
                if (server.Verbose)
                {
                    Console.WriteLine($"[PROXY] Established {clientEndpoint} => {remoteServer}");
                }
                Run();
            }

            public readonly UdpClient client = new UdpClient();
            public DateTime lastActivity = DateTime.UtcNow;
            public readonly IPEndPoint ClientEndpoint;
            public readonly IPEndPoint RemoteServer;
            private bool _isRunning;
            private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

            public async Task SendToServer(byte[] message)
            {
                lastActivity = DateTime.UtcNow;

                await _tcs.Task;
                var sent = await client.SendAsync(message, message.Length, RemoteServer);
                if (this._server.Verbose)
                {
                    Console.WriteLine($"[PROXY] {sent} bytes sent from a client message of {message.Length} bytes from {ClientEndpoint} to {RemoteServer}");
                }
            }

            private void Run()
            {
                Task.Run(async () =>
                {
                    client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
                    _tcs.SetResult(true);
                    using (client)
                    {
                        while (this._isRunning)
                        {
                            try
                            {
                                var result = await client.ReceiveAsync();
                                lastActivity = DateTime.UtcNow;
                                var sent = await _server.server.SendAsync(result.Buffer, result.Buffer.Length, ClientEndpoint);
                                if (this._server.Verbose)
                                {
                                    Console.WriteLine($"[PROXY] {sent} bytes sent from a return message of {result.Buffer.Length} bytes from {RemoteServer} to {ClientEndpoint}");
                                }

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[PROXY] An exception occurred while recieving a server datagram : {ex}");
                            }
                        }
                    }

                });
            }

            public void Stop()
            {
                if (this._server.Verbose)
                {
                    Console.WriteLine($"[PROXY] Closed {ClientEndpoint} => {RemoteServer}");
                }
                this._isRunning = false;
            }

            public void Dispose()
            {
                this.client.Dispose();
            }
        }
    }
}
