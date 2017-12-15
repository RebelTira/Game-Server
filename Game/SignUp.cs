using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_Signup : Packet
    {
        public SP_Signup(User usr)
        {
            newPacket(30777);
            addBlock(1);
            addBlock(Inventory.Itemlist(usr));
            addBlock(Inventory.Costumelist(usr));
        }
    }
}
