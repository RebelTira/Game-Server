using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_RandomBoxEvent : Packet
    {
        public SP_RandomBoxEvent(User u, string item)
        {
            newPacket(21281);
            addBlock(0);
            addBlock(item);
            addBlock(Inventory.Itemlist(u));
            addBlock(Inventory.Costumelist(u));
        }
    }
}
