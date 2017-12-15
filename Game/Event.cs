using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_ExpEvent : Packet
    {
        internal enum EventCodes
        {
            EXP_Activate = 810,
            EXP_Deactivate = 820
        }

        public SP_ExpEvent(EventCodes eCode)
        {
            newPacket(25344);
            addBlock((int)eCode);
            addBlock(0);
        }
    }

    class SP_Event : Packet
    {
        internal enum ErrorCodes
        {
            InventoryFull = 97070,
            ItemNotAvailable = -1
        }

        public SP_Event(User usr, string ItemCode, int Days)
        {
            //30977 1 BA01,^,^,^,^,I003,^,^,^,^,^,^,^,^,^,^,^,^,I000,^,^,^,^,^,^ BA02,^,^,^,^,I004,^,^,^,^,^,^,^,^,^,^,^,^,I001,^,^,^,^,^,^ BA03,^,^,^,^,I005,^,^,^,^,^,^,^,^,^,^,^,^,I000,^,^,^,^,^,^ BA04,^,^,^,^,I006,^,^,^,^,^,^,^,^,^,^,^,^,I000,^,^,^,^,^,^ BA05,^,^,^,^,I007,^,^,^,^,^,^,^,^,^,^,^,^,I000,^,^,^,^,^,^ BS04-3-0-33020719-0-0,BS06-3-0-33020719-0-0,BE04-3-0-33020720-0-0,BF18-3-0-33020720-0-0,BF19-3-0-33020720-0-0,BF20-3-0-33020720-0-0,BF21-3-0-33020720-0-0,BF22-3-0-33020720-0-0,BF23-3-0-33020720-0-0,BF24-3-0-33020720-0-0,BF25-3-0-33020720-0-0,BF26-3-0-33020720-0-0,BF27-3-0-33020720-0-0,BF28-3-0-33020720-0-0,BF29-3-0-33020720-0-0,BF30-3-0-33020720-0-0,BF31-3-0-33020720-0-0,BF32-3-0-33020720-0-0,BS0E-3-0-13101822-0-0,BA19-3-0-13101616-9999-9999,BA24-3-0-13101616-9999-9999,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^ 0 1005 BA24 1 20 
            newPacket(30977);
            addBlock(1);
            for (int i = 0; i < 5; i++)
            {
                addBlock(usr.costumes_char[i]);
            }
            addBlock(Inventory.Itemlist(usr));
            addBlock(0);
            addBlock(1005);
            addBlock(ItemCode);
            addBlock(Days);
            addBlock(usr.eventcount);
        }
    }

    class SP_EventCount : Packet
    {
        public SP_EventCount(User usr)
        {
            newPacket(31120);
            addBlock(1);
            addBlock(usr.eventcount);
        }
    }
}