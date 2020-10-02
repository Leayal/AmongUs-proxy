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
        private CancellationTokenSource cancelSrc;
        
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
            if (this.cancelSrc != null)
            {
                this.cancelSrc.Dispose();
            }
            this.cancelSrc = new CancellationTokenSource();
            this.CreateTunnelAndRun();
        }

        private void CreateTunnelAndRun()
        {
            Task.Factory.StartNew(async () =>
            {
                var token = this.cancelSrc.Token;
                token.Register(this.cancelSrc.Cancel);
                EndPoint m_connectedClientEp = null;
                Socket m_UdpSendSocket = null, m_UdpListenSocket = null;

                try
                {
                    m_UdpListenSocket = new Socket(this._from.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    m_UdpListenSocket.Bind(this._from);

                    //Connect to zone IP EndPoint
                    m_connectedClientEp = this._from;

                    // hardcoded buffer size
                    byte[] buffer_in = new byte[1024 * 8],
                        buffer_out = new byte[1024 * 8];

                    while (this._isRunning)
                    {
                        if (m_UdpListenSocket.Available > 0)
                        {
                            int size = await m_UdpListenSocket.ReceiveFromAsync(buffer_in, ref m_connectedClientEp, token); //client to listener

                            if (m_UdpSendSocket == null)
                            {
                                // Connect to UDP Game Server.
                                m_UdpSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                            }

                            await m_UdpSendSocket.SendToAsync(buffer_in, 0, size, SocketFlags.None, this._to, token); //listener to server.

                        }

                        if (m_UdpSendSocket != null && m_UdpSendSocket.Available > 0)
                        {
                            int size = await m_UdpSendSocket.ReceiveAsync(buffer_out, token); //server to client.

                            await m_UdpListenSocket.SendToAsync(buffer_out, 0, size, SocketFlags.None, m_connectedClientEp, token); //listner

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
            this.cancelSrc.Cancel();
        }

        public void Dispose()
        {
            if (this._isRunning)
            {
                this.cancelSrc.Cancel();
            }
            else if (this.cancelSrc != null)
            {
                this.cancelSrc.Dispose();
            }
        }
    }
}
