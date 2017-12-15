using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class LevelUPItem
    {
        public string Code;
        public int Days;
        public LevelUPItem(string _Code, int _Days)
        {
            Code = _Code;
            Days = _Days;
        }
    }

    class SP_LevelUp : Packet
    {
        public SP_LevelUp(User usr, int Dinar, List<LevelUPItem> Items)
        {
            //31008 1 0 1 2336 2500
            //31008 2 2340 0 10000 T,F,F,F CA01-3-0-13071814-0,DA03-1-0-13071813-0,DB08-1-0-13071813-0,DC06-1-0-13071813-0,DF04-1-0-13071813-0,CB08-2-0-13071114-1,DC03-1-1-13071815-0,DJ03-1-1-13071815-0,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^ ^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^ 0
            //31008 13 94784 2  DC04 7 CA01 7 12000 T,F,F,F DC04-1-0-13081613-0,CA01-3-0-13081612-0,CB08-2-0-13051515-2,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^ ^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^ 1 
            // Chapter 1

            /*newPacket(33280);
            addBlock(usr.RoomSlot);
            addBlock(0);
            addBlock(Managers.LevelCalculator.getExpForLevelusr.EXP));
            addBlock(usr.EXP);
            addBlock(Dinar);*/

            // Chapter 3

            newPacket(31008);
            addBlock(usr.roomslot);
            addBlock(usr.exp);
            addBlock(Items.Count);
            foreach (LevelUPItem Item in Items)
            {
                addBlock(Item.Code);
                addBlock(Item.Days);
            }
            addBlock(Dinar);
            addBlock(usr.AvailableSlots);
            addBlock(Inventory.Itemlist(usr));
            addBlock(Inventory.Costumelist(usr));
            addBlock(0);
        }
    }
}
