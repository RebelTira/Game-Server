using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Game_Server.Game
{
    class SP_ShopCoupon : Packet
    {
        public enum Subtype
        {
            AlreadyUsedCouponByOther = -1, //This coupon number has already been registered
            WrongCoupon = -2, // Wrong Coupon
            CouponRegistrationError = -3, //Registration Error
            UnknownError = -4, // Unknown Error
            CouponCanNotBeUsedUnder7Days = -5, //Coupons only can be used always after 7 days
            UserDinarIsToHigh = -6, // At the moment the users dinar is to high
            AlreadyUsedCouponByHimself = -7, // AlreadyUsed By User
            CouponIsExpired = -8,
            InventoryFull = -9,
            Unknown = -10,
            InvalidCoupon = -11 //Invaild
        }

        public SP_ShopCoupon(Subtype Subtype)
        {
            newPacket(30992);
            addBlock((int)Subtype);
        }

        public SP_ShopCoupon(User usr)
        {
            newPacket(30992);
            addBlock(1);
            addBlock("#toxiic");
            addBlock(0);
            addBlock(Inventory.Itemlist(usr));
            addBlock(usr.AvailableSlots);
            addBlock(Inventory.Costumelist(usr));
            addBlock(usr.dinar);
            addBlock(usr.exp);
        }
    }
    class CP_ShopCoupon : Handler
    {
        public override void Handle(User usr)
        {
            string coupon = getBlock(0).Replace("-", "");
            
            DataTable dt = DB.RunReader("SELECT * FROM ingame_coupons WHERE code='" + coupon + "'");
            
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                bool used = row["used"].ToString() == "1";
                int userId = int.Parse(row["userId"].ToString());
                if (!used)
                {
                    if (Inventory.GetFreeItemSlotCount(usr) > 0)
                    {
                        uint dinars = uint.Parse(row["dinars"].ToString());
                        if (dinars > 0)
                        {
                            usr.dinar += (int)dinars;
                        }

                        uint cashs = uint.Parse(row["cashs"].ToString());
                        if (cashs > 0)
                        {
                            usr.cash  += (int)cashs;
                            usr.send(new SP_CashItemBuy(usr));
                        }

                        if (dinars > 0 || cashs > 0)
                        {
                            DB.RunQuery("UPDATE users SET cash='" + usr.cash + "', dinar='" + usr.dinar + "' WHERE id='" + usr.userId + "'");
                        }

                        if (row["items"].ToString().Length > 0)
                        {
                            List<string> items = row["items"].ToString().Split('|').ToList();

                            if (items.Count > 0)
                            {
                                foreach (string i in items)
                                {
                                    string[] data = i.Split(',');
                                    string code = data[0];
                                    int days = 0;
                                    int.TryParse(data[1], out days);
                                    if (days != 0)
                                    {
                                        Inventory.PerformAddItem(usr, code, days);
                                    }
                                }
                            }
                        }

                        DB.RunQuery("UPDATE ingame_coupons SET used='1', userId='" + usr.userId + "', time='" + Generic.timestamp + "' WHERE code='" + coupon + "'");

                        usr.send(new SP_ShopCoupon(usr));
                    }
                    else
                    {
                        usr.send(new SP_ShopCoupon(SP_ShopCoupon.Subtype.InventoryFull));
                    }
                }
                else
                {
                    usr.send(new SP_ShopCoupon(userId == usr.userId ? SP_ShopCoupon.Subtype.AlreadyUsedCouponByHimself : SP_ShopCoupon.Subtype.AlreadyUsedCouponByOther));
                }
            }
            else
            {
                usr.send(new SP_ShopCoupon(SP_ShopCoupon.Subtype.WrongCoupon));
            }
        }
    }
}