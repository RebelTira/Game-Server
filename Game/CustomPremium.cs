using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_CustomPremium : Packet
    {
        public SP_CustomPremium(int premiumId)
        {
            newPacket(25023);
            addBlock(0);
            addBlock(premiumId);
        }
    }
}
