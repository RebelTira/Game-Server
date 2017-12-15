using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_CustomCRCCheck : Packet
    {
        public SP_CustomCRCCheck()
        {
            newPacket(45820);
            addBlock(122);
        }
    }
}
