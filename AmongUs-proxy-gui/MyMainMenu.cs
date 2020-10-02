using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AmongUs_proxy;

namespace AmongUs_proxy.GUI
{
    public partial class MyMainMenu : Form
    {
        private static readonly Task<IPAddress[]> task_fetchingLANIP;
        static MyMainMenu()
        {

            task_fetchingLANIP = Task.Run<IPAddress[]>(() =>
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                var result = new List<IPAddress>(networkInterfaces.Length);
                // In case the LAN is in another network.
                foreach (var networkinterface in networkInterfaces)
                {
                    if (networkinterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet || networkinterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        if (networkinterface.Supports(NetworkInterfaceComponent.IPv4))
                        {
                            foreach (var ip in networkinterface.GetIPProperties().UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    result.Add(ip.Address);
                                }
                            }
                        }
                    }
                }
                return result.ToArray();
            });
        }

        private UIState _uistate;
        private Host hosting;
        private Connection client;

        public MyMainMenu()
        {
            this._uistate = UIState.None;
            this.hosting = null;
            this.client = null;
            InitializeComponent();
        }

        private UIState State
        {
            get => this._uistate;
            set
            {
                if (this._uistate != value)
                {
                    this._uistate = value;
                    switch (this._uistate)
                    {
                        case UIState.HostReady:
                            this.button1.Enabled = true;
                            this.groupBoxHost.Visible = true;
                            this.button5.Visible = true;
                            this.button5.Enabled = true;
                            this.button1.Text = "Start the proxy server";
                            break;
                        case UIState.HostStarting:
                            this.button1.Enabled = false;
                            this.button5.Enabled = false;
                            this.button1.Text = "Starting the proxy";
                            break;
                        case UIState.HostStarted:
                            this.button1.Enabled = true;
                            this.button5.Enabled = false;
                            this.button1.Text = "Stop the proxy server";
                            break;
                        case UIState.HostStopping:
                            this.button1.Enabled = false;
                            this.button5.Enabled = false;
                            this.button1.Text = "Stopping the proxy server";
                            break;

                        case UIState.None:
                            this.groupBoxHost.Visible = false;
                            this.groupBoxClient.Visible = false;
                            this.button5.Visible = false;
                            break;

                        case UIState.ClientReady:
                            this.button2.Enabled = true;
                            this.groupBoxClient.Visible = true;
                            this.button5.Visible = true;
                            this.button5.Enabled = true;
                            this.button2.Text = "Connect to the proxy server";
                            break;
                        case UIState.ClientConnecting:
                            this.button2.Enabled = false;
                            this.button5.Enabled = false;
                            this.button2.Text = "Connecting to the proxy server";
                            break;
                        case UIState.ClientConnected:
                            this.button2.Enabled = true;
                            this.button5.Enabled = false;
                            this.button2.Text = "Disconnect from the proxy server";
                            break;
                        case UIState.ClientDisconnecting:
                            this.button2.Enabled = false;
                            this.button5.Enabled = false;
                            this.button2.Text = "Disconnecting from the proxy server";
                            break;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (this._uistate)
            {
                case UIState.HostReady:
                    var bindHost = this.comboBox1.Text;
                    if (!IPAddress.TryParse(bindHost, out _))
                    {
                        MessageBox.Show(this, "Invalid IP Address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (this.hosting == null)
                    {
                        this.hosting = new Host();
                    }
                    var roomName = this.textBox2.Text;
                    if (roomName.Length == 0)
                    {
                        MessageBox.Show(this, "Room name cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else if (roomName.Length > 12)
                    {
                        if (MessageBox.Show(this, "The room name is longer than 12 characters.\nDo you still want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        {
                            return;
                        }
                    }
                    this.hosting.GameName = roomName;
                    this.State = UIState.HostStarting;
                    try
                    {
                        this.hosting.Start(bindHost, (int)this.numericUpDown1.Value).Dispose();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.State = UIState.HostReady;
                        return;
                    }
                    this.State = UIState.HostStarted;
                    break;
                case UIState.HostStarted:
                    if (this.hosting != null)
                    {
                        this.State = UIState.HostStopping;
                        this.hosting.Stop();
                        this.State = UIState.HostReady;
                    }
                    break;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            switch (this._uistate)
            {
                case UIState.ClientReady:
                    var text = this.textBox1.Text;
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        MessageBox.Show(this, "Invalid destination.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var portIndex = text.IndexOf(':');
                    if (portIndex != -1)
                    {
                        // Enforce port to be specified.
                        MessageBox.Show(this, "Please specify destination port.\nFormat: <host/IP>:<port number>.\nEx: 192.168.1.1:6969", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (!IPAddress.TryParse(text.Substring(0, portIndex), out var ipAddr))
                    {
                        MessageBox.Show(this, "Invalid IP Address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (!ushort.TryParse(text.Substring(portIndex + 1), out var portAddr) || portAddr == 0)
                    {
                        MessageBox.Show(this, "Invalid port number Address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    this.State = UIState.ClientConnecting;
                    Connection connection = null;
                    try
                    {
                        connection = await Client.Connect(ipAddr.ToString(), portAddr);
                    }
                    catch (Exception ex)
                    {
                        if (connection != null)
                        {
                            connection.Dispose();
                        }
                        MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.State = UIState.ClientReady;
                        return;
                    }
                    _ = connection.UntilTermination().ContinueWith((t) => connection.Dispose());
                    this.client = connection;
                    this.State = UIState.ClientConnected;
                    break;
                case UIState.ClientConnected:
                    if (this.client != null)
                    {
                        this.State = UIState.ClientDisconnecting;
                        this.client.Dispose();
                        this.State = UIState.ClientReady;
                    }
                    break;
            }
        }

        private async void MyMainMenu_Shown(object sender, EventArgs e)
        {
            var listofIPs = await task_fetchingLANIP;
            if (listofIPs.Length != 0)
            {
                comboBox1.Items.AddRange(listofIPs);
                if (comboBox1.Text.Length == 0)
                {
                    comboBox1.SelectedIndex = 0;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.State = UIState.HostReady;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.State = UIState.ClientReady;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.State = UIState.None;
        }
    }
}
