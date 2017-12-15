using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Managers;

namespace Game_Server.Game
{
    class CP_CostumeBuy : Handler
    {
        public override void Handle(User usr)
        {
            if (usr.room != null) return;
            int OPCode = int.Parse(getBlock(0));
            int Period = int.Parse(getBlock(4));
            string Code = getBlock(1);
            int[] convertDays = new int[6] { 3, 7, 15, 30, 60, 365 };
            Item Item = ItemManager.GetItem(Code);
            if (Item != null)
            {
                int Price = Item.GetCashPrice(Period);
                if (usr.cash - Price < 0)
                {
                    usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.NotEnoughDinar, "NULL"));
                }
                /*else if (LevelCalculator.getLevelforExp(usr.Exp) < Item.Level &&usr.Rank < 3)
                {
                    usr.send(new PACKET_ITEMSHOP(PACKET_ITEMSHOP.ErrorCodes.LevelLow, "NULL"));
                }*/
                else if (usr.cash >= Price && Item.Buyable == true && Price > 0)
                {
                    int InventorySlot = Inventory.GetFreeCostumeSlotCount(usr);
                    if (InventorySlot >= 0)
                    {
                        int result = (int)(usr.cash - Price);
                        if (result > 0)
                        {
                            ushort days = (ushort)Inventory.GetDaysFromPeriod(Period);
                            Inventory.AddOutBoxItem(usr, Code, days, 1);

                            usr.cash = result;

                            usr.send(new SP_OutboxSend(usr));
                            usr.send(new SP_CashItemBuy(usr));
                            DB.RunQuery("UPDATE users SET cash='" + result + "' WHERE id='" + usr.userId + "'");
                        }
                        else
                        {
                            usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.NotEnoughCash, "NULL"));
                        }
                    }
                    else
                    {
                        usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.InventoryFull, "NULL"));
                    }
                }
                else
                {
                    usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.CannotBeBougth, "NULL"));
                }
            }
            else
            {
                usr.send(new SP_DinarItemBuy(SP_DinarItemBuy.ErrorCodes.NoLongerValid, "NULL"));
            }
        }
    }
}
