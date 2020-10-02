using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace AmongUs_proxy
{
    public sealed class Connection : IDisposable
    {
        private CancellationTokenSource cancelSrc;
        private TaskCompletionSource<Connection> task_ReceivingBroadcast;
        internal static readonly byte[] BroadcastHandshake;

        static Connection()
        {
            var signature = Encoding.ASCII.GetBytes("leayal-amongus-proxy");
            BroadcastHandshake = new byte[signature.Length + 1];
            Buffer.BlockCopy(signature, 0, BroadcastHandshake, 0, signature.Length);
            // Yes, last byte is \0
        }

        internal Connection(TcpClient tcpClient)
        {
            this.cancelSrc = new CancellationTokenSource();
            this.task_ReceivingBroadcast = new TaskCompletionSource<Connection>();
            Task.Factory.StartNew(async (obj) =>
            {
                using (var client = (TcpClient)obj)
                using (var broadcaster = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0)))
                using (var networkStream = client.GetStream())
                {
                    broadcaster.Connect(IPAddress.Broadcast.ToString(), 47777);
                    
                    // 4096 bytes should be enough??
                    byte[] buffer = new byte[4096];
                    using (var bufferStream = new MemoryStream())
                    {
                        await networkStream.WriteAsync(BroadcastHandshake, 0, BroadcastHandshake.Length);
                        int readLen = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                        if (readLen == sizeof(int) && (BitConverter.ToInt32(buffer, 0) == (int)MessageID.OK))
                        {
                            var destination = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
                            var gameTunnel = new UdpTunnel(new IPEndPoint(IPAddress.Loopback, AmongUs.ServerPort), destination);
                            try
                            {
                                gameTunnel.Start();
                                while (!this.cancelSrc.IsCancellationRequested)
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
                            finally
                            {
                                gameTunnel.Stop();
                            }
                        }
                    }
                    broadcaster.Close();
                    client.Close();
                }
                this.task_ReceivingBroadcast.TrySetResult(this);
            }, tcpClient, this.cancelSrc.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current ?? TaskScheduler.Default);
        }

        public Task UntilTermination()
        {
            return this.task_ReceivingBroadcast.Task;
        }

        public void Dispose()
        {
            this.task_ReceivingBroadcast.TrySetCanceled();
        }
    }
}
