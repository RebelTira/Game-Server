using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Game_Server.Managers;

namespace Game_Server.Game
{
    class CP_CashItemBuy : Handler
    {
        internal enum SubCodes
        {
            OnItemBuy = 1110,
            OnItemUse = 1111,
            OnItemShopOpen = 1113,
            Storage = 1400
        }

        public override void Handle(User usr)
        {
            if (usr.room != null) return;
            SubCodes opcode = (SubCodes)int.Parse(getBlock(0));
            switch (opcode)
            {
                case SubCodes.OnItemShopOpen:
                    {
                        usr.RefreshCash();
                        break;
                    }
                case SubCodes.OnItemBuy:
                    {
                        string itemcode = getBlock(6).ToUpper();
                        int period = int.Parse(getBlock(3));

                        ushort days = (ushort)Inventory.GetDaysFromPeriod(period);
                        
                        Item item = ItemManager.GetItem(itemcode);
                        if (item != null)
                        {
                            if (days > 0)
                            {
                                if (item.Buyable || Configs.Server.ItemShop.hiddenItems.Contains(itemcode))
                                {
                                    int inventorySlot = Inventory.GetFreeItemSlotCount(usr);
                                    if (inventorySlot > 0)
                                    {
                                        uint price = (uint)item.GetCashPrice(period);

                                        usr.RefreshCash(); 

                                        if (price > 0)
                                        {
                                            bool px_item = (itemcode.ToLower().StartsWith("cz") || itemcode.ToLower().StartsWith("cb")); //>--- PremiumUsersOnly
                                            int result = (int)(usr.cash - price);
                                            if (item.Premium && usr.premium < 1)
                                            {
                                                usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.PremiumUsersOnly));
                                            }
                                            /*else if (usr.level < item.Level && usr.rank < 2) // >--- Comentado en origen
                                            {
                                                usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.LevelLow));
                                            }*/
                                            else if(item.accruable && Inventory.GetEAItem(usr, item.Code) >= item.maxAccrueCount)
                                            {
                                                usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.CannotBeBougth));
                                            }
                                            else if (result < 0)
                                            {
                                                usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.NotEnoughDinar));
                                            }
                                            else
                                            {
                                                if (px_item)
                                                    days = 3600;

                                                ushort count = 1;

                                                if (item.accruable)
                                                {
                                                    ushort d = (ushort)item.GetEACount(period);
                                                    if (d >= 1)
                                                    {
                                                        count = d;
                                                    }
                                                }

                                                switch (itemcode)
                                                {
                                                    case "CB53": //>--- CHG_CLANEXTEND
                                                        {
                                                            if (usr.clan == null || usr.clan.maxUsers >= 100)
                                                            {
                                                                usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.CannotBeBougth));
                                                                return;
                                                            }
                                                            usr.clan.maxUsers += 20;
                                                            usr.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Clan, Configs.Server.SystemName + " >> Clan expanded, please re-open clan tab to see changes.", 999, Configs.Server.SystemName));
                                                            usr.send(new SP_DinarItemBuy(usr, itemcode));
                                                            usr.send(new SP_Clan.MyClanInformation(usr));
                                                            DB.RunQuery("UPDATE clans SET maxusers='" + usr.clan.maxUsers + "' WHERE id='" + usr.clan.id + "'");
                                                            return;
                                                        }
                                                    default:
                                                        {
                                                            if ((itemcode.StartsWith("CZ") || itemcode.StartsWith("CC") || itemcode.StartsWith("CR") || itemcode.StartsWith("CB")) && !itemcode.StartsWith("CC0") && itemcode != "CC38")
                                                            {
                                                                days = 3600;
                                                            }
                                                            Inventory.AddOutBoxItem(usr, itemcode, (ushort)days, count);
                                                            break;
                                                        }
                                                }
                                                //DB.RunQuery("INSERT INTO purchases_logs (userid, log, timestamp) VALUES ('" + usr.userId + "', '" + usr.nickname + " bought " + item.Name + " for " + days + " days [" + price + " Cash - Game]', '" + Generic.timestamp + "')");

                                                usr.cash = result;

                                                DB.RunQuery("UPDATE users SET cash=" + result + " WHERE id='" + usr.userId + "'");
                                                usr.send(new SP_CashItemBuy(usr));
                                                usr.send(new SP_OutboxSend(usr));
                                            }
                                        }
                                        else
                                        {
                                            usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.CannotBeBougth));
                                        }
                                    }
                                    else
                                    {
                                        usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.InventoryFull));
                                    }
                                }
                                else
                                {
                                    usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.NoLongerValid));
                                }
                            }
                            else
                            {
                                usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.NoLongerValid));
                            }
                        }
                        break;
                    }
                case SubCodes.Storage:
                    {
                        usr.send(new SP_CustomMessageBox("The storage system has been disabled because it's useless."));
                        return; 

                        /* atm it has some few bugs but working and itself it's useless*/
                        /*
                        int action = int.Parse(getBlock(1));
                     
                        switch(action)
                        {
                            case 2: // Move to storage box
                                {
                                    string itemCode = getBlock(3);
                                    int invIndex = int.Parse(getBlock(2));
                                    string[] data = usr.inventory[invIndex].Split('-');
                                    if(data[0] == itemCode)
                                    {
                                        int emptyIndex = Array.IndexOf(usr.storageInventory, "^");
                                        if(emptyIndex >= 0 && emptyIndex < usr.storageInventoryMax)
                                        {
                                            usr.storageInventory[emptyIndex] = usr.inventory[invIndex];
                                            usr.inventory[invIndex] = "^";

                                            string invCode = Inventory.calculateInventory(invIndex);

                                            for (int i = 0; i < 5; i++)
                                            {
                                                for (int j = 0; j < 8; j++)
                                                {
                                                    if (usr.equipment[i, j] == "I" + invCode || usr.equipment[i, j] == itemCode)
                                                    {
                                                        usr.equipment[i, j] = "^";
                                                    }
                                                    else if (usr.equipment[i, j].Contains("-"))
                                                    {
                                                        string[] multipleSlot = usr.equipment[i, j].Split('-');
                                                        if (multipleSlot[0] == "I" + invCode)
                                                        {
                                                            usr.equipment[i, j] = multipleSlot[1];
                                                        }
                                                        else if (multipleSlot[1] == "I" + invCode)
                                                        {
                                                            usr.equipment[i, j] = multipleSlot[0];
                                                        }
                                                    }
                                                }
                                            }

                                            usr.SaveEquipment();

                                            usr.send(new SP_StorageInventoryUpdate(usr, action, invIndex, itemCode));

                                            usr.send(new SP_UpdateInventory(usr, null));

                                            DB.RunQuery("UPDATE equipment SET inventory = '" + Inventory.Itemlist(usr) + "', storage = '" + Inventory.Storage(usr) + "' WHERE ownerid = '" + usr.userId + "'");
                                        }
                                        else
                                        {
                                            usr.send(new SP_StorageInventoryUpdate(SP_StorageInventoryUpdate.ErrorCode.NoStorageFreeSpace));
                                        }
                                    }
                                    break;
                                }
                            case 3: // Move to inventory
                                {
                                    string itemCode = getBlock(3);
                                    int invIndex = int.Parse(getBlock(2));
                                    int wrTime = Generic.WarRockDateTime;
                                    string[] data = usr.storageInventory[invIndex].Split('-');
                                    if (data[0] == itemCode)
                                    {
                                        int time = int.Parse(data[3]);
                                        if (time > wrTime)
                                        {
                                            if (usr.HasItem(itemCode))
                                            {
                                                usr.storageInventory[invIndex] = "^";

                                                int inventoryIndex = usr.GetItemIndex(itemCode);

                                                string[] inventoryString = usr.inventory[inventoryIndex].Split('-');

                                                DateTime dtStored = DateTime.ParseExact(inventoryString[3], "yyMMddHH", null);

                                                TimeSpan ts = dtStored - DateTime.Now;

                                                Inventory.AddItem(usr, itemCode, (int)ts.TotalDays);

                                                usr.send(new SP_StorageInventoryUpdate(usr, action, invIndex, itemCode));
                                            }
                                            else
                                            {
                                                int emptyIndex = Array.IndexOf(usr.inventory, "^");
                                                if (emptyIndex >= 0)
                                                {
                                                    usr.inventory[emptyIndex] = usr.storageInventory[invIndex];
                                                    usr.storageInventory[invIndex] = "^";
                                                    
                                                    usr.send(new SP_StorageInventoryUpdate(usr, action, invIndex, itemCode));
                                                }
                                                else
                                                {
                                                    usr.send(new SP_StorageInventoryUpdate(SP_StorageInventoryUpdate.ErrorCode.NoInventoryFreeSpace));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            usr.storageInventory[invIndex] = "^";

                                            usr.send(new SP_StorageInventoryList(usr));
                                        }
                                        DB.RunQuery("UPDATE equipment SET inventory = '" + Inventory.Itemlist(usr) + "', storage = '" + Inventory.Storage(usr) + "' WHERE ownerid = '" + usr.userId + "'");
                                    }
                                    break;
                                }
                            case 4: // Remove all expired items
                                {
                                    int wrTime = Generic.WarRockDateTime;
                                    for (int i = 0; i < usr.storageInventory.Length; i++)
                                    {
                                        try
                                        {
                                            string data = usr.storageInventory[i];
                                            if (data != "^")
                                            {
                                                string[] split = data.Split('-');
                                                int time = int.Parse(split[3]);
                                                if (time < wrTime)
                                                {
                                                    usr.storageInventory[i] = "^";
                                                }
                                            }
                                        }
                                        catch { }
                                    }


                                    usr.send(new SP_StorageInventoryList(usr));
                                    DB.RunQuery("UPDATE equipment SET inventory = '" + Inventory.Itemlist(usr) + "', storage = '" + Inventory.Storage(usr) + "' WHERE ownerid = '" + usr.userId + "'");
                                    break;
                                }
                        }

                        break;
                        */
                    }
                    
                case SubCodes.OnItemUse:
                    {
                        string itemcode = getBlock(4);
                        if (!usr.HasItem(itemcode)) { return; }

                        //Log.WriteLine("CashItemBuy-299 INCOMING PACKET :: " + string.Join(" ", getAllBlocks));  
                        
                        if (PackageManager.AddItem(usr, itemcode))
                        {
                            usr.send(new SP_UseItem(usr, itemcode));
                        }
                        else
                        {
                            switch (itemcode)
                            {
                                case "CB01": // Change Nickname
                                    {
                                        string nickname = getBlock(5);
                                        if (!Generic.IsAlphaNumeric(nickname)) return;

                                        DataTable dt = DB.RunReader("SELECT * FROM users WHERE nickname='" + nickname + "'");

                                        if (dt.Rows.Count == 0)
                                        {
                                            DB.RunQuery("INSERT INTO changenick_logs (userId, oldnick, newnick, date, timestamp) VALUES ('" + usr.userId + "', '" + usr.nickname + "', '" + nickname + "', '" + Generic.currentDate + "', '" + Generic.timestamp + "')");

                                            Log.WriteLine("---" + usr.nickname + " is now known as " + nickname + "---");

                                            usr.nickname = nickname;
                                            DB.RunQuery("UPDATE users SET nickname='" + usr.nickname + "' WHERE id='" + usr.userId + "'");
                                            usr.deleteItem(itemcode);
                                            usr.send(new SP_CashItemUse(usr, itemcode));
                                            
                                            int clanrank = usr.clan.clanRank(usr);
                                            switch (clanrank)
                                            {
                                                default:
                                                    {
                                                        ClanUsers cu = usr.clan.GetUser(usr.userId);
                                                        if (cu != null)
                                                        {
                                                            cu.EXP = usr.exp.ToString();
                                                            cu.nickname = usr.nickname;
                                                        }
                                                        break;
                                                    }
                                                case 9:
                                                    {
                                                        ClanPendingUsers cu = usr.clan.getPendingUser(usr.userId);
                                                        if (cu != null)
                                                        {
                                                            cu.EXP = usr.exp.ToString();
                                                            cu.nickname = usr.nickname;
                                                        }
                                                        break;
                                                    }
                                            }
                                        }
                                        break;
                                    }
                                case "CB03": // Kill / Death Reset
                                    {
                                        usr.kills = usr.deaths = 0;
                                        DB.RunQuery("UPDATE users SET kills = '" + usr.kills + "', deaths = '" + usr.deaths + "' WHERE id='" + usr.userId + "'");
                                        usr.deleteItem(itemcode);
                                        usr.send(new SP_CashItemUse(usr, itemcode));
                                        break;
                                    }
                                case "CZ73": // Ham Radio
                                    {
                                        string message = getBlock(5);

                                        Inventory.DecreaseEAItem(usr, itemcode);

                                        string[] m = message.Split(' ');

                                        for (int i = 0; i < m.Length; i++)
                                        {
                                            m[i] = WordFilterManager.Replace(m[i]);
                                        }

                                        message = string.Join(" ", m);

                                        usr.send(new SP_CashItemUse(usr, "CB03"));
                                        usr.AddAdminCPLog(usr.nickname + " sent message " + message.Replace((char)0x1D, (char)0x20) + " [HAM_RADIO]");
                                        UserManager.sendToServer(new SP_Chat(usr.nickname, SP_Chat.ChatType.Notice1, message, 0, usr.nickname));
                                        break;
                                    }
                                case "CC56": // Random Box 1
                                case "CC57": // Random Box 2
                                case "CC36": // Random Box 3
                                case "CC37": // Random Box 4
                                    {
                                        Item boxItem = ItemManager.GetItem(itemcode);

                                        if (boxItem != null)
                                        {
                                            List<PackageItem> randomItems = boxItem.packageItems;

                                            if (randomItems.Count > 0)
                                            {
                                                if (Inventory.GetFreeItemSlotCount(usr) > 0)
                                                {
                                                    int index = Generic.random(0, randomItems.Count - 1);
                                                    if (index == 0 && Generic.random(100, 1000) > 200)
                                                    {
                                                        index = Generic.random(1, randomItems.Count - 1);
                                                    }
                                                    PackageItem item = randomItems[index];
                                                    Managers.Item i = Managers.ItemManager.GetItem(item.item);
                                                    if (i != null)
                                                    {
                                                        int days = item.days;
                                                        Inventory.AddItem(usr, item.item, days);
                                                        Inventory.DecreaseEAItem(usr, itemcode);
                                                        usr.send(new SP_WinItem(usr, item.item, days));
                                                    }
                                                    else
                                                    {
                                                        Log.WriteError(item + " is not a valid item @ random box!");
                                                    }
                                                }
                                            }
                                        }

                                        break;
                                    }
                                case "CZ99": // Random Box
                                    {
                                        if (Inventory.GetFreeItemSlotCount(usr) > 0)
                                        {
                                            string[] randomItems = Configs.Server.RandomBoxEvent.items;
                                            int index = Generic.random(0, randomItems.Length - 1);
                                            string item = randomItems[index];
                                            Managers.Item i = Managers.ItemManager.GetItem(item);
                                            if (i != null)
                                            {
                                                int days = new Random().Next(Configs.Server.RandomBoxEvent.MinDays, Configs.Server.RandomBoxEvent.MaxDays);
                                                if (item.ToUpper().StartsWith("B"))
                                                {
                                                    Inventory.AddCostume(usr, item, days);
                                                }
                                                else
                                                {
                                                    Inventory.AddItem(usr, item, days);
                                                }
                                                
                                                Inventory.DecreaseEAItem(usr, itemcode);
                                                usr.send(new SP_WinItem(usr, item, days));
                                            }
                                            else
                                            {
                                                Log.WriteError(item + " is not a valid item @ random box event!");
                                            }
                                        }
                                        break;
                                    }
                                case "CR16": // Attendance Box
                                    {
                                        if (Inventory.GetFreeItemSlotCount(usr) > 0)
                                        {
                                            string[] randomItems = Configs.Server.ItemShop.attendanceBox;
                                            int index = Generic.random(0, randomItems.Length - 1);
                                            string item = randomItems[index];
                                            Managers.Item i = Managers.ItemManager.GetItem(item);
                                            if (i != null)
                                            {
                                                int days = new Random().Next(3, 14);
                                                if (item.ToUpper().StartsWith("B"))
                                                {
                                                    Inventory.AddCostume(usr, item, days);
                                                }
                                                else
                                                {
                                                    Inventory.AddItem(usr, item, days);
                                                }

                                                Inventory.DecreaseEAItem(usr, itemcode);
                                                usr.send(new SP_WinItem(usr, item, days));
                                            }
                                            else
                                            {
                                                Log.WriteError(item + " is not a valid item @ attendance box box event!");
                                            }
                                        }
                                        break;
                                    }
                                case "CR05": // WarRock Gift Box
                                    {
                                        if (Inventory.GetFreeItemSlotCount(usr) > 0)
                                        {
                                            string[] randomItems = Configs.Server.ChristmasBoxEvent.items;
                                            int index = Generic.random(0, randomItems.Length - 1);
                                            string item = randomItems[index];
                                            Managers.Item i = Managers.ItemManager.GetItem(item);
                                            if (i != null)
                                            {
                                                int days = new Random().Next(Configs.Server.ChristmasBoxEvent.MinDays, Configs.Server.ChristmasBoxEvent.MaxDays);
                                                if (item.ToUpper().StartsWith("B"))
                                                {
                                                    Inventory.AddCostume(usr, item, days);
                                                }
                                                else
                                                {
                                                    Inventory.AddItem(usr, item, days);
                                                }

                                                Inventory.DecreaseEAItem(usr, itemcode);
                                                usr.send(new SP_WinItem(usr, item, days));
                                            }
                                            else
                                            {
                                                Log.WriteError(item + " is not a valid item @ random box event!");
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    }
                    break;
            }
        }
    }

    class SP_WinItem : Packet
    {
        public SP_WinItem(User usr, string itemcode, int days)
        {
            newPacket(30720);
            addBlock(1111);
            addBlock(1);
            addBlock("CB09");
            addBlock(Inventory.Itemlist(usr));
            //T,T,F,F CF02 30 47728
            addBlock(usr.AvailableSlots);
            addBlock(itemcode);
            addBlock(days);
            addBlock(usr.dinar);
        }
    }

    class SP_UseItem : Packet
    {
        public SP_UseItem(User usr, string itemcode)
        {
            //30720 1111 1 CB09 DB33-3-0-13080422-0,CB08-2-0-13052022-3,CC02-3-0-13080422-0,DS01-3-0-13080903-0,CA01-3-0-13081400-0,CD01-3-0-13080422-0,CD02-3-0-13080422-0,DB04-1-0-13070914-0,DA09-1-0-13070215-0,DF03-1-0-13070214-0,DT01-1-0-13071700-0,^,DH01-1-0-13071921-0,DI01-1-0-13062921-0,CF02-3-0-13072602-0,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^ T,T,F,F CF02 30 47728 
            newPacket(30720);
            addBlock(1111);
            addBlock(1);
            addBlock(itemcode);
            addBlock(Inventory.Itemlist(usr));
            addBlock(usr.AvailableSlots);
            addBlock(itemcode);
            addBlock(0);
            addBlock(usr.dinar);
        }
    }

    class SP_CashItemBuy : Packet
    {
        public SP_CashItemBuy(User usr, string ItemCode, int Days)
        {
            //30720 1111 1 CB09 DB33-3-0-13080422-0,CB08-2-0-13052022-3,CC02-3-0-13080422-0,DS01-3-0-13080903-0,CA01-3-0-13081400-0,CD01-3-0-13080422-0,CD02-3-0-13080422-0,DB04-1-0-13070914-0,DA09-1-0-13070215-0,DF03-1-0-13070214-0,DT01-1-0-13071700-0,^,DH01-1-0-13071921-0,DI01-1-0-13062921-0,CF02-3-0-13072602-0,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^ T,T,F,F CF02 30 47728 
            newPacket(30720);
            addBlock(1111);
            addBlock(1);
            addBlock(ItemCode);
            addBlock(Inventory.Itemlist(usr));
            addBlock(usr.AvailableSlots);
            addBlock(ItemCode);
            addBlock(Days); 
            addBlock(usr.dinar);
        }

        public SP_CashItemBuy(User usr)
        {
            newPacket(30720);
            addBlock(1113);
            addBlock(1);
            addBlock(usr.cash);
        }

        public SP_CashItemBuy(User usr, string Items)
        {
            newPacket(30720);
            addBlock(1118);
            addBlock(1);
            addBlock(usr.cash);
            addBlock(Items);
            addBlock(usr.AvailableSlots);
            addBlock(0);
            addBlock(usr.dinar);
        }
    }

    class SP_StorageInventoryList : Packet
    {
        public SP_StorageInventoryList(User usr)
        {
            newPacket(30720);
            addBlock(1400);
            addBlock(1);
            addBlock(0);
            addBlock(usr.storageInventoryMax);
            addBlock(Inventory.Storage(usr));
        }
    }
    
    class SP_StorageInventoryUpdate : Packet
    {
        internal enum ErrorCode : uint
        {
            NoInventoryFreeSpace = 97070,
            NoStorageFreeSpace = 97071
        }

        public SP_StorageInventoryUpdate(ErrorCode code)
        {
            newPacket(30720);
            addBlock(1400);
            addBlock((uint)code);
        }

        public SP_StorageInventoryUpdate(User usr, int action, int index, string itemCode)
        {
            newPacket(30720);
            addBlock(1400);
            addBlock(1);
            addBlock(action);
            addBlock(usr.storageInventoryMax);
            addBlock(index);
            addBlock(itemCode);
            addBlock(Inventory.Storage(usr));
            addBlock(Inventory.Itemlist(usr));
        }
    }

    class SP_CashItemUse : Packet
    {
        internal enum ErrCode
        {
            NeedSupplyBox = -3
        }

        public SP_CashItemUse(SP_CashItemUse.ErrCode ErrCode, User usr, string ItemCode)
        {
            newPacket(30720);
            addBlock(1111);
            addBlock((int)ErrCode);
            addBlock(ItemCode);
            addBlock(Inventory.Itemlist(usr));
        }

        public SP_CashItemUse(User usr, string ItemCode)
        {
            newPacket(30720);
            addBlock(1111);
            addBlock(1);
            addBlock(ItemCode);
            addBlock(Inventory.Itemlist(usr));
            if (ItemCode == "CB03") //>--- INI_KILLDEATH
            {
                addBlock(usr.AvailableSlots);
                addBlock(0);
                addBlock(0);
                addBlock(usr.dinar);
            }
            else if (ItemCode == "CB01") //>--- CHG_NICK
            {
                addBlock(usr.AvailableSlots);
                addBlock(usr.nickname);
            }
        }
    }
}