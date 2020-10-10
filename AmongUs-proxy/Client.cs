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
        public static Task<Client> Connect(string remoteHost, int remotePort) => Connect(remoteHost, remotePort, TimeSpan.FromSeconds(5));

        public static Task<Client> Connect(string remoteHost, int remotePort, TimeSpan timeOut)
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
                            return new Client(tcpClient);
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

        private Client(TcpClient tcpClient)
        {
            this.taskSrc = new TaskCompletionSource<Client>();
            this._client = tcpClient;
            Task.Factory.StartNew(async (obj) =>
            {
                using (var client = (TcpClient)obj)
                using (var broadcaster = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0)))
                using (var networkStream = client.GetStream())
                {
                    networkStream.ReadTimeout = 5000;
                    networkStream.WriteTimeout = 5000;
                    broadcaster.Connect(IPAddress.Broadcast.ToString(), 47777);

                    // 4096 bytes should be enough??
                    byte[] buffer = new byte[4096];
                    using (var bufferStream = new MemoryStream())
                    {
                        await networkStream.WriteAsync(Host.BroadcastHandshake, 0, Host.BroadcastHandshake.Length);
                        int readLen = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                        if (readLen == sizeof(int) && (BitConverter.ToInt32(buffer, 0) == (int)MessageID.OK))
                        {
                            var destination = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
                            this.gameTunnel = new UdpTunnel(new IPEndPoint(IPAddress.Loopback, AmongUs.ServerPort), destination);
                            try
                            {
                                gameTunnel.Start();
                                while (this._client.Connected)
                                {
                                    buffer.WriteBytes((int)MessageID.Broadcast, 0);
                                    await networkStream.WriteAsync(buffer, 0, sizeof(int));
                                    readLen = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                                    var strCount = readLen - sizeof(ushort);
                                    var playerCount = buffer.ReadUInt16(strCount);
                                    bufferStream.Position = 0;
                                    bufferStream.SetLength(0);
                                    using (var bw = new BinaryWriter(bufferStream, Encoding.UTF8, true))
                                    {
                                        bw.Write((byte)4);
                                        bw.Write((byte)2);
                                        bw.Write(Encoding.UTF8.GetBytes($"{Encoding.UTF8.GetString(buffer, 0, strCount)} (Proxy)"));
                                        bw.Write(Encoding.UTF8.GetBytes($"~Open~{playerCount}~"));
                                        bw.Flush();
                                        byte[] broadcastMessage = bufferStream.ToArray();
                                        broadcaster.Send(broadcastMessage, (int)broadcastMessage.Length);
                                    }
                                    await Task.Delay(100);
                                }
                            }
                            catch (Exception ex) when (!(ex is ObjectDisposedException))
                            {
                                this.taskSrc.TrySetException(ex);
                            }
                            finally
                            {
                                gameTunnel.Stop();
                            }
                            this.taskSrc.TrySetResult(this);
                        }
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
