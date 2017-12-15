using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Managers
{
    class PackageManager
    {
        public static bool AddItem(User usr, string itemCode)
        {
            if (itemCode == "CC36" || itemCode == "CC37" || itemCode == "CC56" || itemCode == "CC57") return false;
            string[] blacklist = new string[] { "CZ99" };
            Item item = ItemManager.GetItem(itemCode);
            if (item != null)
            {
                switch(itemCode)
                {
                    case "CC41": usr.AddPremium(3, 30); break;
                    case "CC44": usr.AddPremium(2, 30); break;
                }

                if (item.dinarReward > 0)
                {
                    usr.dinar += (int)item.dinarReward;
                    DB.RunQuery("UPDATE users SET dinar = '" + usr.dinar + "' WHERE id='" + usr.userId + "'");
                }

                if (item.packageItems.Count > 0 && Array.IndexOf(blacklist, item.Code) == -1)
                {
                    if (itemCode.StartsWith("B") && Inventory.GetFreeCostumeSlotCount(usr) < item.packageItems.Count || !itemCode.StartsWith("B") && Inventory.GetFreeItemSlotCount(usr) < item.packageItems.Count) return false;

                    foreach (PackageItem pItem in item.packageItems)
                    {
                        if (Inventory.GetFreeItemSlotCount(usr) > 0)
                        {
                            if (pItem.item.StartsWith("B"))
                            {
                                Inventory.AddCostume(usr, pItem.item, pItem.days);
                            }
                            else
                            {
                                Inventory.AddItem(usr, pItem.item, pItem.days);
                            }
                        }
                        else
                        {
                            DB.RunQuery("INSERT INTO inbox (ownerid, itemcode, days) VALUES ('" + usr.userId + "', '" + pItem.item + "', '" + pItem.days+ "')");
                        }
                    }
                    return true;
                }
            }

            return false;
        }
    }
}
