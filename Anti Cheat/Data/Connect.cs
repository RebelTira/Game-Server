using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Anti_Cheat.Data
{
    class SP_Connect : Structure.Packet
    {
        public SP_Connect(Client usr)
        {
            newPacket(10040);
            addBlock((int)Math.Ceiling((double)(usr.sessionId * 3.35)) * Configs.Server.ClientVersion);
        }
    }
}
