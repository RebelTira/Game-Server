/*
 _____   ___ __  __  _____ _____   ___  _  __              ___   ___   __    __ 
/__   \ /___\\ \/ /  \_   \\_   \ / __\( )/ _\            / __\ /___\ /__\  /__\
  / /\///  // \  /    / /\/ / /\// /   |/ \ \            / /   //  /// \// /_\  
 / /  / \_//  /  \ /\/ /_/\/ /_ / /___    _\ \          / /___/ \_/// _  \//__  
 \/   \___/  /_/\_\\____/\____/ \____/    \__/          \____/\___/ \/ \_/\__/  
__________________________________________________________________________________

Created by: ToXiiC
Thanks to: CodeDragon, Kill1212, CodeDragon

*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Game_Server.Managers;

namespace Game_Server
{
    class Inventory
    {
        private static int[] periodDays = new int[] { 1, 3, 7, 15, 30, 60, 365 };

        public static string Itemlist(User usr)
        {
            return string.Join(",", usr.inventory);
        }
        
        public static string Costumelist(User usr)
        {
            return string.Join(",", usr.costume);
        }

        public static string Storage(User usr)
        {
            return string.Join(",", usr.storageInventory);
        }
        

        public static string calculateInventory(int Id)
        {
            return string.Format("I{0:000}", Id);
        }

        public static void PerformAddItem(User usr, string itemcode, int days, int count = 1)
        {
            Managers.Item item = Managers.ItemManager.GetItem(itemcode);
            if (itemcode != null)
            {
                if ((item.accruable || item.BuyType == 4) && usr.HasItem(itemcode)) //>--- BuyType = 4 One Use
                {
                    Inventory.IncreaseEAItem(usr, itemcode, count);
                }
                else
                {
                    if (!Managers.PackageManager.AddItem(usr, itemcode))
                    {
                        if (itemcode.StartsWith("B"))
                        {
                            Inventory.AddCostume(usr, itemcode, days);
                        }
                        else
                        {
                            Inventory.AddItem(usr, itemcode, days);
                        }

                        if (count > 1 && (item.accruable || item.BuyType == 4))
                        {
                            Inventory.IncreaseEAItem(usr, itemcode, count - 1);
                        }
                    }
                }
            }
        }

        public static int GetDaysFromPeriod(int period)
        {
            if (periodDays[period] >= 0)
            {
                return periodDays[period];
            }
            return -1;
        }

        public static int GetFreeItemSlotCount(User usr)
        {
            return usr.inventory.Cast<string>().Where(r => r == "^").Count();
        }

        public static int GetFreeCostumeSlotCount(User usr)
        {
            return usr.costume.Cast<string>().Where(r => r == "^").Count();
        }

        public static bool AddOutBoxItem(User usr, string ItemCode, ushort Days, ushort count)
        {
            Item i = ItemManager.GetItem(ItemCode);
            if (i != null)
            {
                int ts = Generic.timestamp;

                DB.RunQueryNotAsync("INSERT INTO outbox (ownerid, itemcode, days, count, timestamp) VALUES ('" + usr.userId + "', '" + ItemCode + "', '" + Days + "', '" + count + "','" + ts + "')");

                DataTable dt = DB.RunReader("SELECT * FROM outbox WHERE ownerid='" + usr.userId + "' AND itemcode='" + ItemCode + "' AND days='" + Days + "' AND timestamp='" + ts + "' ORDER BY id DESC");
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    int id = int.Parse(row["id"].ToString());
                    if (!usr.OutboxItems.ContainsKey(id))
                    {
                        OutboxItem item = new OutboxItem(id, ItemCode, Days, ts, count);

                        usr.OutboxItems.TryAdd(id, item);
                    }
                    Log.WriteLine(usr.nickname + " -> Added outbox item " + i.Name + " (ID: " + id + ")");
                }
            }
            usr.send(new Game.SP_Outbox(usr));
            return true;
        }

        public static void RemoveOutBoxItem(User usr, int Id) // Works but if you remove try&catch it will kick you from server
        {
            if (usr.OutboxItems.ContainsKey(Id))
            {
                OutboxItem u;
                usr.OutboxItems.TryRemove(Id, out u);
                DB.RunQuery("DELETE FROM outbox WHERE id='" + Id + "' AND ownerid='" + usr.userId + "'");
            }
        }

        public static bool HasOutboxItem(User usr, string strCode)
        {
            foreach (OutboxItem II in usr.OutboxItems.Values)
            {
                if (string.Compare(II.itemcode, strCode, true) == 0) { return true; }
            }
            return false;
        }

        public static bool AddItem(User usr, string item, int days)
        {
            try
            {
                if (days == -1) days = 3600; //>--- Para 10 años

                int itemIndex = usr.GetItemIndex(item);

                if (itemIndex != -1)
                {
                    Item i = ItemManager.GetItem(item);
                    string[] itemData = usr.inventory[itemIndex].Split('-');
                    DateTime itemTime = DateTime.ParseExact(itemData[3], "yyMMddHH", null);
                    itemTime = itemTime.AddDays(days);
                    itemData[3] = String.Format("{0:yyMMddHH}", itemTime);
                    if (i != null)
                    {
                        Log.WriteInfo(">--- Inventory-162 user - item: " + item + " days: " + days);
                        if (i.accruable || i.BuyType == 4)
                        {
                            int count = 1;
                            int.TryParse(itemData[4], out count);
                            count++;
                            if (count >= i.maxAccrueCount) count = i.maxAccrueCount;
                            itemData[4] = count.ToString();
                        }
                    }
                    usr.inventory[itemIndex] = string.Join("-", itemData);
                    DB.RunQuery("UPDATE equipment SET inventory='" + Itemlist(usr) + "' WHERE ownerid='" + usr.userId + "'");
                    return true;
                }
                else
                {
                    int index = Array.IndexOf(usr.inventory, "^");

                    if (index != -1 && index <= Configs.Server.Player.MaxInventorySlot)
                    {
                        //CZ83-2-0-1312231600-41
                        if (usr.inventory[index] == "^")
                        {
                            Item i = ItemManager.GetItem(item);
                            int id = 0;
                            if (i != null)
                            {
                                if (i.accruable || i.BuyType == 4)
                                {
                                    id = 1;
                                    days = 3600;
                                }
                            }
                            DateTime current = DateTime.Now;
                            current = current.AddDays(days);
                            current = current.AddHours(-4);
                            usr.inventory[index] = item + "-1-1-" + String.Format("{0:yyMMddHH}", current) + "-" + id;
                            DB.RunQuery("UPDATE equipment SET inventory='" + Itemlist(usr) + "' WHERE ownerid='" + usr.userId + "'");
                            return true;
                        }
                    }
                    else
                    {
                        DB.RunQuery("INSERT INTO inbox (ownerid, itemcode, days) VALUES ('" + usr.userId + "', '" + item + "', '" + days + "')");
                        usr.send(new Game.SP_CustomMessageBox("The item you bought has been added to inbox.\nYour inventory is full, delete a item and re-login to get it."));
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteError("Error at AddItem: " + ex.Message + " - " + ex.StackTrace);
            }
            return false;
        }
        
        public static bool AddCostume(User usr, string item, int days)
        {
            try
            {
                if (days == -1) days = 3600;

                int itemIndex = usr.GetCostumeIndex(item);

                if (itemIndex != -1)
                {
                    Item i = ItemManager.GetItem(item);
                    string[] itemData = usr.costume[itemIndex].Split('-');
                    DateTime itemTime = DateTime.ParseExact(itemData[3], "yyMMddHH", null);
                    itemTime = itemTime.AddDays(days);
                    itemData[3] = String.Format("{0:yyMMddHH}", itemTime);
                    usr.costume[itemIndex] = string.Join("-", itemData);
                    if (i != null)
                    {
                        if (i.accruable || i.BuyType == 4)
                        {
                            int count = 1;
                            int.TryParse(itemData[4], out count);
                            if (count > i.maxAccrueCount) count = i.maxAccrueCount;
                            itemData[4] = (count + 1).ToString();
                        }
                    }
                    DB.RunQuery("UPDATE users_costumes SET inventory='" + Costumelist(usr) + "' WHERE ownerid='" + usr.userId + "'");
                    return true;
                }
                else
                {
                    int index = index = Array.IndexOf(usr.costume, "^");

                    if (index != -1 && index < Configs.Server.Player.MaxCostumeSlot)
                    {
                        if (usr.costume[index] == "^")
                        {
                            DateTime current = DateTime.Now;
                            current = current.AddDays(days);
                            current = current.AddHours(-4);
                            usr.costume[index] = item + "-1-1-" + String.Format("{0:yyMMddHH}", current) + "-0-0-0-0-0";
                            DB.RunQuery("UPDATE users_costumes SET inventory='" + Costumelist(usr) + "' WHERE ownerid='" + usr.userId + "'");
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        DB.RunQuery("INSERT INTO inbox (ownerid, itemcode, days) VALUES ('" + usr.userId + "', '" + item + "', '" + days + "')");
                        usr.send(new Game.SP_CustomMessageBox("The item you bought has been added to inbox.\nYour inventory is full, delete a item and relogin."));
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.WriteError("Error at AddCostume: " + ex.Message + " - " + ex.StackTrace);
            }
            return false;
        }

        public static void IncreaseEAItem(User usr, string item, int c = 1)
        {
            int itemIndex = usr.GetItemIndex(item);

            if (itemIndex != -1)
            {
                string[] itemdata = usr.inventory[itemIndex].Split('-');
                int count = 0;
                int.TryParse(itemdata[4], out count);
                if (count > 0)
                {
                    int b = (count + c);
                    if (b > 999) b = 999;
                    itemdata[4] = b.ToString();
                    usr.inventory[itemIndex] = string.Join("-", itemdata);
                }
            }
        }
        public static void DecreaseEAItem(User usr, string item, int c = 1)
        {
            int itemIndex = usr.GetItemIndex(item);

            if (itemIndex != -1)
            {
                string[] itemdata = usr.inventory[itemIndex].Split('-');
                int count = 0;
                int.TryParse(itemdata[4], out count);
                count -= c;
                if (count >= 1)
                {
                    itemdata[4] = (count.ToString());
                    usr.inventory[itemIndex] = string.Join("-", itemdata);
                }
                else
                {
                    usr.deleteItem(item);
                }
            }
        }

        public static int GetEAItem(User usr, string item)
        {
            int itemIndex = usr.GetItemIndex(item);

            if (itemIndex != -1)
            {
                string[] itemdata = usr.inventory[itemIndex].Split('-');
                int count = 0;
                int.TryParse(itemdata[4], out count);
                return count;
            }
            return 0;
        }

        public static int GetExpirationDate(User usr, string item)
        {
            int index = usr.GetItemIndex(item);

            int time = int.Parse(DateTime.Now.ToString("yyMMddHH"));

            if (index != -1)
            {
                int.TryParse(usr.inventory[index].Split('-')[3], out time);
            }
            return time;
        }

        public static int GetUserWeaponCount(User usr)
        {
            return usr.inventory.Where(r => r.StartsWith("D")).Count();
        }

        public static bool isPXItem(string Weapon)
        {
            Item item = ItemManager.GetItem(Weapon);
            if (item != null)
            {
                return item.UseableSlot(5); //>--- Slot (PXs)
            }
            return false;
        }
    }
}
