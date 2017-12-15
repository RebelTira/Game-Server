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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Managers;

namespace Game_Server.Room_Data
{
    class RoomHandler_Heal : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            byte targetSlot = byte.Parse(getBlock(6));
            bool BoxStation = (getBlock(7) == "1" ? true : false);
            bool ToHeal = ((getBlock(8) == "0" || getBlock(8) == "2") ? true : false);
            if (targetSlot >= 0 && targetSlot <= room.maxusers)
            {
                User target = room.users[targetSlot];
                if (target == null) return;
                if (ToHeal)
                {
                    string item = ItemManager.GetItemCodeByID(usr.weapon);

                    //>--- Curar excepto en FFA y al enemigo
                    if (target.Health <= 0 || target.Health >= 1000 && item != "DS01" || (room.mode != 1 && room.GetSide(usr) != room.GetSide(target))) return; 

                    if (!BoxStation)
                    {
                        if (usr.HasItem(item) || usr.IsWhitelistedWeapon(item))
                        {
                            if (target.roomslot != usr.roomslot && target.Health < 300)
                            {
                                usr.rPoints += Configs.Server.Experience.OnFriendHeal;
                            }

                            if (item == "BS0E") /* Medic Bag */
                            {
                                target.Health += 50;
                            }
                            else
                            {
                                //Log.WriteLine(item);
                                switch (item)
                                {
                                    case "DQ01": // Medic Kit 1
                                        {
                                            target.Health += 300;
                                            break;
                                        }
                                    case "DQ02": // Medic Kit 2
                                        {
                                            target.Health += 400;
                                            break;
                                        }
                                    case "DQ03": // Medic Kit 3
                                        {
                                            target.Health += 600;
                                            break;
                                        }
                                    case "DS01": // Adrenaline
                                        {
                                            int res = target.Health < 300 ? 300 : target.Health + 50;
                                            target.Health = res;
                                            break;
                                        }
                                    case "DS10": // HP Kit
                                        {
                                            target.Health += 200;
                                            break;
                                        }
                                }

                                if (item.StartsWith("DQ")) // If is a medic kit
                                {
                                    int g_perc = Generic.random(0, 500);

                                    if (g_perc < 20)
                                    {
                                        usr.RandomGunsmithResource();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        target.Health += 400;
                    }

                    if (target.Health > 1000) target.Health = 1000;
                }
                else
                {
                    target.Health -= 100; // Remove 1HP for tick!
                    if (target.Health <= 0)
                    {
                        sendBlocks[3] = (int)Subtype.Suicide;
                        target.OnDie();
                    }
                }

                sendBlocks[7] = target.Health;
            }

            /* Important */

            sendPacket = true;
        }
    }
}
