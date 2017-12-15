using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_ChatEvent : Packet
    {
        public SP_ChatEvent(User usr, string code)
        {
            newPacket(30775);
            addBlock(1);
            addBlock(code);
            addBlock(0);
            addBlock(Inventory.Itemlist(usr));
        }
    }
}
