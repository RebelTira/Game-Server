using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Managers;

namespace Game_Server.Game
{
    class CP_DinarItemBuy : Handler
    {
        public override void Handle(User usr)
        {
            if (usr.room != null) return;
            
            string itemcode = getBlock(1).ToUpper();
            int period = int.Parse(getBlock(4));

            int days = Inventory.GetDaysFromPeriod(period);

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
                            uint price = (uint)item.GetPrice(period);
                            double left = 1;
                            if (usr.HasItem(itemcode))
                            {
                                DateTime item_expire_time = DateTime.ParseExact(Inventory.GetExpirationDate(usr, itemcode).ToString(), "yyMMddHH", null);
                                left = ((TimeSpan)(item_expire_time - DateTime.Now)).TotalDays;
                            }
                            if (price > 0)
                            {
                                if (left < 60)
                                {
                                    bool px_item = (itemcode.ToLower().StartsWith("cz"));
                                    int result = (int)(usr.dinar - price);
                                    if (item.Premium && usr.premium < 1)
                                    {
                                        usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.PremiumUsersOnly));
                                    }
                                    else if (usr.level < item.Level && usr.rank < 2)
                                    {
                                        usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.LevelLow));
                                    }
                                    else if (result < 0)
                                    {
                                        usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.NotEnoughDinar));
                                    }
                                    else
                                    {
                                        if (px_item)
                                            days = 3600;

                                        if (Inventory.AddItem(usr, itemcode, days))
                                        {
                                            usr.dinar = result;

                                            usr.send(new SP_DinarItemBuy(usr, itemcode));

                                            //DB.RunQuery("INSERT INTO purchases_logs (userid, log, timestamp) VALUES ('" + usr.userId + "', '" + usr.nickname + " bought " + item.Name + " for " + days + " days [" + price + " Dinar - Game]', '" + Generic.timestamp + "')");

                                            DB.RunQuery("UPDATE users SET dinar=" + result+ " WHERE id='" + usr.userId + "'");
                                        }
                                        else
                                        {
                                            usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.CannotBeBougth));
                                        }
                                    }
                                }
                                else
                                {
                                    usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.MaximumTimeLimit));
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
                    // Fake packet sent / client modified??
                    usr.disconnect();
                }
            }
        }
    }

    class SP_DinarItemBuy : Packet
    {
        internal enum ErrorCodes
        {
            Success = 1,
            NoLongerValid = 97010,
            PurchaseFifthSlotFirst = 97012,
            CannotBeBougth = 97020,
            NotEnoughDinar = 97040,
            LevelLow = 97050,
            NotEnoughCash = 97092,
            InventoryFull = 97070,
            MaximumTimeLimit = 97100,
            PremiumUsersOnly = 98010,
            GoldUsersOnly = 98020,
            FifthSlotFreeForGoldUsers = 98030,
        }

        public SP_DinarItemBuy(User usr, string item)
        {
            newPacket(30208);
            addBlock(1);
            addBlock(1110);
            addBlock(-1);
            addBlock(3); // Important
            addBlock(4); // <- Item Count | Only Weapons are counted Inventory.GetUserWeaponCount(usr)
            addBlock(Inventory.Itemlist(usr));
            addBlock(usr.dinar);
            addBlock(usr.AvailableSlots); // Slot Enabled
        }

        public SP_DinarItemBuy(ErrorCodes err, params object[] Params)
        {
            newPacket(30208);
            addBlock((int)err);
            foreach (object obj in Params)
            {
                addBlock(obj);
            }
        }
    }
}
    
   
