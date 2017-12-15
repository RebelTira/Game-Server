using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_CustomWeed : Packet
    {
        public SP_CustomWeed()
        {
            newPacket(24523);
            addBlock("WEED");
        }
    }
    class SP_CustomMLG : Packet
    {
        public SP_CustomMLG()
        {
            newPacket(24523);
            addBlock("MLG");
        }
    }
}
