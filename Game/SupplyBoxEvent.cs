using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_SupplyBoxEvent : Packet
    {
        public SP_SupplyBoxEvent(User usr)
        {
            //32258 18 17 0 2
            newPacket(32258);
            addBlock(18);
            addBlock(17);
            addBlock(0);
            addBlock(2);
        }
    }
}
