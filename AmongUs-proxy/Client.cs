using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text;

namespace AmongUs_proxy
{
    public sealed class Client : IDisposable
    {
        public static Task<Client> Connect(string remoteHost, int remotePort, bool localonly) => Connect(remoteHost, remotePort, localonly, TimeSpan.FromSeconds(5));

        public static Task<Client> Connect(string remoteHost, int remotePort, bool localonly, TimeSpan timeOut)
        {
            return Task.Run(async () =>
            {
                var tcpClient = new TcpClient();
                tcpClient.ReceiveTimeout = (int)timeOut.TotalMilliseconds;
                tcpClient.LingerState = new LingerOption(false, 0);
                tcpClient.SendTimeout = (int)timeOut.TotalMilliseconds;
                var connecting = tcpClient.ConnectAsync(remoteHost, remotePort);
                var cancellationCompletionSrc = new TaskCompletionSource<bool>();
                using (var cts = new CancellationTokenSource(timeOut))
                {
                    using (cts.Token.Register(() => cancellationCompletionSrc.TrySetResult(true)))
                    {
                        if (connecting == await Task.WhenAny(connecting, cancellationCompletionSrc.Task))
                        {
                            await connecting;
                            return new Client(tcpClient, localonly);
                        }
                        else
                        {
                            // Close the connect attempt when the timeout happens before the attempt succeed.

                            // Overkill with both Close() and Dispose()
                            using (tcpClient)
                            {
                                tcpClient.Close();
                            }
                            throw new TimeoutException("Connection attempt has timed out. Cannot connect to the destination. Are you sure the destination is correct? Is the host's proxy server running?");
                        }
                    }
                }
            });
        }

        private UdpTunnel gameTunnel;
        private TcpClient _client;
        private TaskCompletionSource<Client> taskSrc;

        private Client(TcpClient tcpClient, bool localonly)
        {
            var gameProxy = new UdpProxy();
            this.taskSrc = new TaskCompletionSource<Client>();
            this._client = tcpClient;
            Task.Factory.StartNew(async (obj) =>
            {
                using (var client = (TcpClient)obj)
                using (var broadcaster = new UdpClient(new IPEndPoint(localonly ? IPAddress.Loopback : IPAddress.Any, 0)))
                using (var networkStream = client.GetStream())
                {
                    networkStream.ReadTimeout = 5000;
                    networkStream.WriteTimeout = 5000;
                    broadcaster.Connect(IPAddress.Broadcast.ToString(), AmongUs.BroadcastPort);

                    // 4096 bytes should be enough??
                    byte[] buffer = new byte[4096];
                    byte[] broadcastMessage = null;

                    await networkStream.WriteAsync(Host.BroadcastHandshake, 0, Host.BroadcastHandshake.Length);
                    int readLen = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    if (readLen == sizeof(int) && (BitConverter.ToInt32(buffer, 0) == (int)MessageID.OK))
                    {
                        var destination = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
                        // this.gameTunnel = new UdpTunnel(new IPEndPoint(localonly ? IPAddress.Loopback : IPAddress.Any, AmongUs.ServerPort), destination);
                        try
                        {
                            _ = gameProxy.Start(destination.Address.ToString(), (ushort)destination.Port, AmongUs.ServerPort, (localonly ? IPAddress.Loopback : IPAddress.Any).ToString());
                            // _ = gameTunnel.Start();
                            while (this._client.Connected)
                            {
                                buffer.WriteBytes((int)MessageID.Broadcast, 0);
                                await networkStream.WriteAsync(buffer, 0, sizeof(int));
                                readLen = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                                var strCount = readLen - sizeof(ushort);
                                var playerCount = buffer.ReadUInt16(strCount);

                                var extendedPart = $" (Proxy)~Open~{playerCount}~";
                                var byteCountOfExtendedPart = Encoding.UTF8.GetByteCount(extendedPart);
                                var byteCountOfMessage = 2 + strCount + byteCountOfExtendedPart;

                                // Reuse allocated buffer to avoid unnecessary allocations.
                                // Only create anew in case it has not been created yet or the buffer's size is not enough
                                if (broadcastMessage == null || broadcastMessage.Length < byteCountOfMessage)
                                {
                                    broadcastMessage = new byte[2 + strCount + byteCountOfExtendedPart];
                                    broadcastMessage[0] = 4;
                                    broadcastMessage[1] = 2;
                                }

                                // Copy game name.
                                Buffer.BlockCopy(buffer, 0, broadcastMessage, 2, strCount);

                                // Encode the extended part into the buffer
                                Encoding.UTF8.GetBytes(extendedPart, 0, extendedPart.Length, broadcastMessage, strCount + 2);

                                broadcaster.Send(broadcastMessage, byteCountOfMessage);

                                await Task.Delay(100);
                            }
                        }
                        catch (Exception ex) when (!(ex is ObjectDisposedException))
                        {
                            this.taskSrc.TrySetException(ex);
                        }
                        finally
                        {
                            gameProxy.Stop();
                            // gameTunnel.Stop();
                        }
                        this.taskSrc.TrySetResult(this);
                    }

                    broadcaster.Close();
                    client.Close();
                    this.taskSrc.TrySetCanceled();
                }
            }, tcpClient, TaskCreationOptions.LongRunning);
        }

        public Task<Client> WhenConnectionTerminated() => this.taskSrc.Task;

        public void Dispose()
        {
            if (this._client != null)
            {
                this._client.Close();
            }
        }
    }
}
