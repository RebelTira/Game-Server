using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_CostumeEquip : Packet
    {
        internal enum ErrCode
        {
            CannotDeleteDefaultItem = 97092
        }
        public SP_CostumeEquip(ErrCode code)
        {
            newPacket(29972);
            addBlock((int)code);
        }
        public SP_CostumeEquip(int Class, string Code)
        {
            newPacket(29971);
            addBlock(1);
            addBlock(Class);
            addBlock(Code);
        }
    }

    class CP_CostumeEquip : Handler
    {
        string getDefaultClass(int Class)
        {
            if (Class >= 0 && Class <= 4)
            {
                return "BA0" + (Class + 1);
            }
            return null;
        }

        public override void Handle(User usr)
        {
            if (usr.room != null) return;
            bool Equip = (getBlock(0) == "0");
            int Class = int.Parse(getBlock(1));
            string Code = getBlock(4);
            int WhereToPlace = int.Parse(getBlock(5));

            Managers.Item Item = Managers.ItemManager.GetItem(Code);
            if (Item != null && Class >= 0 && Class <= 4)
            {
                if (usr.HasCostume(Code))
                {
                    if (Code.StartsWith("BA"))
                    {
                        usr.costumes_char[Class] = (Equip ? Code : getDefaultClass(Class)) + ",^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^";
                    }
                    else
                    {
                        string[] Placement = usr.costumes_char[Class].Split(new char[] { ',' });
                        Placement[WhereToPlace] = (Equip ? Code : "^");
                        usr.costumes_char[Class] = string.Join(",", Placement);
                    }

                    string output = usr.costumes_char[Class];

                    usr.send(new SP_CostumeEquip(Class, output));
                    DB.RunQuery("UPDATE users_costumes SET class_" + Class + "='" + output + "' WHERE ownerid='" + usr.userId + "'");
                    usr.send(new SP_CashItemBuy(usr));
                }
                else
                {
                    Log.WriteError(usr.nickname + " tried to equip " + Code + " but he hasn't it!");
                    usr.disconnect();
                }
            }
        }
    }
}