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

namespace Game_Server.Room_Data
{
    class RoomHandler_PlacementUse : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (!room.gameactive || !usr.IsAlive() || usr.Health <= 0) return;
            int plantingId = int.Parse(getBlock(8));
            string item = getBlock(27);

            /* Todo - land status for disable land*/
            
            if (!room.Placements.ContainsKey(plantingId)) return;

            Placement placement = room.getPlacement(plantingId);
            if (placement.Used) return;

            User planter = room.getPlacementOwner(plantingId);
            if (planter == null) return;

            int planterSide = room.GetSide(planter);
            int mySide = room.GetSide(usr);

            switch (item)
            {
                case "DV01": // Medic Box
                    {
                        usr.Health += 500;
                        if (planter != null)
                        {
                            if (usr.Equals(planter) == false && mySide == planterSide && planter.droppedMedicBox < 10)
                            {
                                planter.droppedMedicBox++;
                                planter.rPoints += Configs.Server.Experience.OnNormalPlaceUse;
                            }
                        }
                        break;
                    }
                case "DU01": // Ammo Box
                    {
                        if (planter != null)
                        {
                            if (usr.Equals(planter) == false && mySide == planterSide && planter.droppedAmmo < 10)
                            {
                                planter.droppedAmmo++;
                                planter.rPoints += Configs.Server.Experience.OnLandPlaceUse;
                                switch (usr.Class)
                                {
                                    case 3: // Assault
                                        {
                                            usr.throwNades = 0;
                                            break;
                                        }
                                    case 4: // Heavy
                                        {
                                            usr.throwRockets = 0;
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                    }
                case "DU02": // M14
                    {
                        usr.Health -= 100;
                        if (usr.Health < 1)
                            usr.Health = 1;
                        if (planter != null)
                        {
                            if (usr.Equals(planter) == false && mySide != planterSide && planter.droppedM14 < 8)
                            {
                                planter.droppedM14++;
                                planter.rPoints += Configs.Server.Experience.OnLandPlaceUse;
                            }
                        }
                        break;
                    }
                case "DS05": // Flash
                    {
                        if (planter != null)
                        {
                            if (usr.Equals(planter) == false && mySide != planterSide && planter.droppedFlash < 6)
                            {
                                planter.droppedFlash++;
                                planter.rPoints += Configs.Server.Experience.OnNormalPlaceUse;
                            }
                        }   
                        break;
                    }
                case "DZ01": // Heavy Ammo Box
                    {
                        if (usr.Class == 4)
                        {
                            usr.throwRockets = 0;
                        }
                        break;
                    }
            }

            if (usr.Health > 1000)
                usr.Health = 1000;

            sendBlocks[10] = usr.Health;

            room.RemovePlacement(plantingId);
            
            /* Important */

            sendPacket = true;
        }
    }
}
