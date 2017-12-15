using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_DeleteItem : Packet
    {
        public SP_DeleteItem(User usr, string itemCode)
        {
            newPacket(30224);
            addBlock(1);
            addBlock(itemCode);

            // Build Inventory //

            addBlock(Inventory.Itemlist(usr));
            addBlock(usr.AvailableSlots); //Slots Enabled

            for (int i = 0; i < 5; i++)
            {
                addBlock(usr.GetEquipment(i));
            }
        }
    }

    class CP_DeleteItem : Handler
    {
        public override void Handle(User usr)
        {
            if (usr.room != null) return;
            string item = getBlock(0);

            if (usr.HasItem(item))
            {
                int index = usr.GetItemIndex(item);
                if (index != -1)
                {
                    usr.deleteItem(item);
                    string calculatedItem = Inventory.calculateInventory(index);
                    for (int I = 0; I < 5; I++)
                    {
                        for (int J = 0; J < 8; J++)
                        {
                            if (usr.equipment[I, J] == calculatedItem || usr.equipment[I, J] == item)
                            {
                                usr.equipment[I, J] = "^";
                            }
                        }
                    }
                    usr.LoadRetails();
                    usr.send(new SP_DeleteItem(usr, item));
                }
            }
            else
            {
                Log.WriteError(usr.nickname + " tried to delete " + item + " but he haven't it!");
            }
        }
    }
    
    class SP_DeleteCostume : Packet
    {
        public SP_DeleteCostume(User usr, string itemCode)
        {
            newPacket(30225);
            addBlock(1);
            addBlock(itemCode);

            // Build Inventory //

            addBlock(Inventory.Costumelist(usr));

            for (int i = 0; i < 5; i++)
            {
                addBlock(usr.costume[i]);
            }
        }
    }

    class CP_DeleteCostume : Handler
    {
        public override void Handle(User usr)
        {
            if (usr.room != null) return;
            string item = getBlock(0);

            if (item.ToUpper() == "BA01" || item.ToUpper() == "BA02" || item.ToUpper() == "BA03" || item.ToUpper() == "BA04" || item.ToUpper() == "BA05")
            {
                usr.send(new SP_CostumeEquip(SP_CostumeEquip.ErrCode.CannotDeleteDefaultItem));
            }
            else
            {
                if (usr.HasCostume(item))
                {
                    usr.deleteCostume(item);
                    usr.CheckForCostume();
                    usr.send(new SP_DeleteCostume(usr, item));
                    usr.send(new SP_CashItemBuy(usr));
                }
                else
                {
                    Log.WriteError(usr.nickname + " tried to delete " + item + " but he haven't it!");
                }
            }
        }
    }
}
