using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmongUs_proxy.GUI
{
    enum UIState
    {
        None = 0,
        HostReady,
        HostStarting,
        HostStarted,
        HostStopping,
        ClientReady,
        ClientConnecting,
        ClientConnected,
        ClientDisconnecting
    }
}
