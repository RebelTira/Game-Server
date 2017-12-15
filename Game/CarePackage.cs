using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Managers;

namespace Game_Server.Game
{
    class SP_CARE_PACKAGE : Packet
    {
        internal class Open : Packet
        {
            public Open()
            {
                //30272 6 900 1 DC34 5 30 DC04 15 15 DG03 20 7 DC09 30 7 DB02 30 7 900 1 DF36 5 30 DF06 15 15 CE01 20 7 DK01 30 7 DF08 30 7 900 1 DC93 5 30 DC03 15 15 CE01 20 7 DF08 30 7 DB06 30 7 900 1 DG25 5 30 DG06 15 15 DB02 20 7 DK01 30 7 DC09 30 7 900 1 DF65 5 30 DF05 15 15 DO02 20 7 DB06 30 7 DC09 30 7 3000 0 DT01 5 30 DS01 15 15 DF03 20 7 DA09 30 7 DB04 30 7  
                newPacket(30272);
                addBlock(CarePackage.items.Count); // HowMuch

                foreach (CarePackageItem i in CarePackage.items.Values)
                {
                    addBlock(i.Price);
                    addBlock(i.Method);
                    addBlock(i.Item);
                    addBlock(-1);
                    addBlock(i.days);

                    addBlock(i.Item1);
                    addBlock(-1);
                    addBlock(i.days1);

                    addBlock(i.Item2);
                    addBlock(-1);
                    addBlock(i.days2);

                    addBlock(i.Item3);
                    addBlock(-1);
                    addBlock(i.days3);

                    addBlock(i.Item4);
                    addBlock(-1);
                    addBlock(i.days4);
                }
            }
        }

        internal class SendItem : Packet
        {
            public SendItem(User usr, string itemcode, int days, bool isdinar, bool win)
            {
                newPacket(30273);
                addBlock(1);
                addBlock(isdinar ? 0 : 1);
                addBlock(win ? 1 : 0);
                addBlock(itemcode);
                addBlock(1);
                addBlock(Inventory.Itemlist(usr));
                addBlock(isdinar ? usr.dinar : usr.cash);
                addBlock(usr.AvailableSlots); //Slots Enable
            }
        }
    }

    class CP_CarePackage : Handler
    {
        public override void Handle(User usr)
        {
            if (usr.room != null || !Configs.Server.Player.CarePackage) return;

            usr.send(new SP_CARE_PACKAGE());
        }
    }

    class CP_CarePackageSendItem : Handler
    {
        public override void Handle(User usr)
        {
            if (!Configs.Server.Player.CarePackage) return;
            try
            {
                int ItemID = int.Parse(getBlock(0));
                CarePackageItem i = CarePackage.GetItem(ItemID);
                if (i != null)
                {
                    string ItemCode = i.Item;
                    int Days = i.days;
                    bool isDinar = (i.Method == 0 ? true : false);
                    uint Price = (uint)i.Price;

                    int result = ((isDinar ? usr.dinar : usr.cash) - (int)Price);

                    bool Win = true;

                    int Rand = new Random().Next(0, 4);
                    if (Rand != 0)
                    {
                        Win = false;
                        switch (Rand)
                        {
                            case 1:
                                {
                                    ItemCode = i.Item1;
                                    Days = i.days1;
                                    break;
                                }
                            case 2:
                                {
                                    ItemCode = i.Item2;
                                    Days = i.days2;
                                    break;
                                }
                            case 3:
                                {
                                    ItemCode = i.Item3;
                                    Days = i.days3;
                                    break;
                                }
                            case 4:
                                {
                                    ItemCode = i.Item4;
                                    Days = i.days4;
                                    break;
                                }
                        }
                    }

                    if (result >= 0)
                    {
                        if (isDinar == true)
                        {
                            usr.dinar = (int)result;
                            DB.RunQuery("UPDATE users SET dinar='" + result + "' WHERE id='" + usr.userId + "'");
                            Inventory.AddItem(usr, ItemCode, Days);
                        }
                        else
                        {
                            usr.cash = (int)result;
                            DB.RunQuery("UPDATE users SET cash='" + result + "' WHERE id='" + usr.userId + "'");
                            Inventory.AddOutBoxItem(usr, ItemCode, (ushort)Days, 1);
                        }

                        usr.send(new SP_CARE_PACKAGE.SendItem(usr, ItemCode, Days, isDinar, Win));
                    }
                }
            }
            catch { }
        }
    }
}
