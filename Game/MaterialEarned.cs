using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_MaterialEarned : Packet
    {
        public SP_MaterialEarned(User usr, int type)
        {
            newPacket(30996);
            addBlock(type);
            addBlock(Inventory.Itemlist(usr));
        }
    }
}
