using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_CouponEvent : Packet
    {
        public SP_CouponEvent(int TodayCoupon, int Coupons)
        {
            newPacket(25605);
            addBlock(1);
            addBlock(TodayCoupon);
            addBlock(Coupons);
            addBlock(1);
            addBlock(1);
            addBlock("0-0-0-0-0-0-0-0-0");
        }

        public SP_CouponEvent(User usr)
        {
            newPacket(25605);
            addBlock(0);
            addBlock(usr.todaycoupons); // Today Coupons
            addBlock(usr.coupons); // Coupons
            addBlock(usr.coupontime);
            addBlock(0);
            addBlock("0-0-0-0-0-0-0-0-0");
        }
    }

    class CP_CouponEvent : Handler
    {
        public override void Handle(User usr)
        {
            if (usr.room != null || !Configs.Server.Player.CouponEvent) return;

            usr.send(new SP_CouponEvent(usr));
        }
    }

    class SP_CouponBuy : Packet
    {
        internal enum ErrorCode
        {
            NotEnoughCoupons = 1
        }

        public SP_CouponBuy(ErrorCode ErrCode)
        {
            newPacket(25606);
            addBlock((int)ErrCode);
        }

        public SP_CouponBuy(string WeaponCode, User usr)
        {
            newPacket(25606);
            addBlock(0);
            addBlock(usr.AvailableSlots); // Slot Enabled
            addBlock(Inventory.Itemlist(usr));
            addBlock(usr.coupontime);
            addBlock(usr.dinar);
            addBlock(usr.todaycoupons);
            addBlock(usr.coupons);
            addBlock(Inventory.Costumelist(usr));
        }
    }

    class CP_CouponBuy : Handler
    {
        public override void Handle(User usr)
        {
            if (usr.room != null || !Configs.Server.Player.CouponEvent) return;
            int ID = -1;
            int.TryParse(getBlock(0), out ID);
            if (ID >= 0 && ID <= 8)
            {
                int Days = 0;
                int CouponToRemove = 0;
                string ItemBuy = null;
                #region Calculate
                switch (ID)
                {
                    case 0: ItemBuy = "CC41"; Days = 3; CouponToRemove = 15; break;
                    case 1: ItemBuy = "CI01"; Days = 3; CouponToRemove = 10; break;
                    case 2: ItemBuy = "DF96"; Days = 3; CouponToRemove = 20; break;
                    case 3: ItemBuy = "BS12"; Days = 3; CouponToRemove = 10; break;
                    case 4: ItemBuy = "DF14"; Days = 3; CouponToRemove = 15; break;
                    case 5: ItemBuy = "DC40"; Days = 3; CouponToRemove = 15; break;
                    case 6: ItemBuy = "DF18"; Days = 3; CouponToRemove = 15; break;
                    case 7: ItemBuy = "DF12"; Days = 3; CouponToRemove = 10; break;
                    case 8: ItemBuy = "DG44"; Days = 3; CouponToRemove = 10; break;
                }
                #endregion
                if (usr.coupons >= CouponToRemove)
                {
                    int InventorySlot = Inventory.GetFreeItemSlotCount(usr);
                    if (InventorySlot > 0)
                    {
                        usr.coupons -= CouponToRemove;
                        DB.RunQuery("UPDATE users SET coupons='" + usr.coupons + "' WHERE id='" + usr.userId + "'");
                        if (ItemBuy != null)
                        {
                            if (ItemBuy == "CC41")
                            {
                                int PremDays = new Random().Next(1, Days);
                                if (usr.premium == 3)
                                {
                                    usr.premiumExpire += (uint)(86400 * PremDays);
                                }
                                else
                                {
                                    usr.premiumExpire = (uint)(Generic.timestamp + (86400 * PremDays));
                                }

                                usr.premium = 3;
                                Inventory.AddItem(usr, "DB33", PremDays);
                                Inventory.AddItem(usr, "CD01", PremDays);
                                Inventory.AddItem(usr, "CD02", PremDays);
                                DB.RunQuery("UPDATE users SET premium='3', premiumExpire='" + usr.premiumExpire + "' WHERE id='" + usr.userId + "'");
                                usr.send(new SP_PingInformation(usr));
                            }
                            else
                            {
                                if (ItemBuy.StartsWith("B"))
                                {
                                    Inventory.AddCostume(usr, ItemBuy, Days);
                                }
                                else
                                    Inventory.AddItem(usr, ItemBuy, Days);
                            }

                            usr.send(new SP_CouponBuy(ItemBuy, usr));
                        }
                        else
                        {
                            usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.CannotBeBougth, "err"));
                        }
                    }
                    else
                    {
                        usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.InventoryFull, "NULL"));
                    }
                }
                else
                {
                    usr.send(new SP_CouponBuy(SP_CouponBuy.ErrorCode.NotEnoughCoupons));
                }
            }
            else
            {
                /* Fake packet - Invalid ID*/
                usr.disconnect();
            }
        }
    }
}