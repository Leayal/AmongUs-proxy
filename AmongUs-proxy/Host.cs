using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace AmongUs_proxy
{
    public sealed class Host
    {
        internal static readonly byte[] BroadcastHandshake;

        static Host()
        {
            var handshakeSignature = "leayal-amongus-proxy";
            var length = Encoding.ASCII.GetByteCount(handshakeSignature);
            BroadcastHandshake = new byte[length + 1];
            var encodedLen = Encoding.ASCII.GetBytes(handshakeSignature, 0, handshakeSignature.Length, BroadcastHandshake, 0);

            // Unlikely to be happen, but just to be safe
            if (encodedLen != (BroadcastHandshake.Length - 1))
            {
                var swapBuffer = new byte[encodedLen + 1];
                Buffer.BlockCopy(BroadcastHandshake, 0, swapBuffer, 0, encodedLen);
                BroadcastHandshake = swapBuffer;
            }
            // Yes, last byte is \0
        }

        private UdpProxy gameTunnel;
        private bool _isRunning;
        private TcpListener broadcastRelay;
        private UdpClient broadcastListener;

        /// <summary>
        /// [4][2][${RoomName}][~Open~${user_count}~]
        /// </summary>
        private string _game_name;
        private long _player_count;

        public Host()
        {
            this._isRunning = false;
            this._game_name = string.Empty;
            this._player_count = 0;
            this.gameTunnel = new UdpProxy();
        }

        public string GameName
        {
            get => this._game_name;
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException();
                this._game_name = value;
            }
        }

        public void Start(string localIp, int localPort)
        {
            if (this._isRunning) return;
            if (!IPAddress.TryParse(localIp, out var validIp))
            {
                throw new ArgumentException("Provided IPAddress is not valid.", nameof(localIp));
            }
            else if (localPort < 0 || localPort > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(localPort), "Provided Port is not valid.");
            }
            this._isRunning = true;

            this.broadcastRelay = new TcpListener(validIp, localPort);
            this.broadcastRelay.Start();
            this.broadcastRelay.BeginAcceptTcpClient(this.AcceptingTcpClient, broadcastRelay);

            if (this.broadcastListener != null)
            {
                this.broadcastListener.Close();
                this.broadcastListener.Dispose();
            }

            this.broadcastListener = new UdpClient()
            {
                ExclusiveAddressUse = false
            };
            this.broadcastListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.broadcastListener.Client.Bind(new IPEndPoint(IPAddress.Any, AmongUs.BroadcastPort));
            this.broadcastListener.BeginReceive(this.ListeningForBroadcast, null);

            // this.gameTunnel = new UdpTunnel((IPEndPoint)broadcastRelay.LocalEndpoint, new IPEndPoint(Constants.LanIP, AmongUs.ServerPort));
            var bindGameServer = (IPEndPoint)broadcastRelay.LocalEndpoint;
            _ = this.gameTunnel.Start(Constants.LanIP.ToString(), AmongUs.ServerPort, (ushort)bindGameServer.Port, bindGameServer.Address.ToString());
        }

        private async void ListeningForBroadcast(IAsyncResult ar)
        {
            if (this.broadcastListener == null)
            {
                // Object disposed;
                return;
            }
            try
            {
                var receivedEnd = new IPEndPoint(IPAddress.Any, AmongUs.BroadcastPort);
                var buffer = this.broadcastListener.EndReceive(ar, ref receivedEnd);
                if (buffer.Length == 0)
                {
                    if (this.broadcastListener != null)
                    {
                        this.broadcastListener.Close();
                        this.broadcastListener.Dispose();
                        this.broadcastListener = null;
                    }
                    return;
                }
                var str = Encoding.UTF8.GetString(buffer, 2, buffer.Length - 2);
                if (str.Length != 0 && str[str.Length - 1] == '~')
                {
                    int index = str.LastIndexOf("~open~", StringComparison.OrdinalIgnoreCase);
                    if (index != -1)
                    {
                        index += 6;
                        var taaa = str.Substring(index, str.Length - 1 - index);
                        if (int.TryParse(str.Substring(index, str.Length - 1 - index), out var playerCount))
                        {
                            Interlocked.Exchange(ref this._player_count, playerCount);
                        }
                    }
                }
                await Task.Delay(100);
                this.broadcastListener.BeginReceive(this.ListeningForBroadcast, null);
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException)
            {
                if (this.broadcastListener != null)
                {
                    this.broadcastListener.Close();
                    this.broadcastListener.Dispose();
                    this.broadcastListener = null;
                }
            }
            catch (SocketException)
            {
                if (this.broadcastListener != null)
                {
                    this.broadcastListener.Close();
                    this.broadcastListener.Dispose();
                    this.broadcastListener = null;
                }
            }
            // Other exception won't be handled for error message.
        }

        private void AcceptingTcpClient(IAsyncResult ar)
        {
            var listener = (TcpListener)ar.AsyncState;
            TcpClient tcpClient;
            try
            {
                tcpClient = listener.EndAcceptTcpClient(ar);
            }
            catch (ObjectDisposedException)
            {
                // listener.Stop();
                return;
            }
            this.HandleIncomingHandshake(listener, tcpClient);
            listener.BeginAcceptTcpClient(this.AcceptingTcpClient, listener);
        }

        private void HandleIncomingHandshake(TcpListener server, TcpClient client)
        {
            Task.Factory.StartNew(async (obj) =>
            {
                using (var tcpClient = (TcpClient)obj)
                {
                    var buffer = new byte[4096];
                    using (var networkStream = tcpClient.GetStream())
                    {
                        try
                        {
                            int read = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                            var handshakeMessage = new byte[read];
                            Buffer.BlockCopy(buffer, 0, handshakeMessage, 0, read);
                            if (!BroadcastHandshake.UnsafeCompare(handshakeMessage))
                            {
                                return;
                            }
                            buffer.WriteBytes((int)MessageID.OK, 0);
                            await networkStream.WriteAsync(buffer, 0, sizeof(int));

                            while (this._isRunning)
                            {
                                read = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                                if (read == sizeof(uint))
                                {
                                    switch ((MessageID)buffer.ReadUInt16(0))
                                    {
                                        case MessageID.Handshake:
                                            break;
                                        case MessageID.Broadcast:
                                            using (var mem = new MemoryStream(buffer))
                                            using (var bw = new BinaryWriter(mem))
                                            {
                                                mem.Position = 0;
                                                mem.SetLength(0);
                                                bw.Write(Encoding.UTF8.GetBytes(this._game_name));
                                                bw.Write((ushort)(Interlocked.Read(ref this._player_count)));
                                                bw.Flush();
                                                await networkStream.WriteAsync(buffer, 0, (int)mem.Length);
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            tcpClient.Close();
                        }
                    }
                }
            }, client, TaskCreationOptions.LongRunning);
        }
        
        public void Stop()
        {
            if (!this._isRunning) return;
            this._isRunning = false;
            this.gameTunnel.Stop();
            this.broadcastRelay.Stop();

            if (this.broadcastListener != null)
            {
                this.broadcastListener.Close();
                this.broadcastListener.Dispose();
                this.broadcastListener = null;
            }
        }
    }
}
