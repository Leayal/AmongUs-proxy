using System;
using System.Collections.Generic;
using System.Text;

namespace AmongUs_proxy
{
    enum MessageID : int
    {
        Handshake = 0,
        OK,
        Broadcast,
        GamePacket
    }
}
