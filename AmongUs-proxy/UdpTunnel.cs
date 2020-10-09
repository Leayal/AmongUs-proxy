using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AmongUs_proxy
{
    class UdpTunnel : IDisposable
    {
        private IPEndPoint _from, _to;
        private bool _isRunning;
        private UdpClient m_UdpSendSocket = null, m_UdpListenSocket = null;

        public UdpTunnel(IPEndPoint from, IPEndPoint to)
        {
            this._from = from;
            this._to = to;
            this._isRunning = false;
        }

        public void Start()
        {
            if (this._isRunning) return;
            this._isRunning = true;
            this.CreateTunnelAndRun();
        }

        private void CreateTunnelAndRun()
        {
            Task.Factory.StartNew(async () =>
            {
                IPEndPoint m_connectedClientEp = null;

                try
                {
                    m_UdpListenSocket = new UdpClient(this._from);
                    // m_UdpListenSocket = new Socket(this._from.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    // m_UdpListenSocket.Bind(this._from);

                    //Connect to zone IP EndPoint
                    m_connectedClientEp = this._from;

                    while (this._isRunning)
                    {
                        if (m_UdpListenSocket.Available > 0)
                        {
                            var result = await m_UdpListenSocket.ReceiveAsync();
                            // var result = await m_UdpListenSocket.ReceiveFromAsync(segment1, SocketFlags.None, m_connectedClientEp); //client to listener
                            // result.ReceivedBytes
                            // result.RemoteEndPoint
                            m_connectedClientEp = result.RemoteEndPoint;

                            if (m_UdpSendSocket == null)
                            {
                                // Connect to UDP Game Server.
                                // m_UdpSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                m_UdpSendSocket = new UdpClient();
                            }
                            await m_UdpSendSocket.SendAsync(result.Buffer, result.Buffer.Length, this._to); //listener to server.

                        }

                        if (m_UdpSendSocket != null && m_UdpSendSocket.Available > 0)
                        {
                            var result = await m_UdpSendSocket.ReceiveAsync(); //server to client.
                            await m_UdpListenSocket.SendAsync(result.Buffer, result.Buffer.Length, m_connectedClientEp);
                            // m_UdpListenSocket.SendTo(buffer2, size, SocketFlags.None, m_connectedClientEp); //listner

                        }
                    }
                }
                finally
                {
                    if (m_UdpSendSocket != null)
                    {
                        m_UdpSendSocket.Close();
                        m_UdpSendSocket.Dispose();
                    }
                    if (m_UdpListenSocket != null)
                    {
                        m_UdpListenSocket.Close();
                        m_UdpListenSocket.Dispose();
                    }
                }
            });
        }

        public void Stop()
        {
            if (!this._isRunning) return;
            this._isRunning = false;
            if (m_UdpSendSocket != null)
            {
                m_UdpSendSocket.Close();
                m_UdpSendSocket.Dispose();
            }
            if (m_UdpListenSocket != null)
            {
                m_UdpListenSocket.Close();
                m_UdpListenSocket.Dispose();
            }
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
