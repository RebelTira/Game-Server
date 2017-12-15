using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_UpdateInventory : Packet
    {
        public SP_UpdateInventory(User usr, List<string> items)
        {
            newPacket(30976);
            addBlock(1);
            addBlock(usr.AvailableSlots);

            for (int i = 0; i < 5; i++)
            {
                addBlock(usr.GetEquipment(i));
            }

            addBlock(Inventory.Itemlist(usr));
            addBlock(items != null ? items.Count : 0);
            if (items != null)
            {
                foreach (string var in items)
                {
                    addBlock(var);
                }
            }
        }
    }

    class SP_UpdateChristmasEquipment : Packet
    {
        public SP_UpdateChristmasEquipment(User usr, List<string> items)
        {
            newPacket(30976);
            addBlock(1);
            addBlock(usr.AvailableSlots);

            string equipment = "D201," + (usr.premium >= 3 ? "D204" : "^") + ",^,^,^,^,^,^"; //>--- Snowball,  Big Snowball

            for (int i = 0; i < 5; i++)
            {
                addBlock(equipment);
            }

            addBlock(Inventory.Itemlist(usr));
            addBlock(items.Count);
            foreach (string var in items)
            {
                addBlock(var);
            }
        }
    }
}
