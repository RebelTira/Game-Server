using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class CP_LoginEvent : Handler
    {
        string getWeapon(int Count)
        {
            switch (Count)
            {
                case 0: return "CM06"; //>--- PX_CHARGE_5000
                case 1: return "CI01"; //>--- RELOAD_UP
                case 2: return "CD01"; //>--- EXP 30UP Set
                case 3: return "CF02"; //>--- SP_5UP_SET
                case 4: return "CC05"; //>--- UP_DOUBLE
                case 5: return "CA01"; //>--- SLOT_5TH
                case 6: return "CR16"; //>--- Attendance_Box
            }
            return "NULL";
        }
        public override void Handle(User usr)
        {
            string itemCode = getWeapon(usr.rewardEvent.progress);
            int days = 3;
            if (!usr.rewardEvent.doneToday)
            {
                usr.rewardEvent.doneToday = true;
                Managers.Item item = Managers.ItemManager.GetItem(itemCode);
                if (item.dinarReward > 0)
                {
                    usr.dinar += (int)item.dinarReward;
                    DB.RunQuery("UPDATE users SET dinar = '" + usr.dinar + "' WHERE id='" + usr.userId + "'");
                }
                else
                {
                    if (itemCode.StartsWith("B"))
                    {
                        Inventory.AddCostume(usr, itemCode, days);
                    }
                    else
                    {
                        Inventory.AddItem(usr, itemCode, days);
                    }
                }
                usr.rewardEvent.progress++;
                usr.send(new SP_LoginEvent(usr, itemCode, days));
                DB.RunQuery("UPDATE users SET loginEventProgress = '" + usr.rewardEvent.progress + "', loginEventToday = '1' WHERE id='" + usr.userId + "'");
            }
            else
            {
                usr.send(new SP_LoginEvent(SP_LoginEvent.ErrorCodes.AlreadyChecked));
            }
        }
    }

    class SP_LoginEvent : Packet
    {
        public enum ErrorCodes
        {
            Success = 1,
            AlreadyChecked = -1
        }

        public SP_LoginEvent(ErrorCodes err)
        {
            newPacket(30933);
            addBlock((int)err);
        }

        public SP_LoginEvent(User usr, string item, int days)
        {
            newPacket(30993);
            addBlock(1);
            addBlock(1);
            addBlock(usr.rewardEvent.progress);
            addBlock(item);
            addBlock(days);
            addBlock(Inventory.Itemlist(usr));
            addBlock(usr.AvailableSlots); //Slots Enabled
            addBlock(Inventory.Costumelist(usr));
            addBlock(usr.dinar);
        }
    }
}
