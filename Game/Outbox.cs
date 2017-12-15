using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class CP_Outbox : Handler
    {
        internal enum SubCodes
        {
            Activate = 1118,
            Delete = 1119
        }

        public override void Handle(User usr)
        {
            SubCodes subcode = (SubCodes)int.Parse(getBlock(0));
            int outboxId = int.Parse(getBlock(1));

            switch (subcode)
            {
                case SubCodes.Delete:
                    {
                        if (usr.OutboxItems.Count <= 0) return;
                        string itemCode = getBlock(4);

                        if (Inventory.HasOutboxItem(usr, itemCode))
                        {
                            Inventory.RemoveOutBoxItem(usr, outboxId);
                            usr.send(new SP_Outbox(usr));
                        }
                        break;
                    }
                case SubCodes.Activate:
                    {
                        if (usr.OutboxItems.Count <= 0) return;
                        string itemCode = getBlock(4);

                        if (Inventory.HasOutboxItem(usr, itemCode))
                        {
                            bool found = usr.OutboxItems.Values.Where(r => r.id == outboxId).Count() > 0;
                            if (found)
                            {
                                OutboxItem i = usr.OutboxItems.Values.Where(r => r.id == outboxId).FirstOrDefault();
                                int days = i.days;
                                if (Inventory.GetFreeItemSlotCount(usr) > 0)
                                {
                                    Managers.Item item = Managers.ItemManager.GetItem(i.itemcode);
                                    if (item != null)
                                    {
                                        Inventory.PerformAddItem(usr, itemCode, days, i.count);
                                        Inventory.RemoveOutBoxItem(usr, outboxId);
                                        usr.send(new SP_OutboxUse(usr, itemCode));
                                        usr.send(new SP_Outbox(usr));
                                    }
                                }
                                else
                                {
                                    usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.InventoryFull));
                                }
                            }
                        }
                        break;
                    }
            }
        }
    }

    class SP_Outbox : Packet
    {
        public SP_Outbox(User usr)
        {
            newPacket(30752);
            addBlock(1117);
            addBlock(1);
            addBlock(usr.dinar);
            addBlock(0);
            addBlock(usr.cash);
            addBlock("LIST");
            addBlock(usr.OutboxItems.Count);
            var sortedDictionary = usr.OutboxItems.Values.OrderByDescending(i => i.timestamp).ToList();
            foreach (var i in sortedDictionary)
            {
                addBlock(i.id);
                addBlock(usr.userId);
                addBlock(i.itemcode);
                addBlock((i.count > 1 ? i.count : i.days));
                addBlock("NULL");
                addBlock("NULL");
                addBlock(usr.nickname);
                addBlock(0);
            }
            addBlock(0);
            addBlock(1);
            addBlock(0);
            addBlock(0);
        }
    }

    class SP_OutboxSend : Packet
    {
        public SP_OutboxSend(User usr)
        {
            newPacket(30752);
            addBlock(1117);
            addBlock(1);
            addBlock(usr.dinar);
            addBlock(0);
            addBlock(usr.cash);
            addBlock("LIST");
            addBlock(usr.OutboxItems.Count);
            var sortedDictionary = usr.OutboxItems.OrderByDescending(i => i.Key).ToList();
            foreach (var i in sortedDictionary)
            {
                OutboxItem Item = i.Value;
                addBlock(Item.id);
                addBlock(usr.userId);
                addBlock(Item.itemcode);
                addBlock((Item.count > 1 ? Item.count : Item.days));
                addBlock("NULL");
                addBlock("NULL");
                addBlock(usr.nickname);
                addBlock(0);
            }
            addBlock(1);
            addBlock(Inventory.Itemlist(usr));
            addBlock(usr.AvailableSlots); //Slots Enabled
            addBlock(Inventory.Costumelist(usr));
            addBlock(0);
            addBlock(1);
        }
    }

    class SP_OutboxUse : Packet
    {
        public SP_OutboxUse(User usr, string itemcode)
        {            
            //30752 1118 1 46628 0 0 CB09 0 1 DB33-3-0-13080422-0,DA03-1-1-13071419-0,CC02-3-0-13080422-0,DS01-3-0-13080903-0,CA01-3-0-13081400-0,CD01-3-0-13080422-0,CD02-3-0-13080422-0,DB04-1-0-13070914-0,CB09-2-2-13070719-6,DV01-3-0-13080620-0,DT01-1-0-13071700-0,DI05-1-0-13071720-0,DH01-1-0-13071921-0,DF12-3-0-13071420-0,CF02-3-0-13072602-0,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^ T,T,F,F BA10-3-0-14032700-0-0,BA08-3-0-14070720-0-0,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^ 1 0 
            newPacket(30752);
            addBlock(1118);
            addBlock(1);
            addBlock(usr.dinar);
            addBlock(0);
            addBlock(usr.cash);
            addBlock(itemcode);
            addBlock(0);
            addBlock(1);
            addBlock(Inventory.Itemlist(usr));
            addBlock(usr.AvailableSlots); //Slots Enabled
            addBlock(Inventory.Costumelist(usr));
        }
    }
}
