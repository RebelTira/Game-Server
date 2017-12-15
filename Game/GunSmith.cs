using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Game_Server.Game
{
    class GunSmith
    {
        public int gameid, cost;
        public string item, rare;
        public string[] required_materials, required_items, lose_items;

        public GunSmith(int gameid, int cost, string item, string rare, string[] required_materials, string[] required_items, string[] lose_items)
        {
            this.gameid = gameid;
            this.cost = cost;
            this.item = item;
            this.rare = rare;
            this.required_materials = required_materials;
            this.required_items = required_items;
            this.lose_items = lose_items;
        }
    }

    class CP_GunSmith : Handler
    {
        internal enum WonType : byte
        {
            Lose = 0,
            Win = 1,
            RareWin = 2
        }
        enum Type : byte
        {
            Dinar = 0,
            Ticket = 1
        }
        public override void Handle(User usr)
        {
            const string FRM_CODE = "CZ83";
            const string FC_CODE = "CZ84";
            const string CFRP_CODE = "CZ85";
            int index = int.Parse(getBlock(0));
            Type type = (Type)int.Parse(getBlock(1));

            if (Inventory.GetFreeItemSlotCount(usr) == 0) return;

            if (type == Type.Ticket && usr.HasItem("CZ75") || type == Type.Dinar)
            {
                GunSmith gunsmith = Managers.GunSmithManager.GetGunSmithByGameId(index);
                if (gunsmith != null)
                {
                    string item = gunsmith.item;
                    string rare = gunsmith.rare;

                    int myCFRP = Inventory.GetEAItem(usr, CFRP_CODE);
                    int myFC = Inventory.GetEAItem(usr, FC_CODE);
                    int myFRM = Inventory.GetEAItem(usr, FRM_CODE);

                    // GLOBAL SET: FRM - FC - CFRP

                    string[] t = gunsmith.required_materials;

                    uint cost = (uint)gunsmith.cost;

                    if (type != Type.Dinar)
                    {
                        cost = 0;
                    }

                    if (cost >= 0)
                    {
                        int requiredCFRP = 0;
                        int requiredFC = 0;
                        int requiredFRM = 0;

                        int.TryParse(t[0].ToString(), out requiredCFRP);
                        int.TryParse(t[1].ToString(), out requiredFC);
                        int.TryParse(t[2].ToString(), out requiredFRM);

                        if (myCFRP >= requiredCFRP && myFC >= requiredFC && myFRM >= requiredFRM)
                        {
                            string[] required_items = gunsmith.required_items;

                            foreach (var i in required_items)
                            {
                                if (!usr.HasItem(i))
                                {
                                    Log.WriteError("User " + usr.nickname + " hasn't " + i);
                                    usr.disconnect();
                                    return;
                                }
                            }

                            foreach (var i in required_items)
                            {
                                usr.deleteItem(i);
                            }

                            if (requiredCFRP > 0)
                            {
                                Inventory.DecreaseEAItem(usr, CFRP_CODE, requiredCFRP);
                            }

                            if (requiredFC > 0)
                            {
                                Inventory.DecreaseEAItem(usr, FC_CODE, requiredFC);
                            }

                            if (requiredFRM > 0)
                            {
                                Inventory.DecreaseEAItem(usr, FRM_CODE, requiredFRM);
                            }

                            string[] lose_items = gunsmith.lose_items;

                            int WonPerc = Generic.random(0, 50);

                            int calcPerc = (type == Type.Dinar ? 10 : 25);

                            int days = 30;

                            WonType wonType = WonType.Win;

                            if (WonPerc > calcPerc)
                            {
                                wonType = WonType.Lose;
                                int idx = Generic.random(0, lose_items.Length - 1);
                                item = lose_items[idx];
                                days = Generic.random(7, 30);
                            }
                            else if (WonPerc == 17)
                            {
                                item = rare;
                                wonType = WonType.RareWin;
                            }

                            if (type == Type.Dinar)
                            {
                                usr.dinar -= (int)cost;
                            }
                            else
                            {
                                Inventory.DecreaseEAItem(usr, "CZ75");  //>---  GunSmith_Ticket
                            }

                            Inventory.AddItem(usr, item, days);
                            usr.send(new SP_GunSmith(usr, item, wonType));
                        }
                    }
                }
            }
            else
            {
                // If its not valid
                usr.disconnect();
            }
        }
    }

    class SP_GunSmith : Packet
    {
        public SP_GunSmith(User usr, string itemcode, CP_GunSmith.WonType wonType)
        {
            //30995 1 DF05 0 4406414 120407 CZ85-1-1-62022719-50,CZ84-1-1-62022719-50,CZ83-1-1-62022719-50,DB33-1-1-14083111-0-0-0-0-0,CD01-1-1-14083111-0-0-0-0-0,CD02-1-1-14083111-0-0-0-0-0,CC02-1-1-14083111-0-0-0-0-0,DF05-1-1-14093018-0,DG27-1-1-14080519-0,DF35-1-1-14080819-0,DC33-1-1-14080419-0,DS03-1-1-14081319-0,DB04-1-1-14080820-0,DC09-1-1-14080820-0,DC01-1-1-14080820-0,DB03-1-1-14080820-0,DF02-1-1-14080820-0,DF06-1-1-14080820-0,DB02-1-1-14080820-0,DG03-1-1-14080820-0,DD02-1-1-14080820-0,DD01-1-1-14080820-0,CZ75-1-1-14081120-1,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^ ^,I001,DF01,DR01,^,^,^,DF32 ^,I001,DF01,DQ01,I005,^,^,DF35 ^,I001,DG05,DN01,^,^,^,DG08 ^,I001,DC02,DN01,^,^,^,DC04 ^,I001,DJ01,DL01,^,^,^,DH04
            //30995 1 DF12 0 31150 2200 CA01-1-3-1408070913-0,DA03-5-3-1408070913-0,DB08-5-3-1408070913-0,DC06-5-3-1408070913-0,DF04-3-3-1408070913-0,DF12-3-0-1408240135-0,^,^,^,CZ83-4-0-1408020034-1,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^ DA02,DB01,DF01,DR01,^,^,^,^ DA02,DB01,DF01,DQ01,^,^,^,^ DA02,DB01,DG05,DN01,^,^,^,^ DA02,I002,I003,DN01,^,^,^,^ DA02,DB01,DJ01,DL01,^,^,^,^
            newPacket(30995);
            addBlock(1);
            addBlock(itemcode);
            addBlock((byte)wonType);
            addBlock(usr.dinar);
            addBlock(usr.cash);
            addBlock(Inventory.Itemlist(usr));
            for (int i = 0; i < 5; i++)
            {
                addBlock(usr.GetEquipment(i));
            }
        }
    }
}
