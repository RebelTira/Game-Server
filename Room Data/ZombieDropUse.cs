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
    class RoomHandler_ZombieDropUse : RoomDataHandler
    {
        internal enum DropType
        {
            Respawn = 0,
            Medic = 1,
            Ammo = 2,
            Repair = 3
        }

        public override void Handle(User usr, Room room)
        {
            if (!room.gameactive && room.channel != 3) return;

            DropType DropType = (DropType)(int.Parse(getBlock(7)));

            switch (DropType)
            {
                case DropType.Respawn:
                    {
                        break;
                    }
                case DropType.Medic:
                    {
                        usr.Health = 1000;
                        sendBlocks[10] = usr.Health;
                        break;
                    }
                case DropType.Ammo:
                    {
                        break;
                    }
                case DropType.Repair:
                    {
                        int vehicleId = room.GetIncubatorVehicleId();
                        Vehicle Vehicle = room.GetVehicleByID(vehicleId);
                        if (Vehicle != null)
                        {
                            Vehicle.Health += 10000;

                            if (Vehicle.Health > Vehicle.MaxHealth)
                                Vehicle.Health = Vehicle.MaxHealth + 1;

                            sendBlocks[10] = Vehicle.Health;
                            sendBlocks[11] = Vehicle.MaxHealth;
                        }
                        break;
                    }
                default:
                    {
                        Log.WriteError("Unknown Zombie Drop ID: " + int.Parse(getBlock(7)));
                        break; // Unknown
                    }
            }

            room.DropID--;

            /* Important */

            sendPacket = true;
        }
    }
}
