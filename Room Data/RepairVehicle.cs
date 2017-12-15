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
    class RoomHandler_RepairVehicle : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (!room.gameactive) return;

            int vehicleId = int.Parse(getBlock(6));
            int tarGetSeat = int.Parse(getBlock(7));
            Vehicle vehicle = room.GetVehicleByID(vehicleId);
            if (vehicle == null || vehicle.Side != room.GetSide(usr) && vehicle.Side != -1) return;

            if (vehicle.Health >= vehicle.MaxHealth || usr.LastRepairTick > Generic.timestamp) return;

            double RepairPercentage = 0.075; // 7.5%

            string item = ItemManager.GetItemCodeByID(usr.weapon);

            switch (item)
            {
                case "DR01":
                    {
                        RepairPercentage = 0.10; // 10%
                        break;
                    }
                case "DR02":
                    {
                        RepairPercentage = 0.15; // 15%
                        break;
                    }
                case "DU51":
                    {
                        RepairPercentage = 0.25; // 25%
                        break;
                    }
            }

            int repair = (int)Math.Truncate(vehicle.MaxHealth * RepairPercentage);

            vehicle.Health += repair;
            if (vehicle.Health > vehicle.MaxHealth) vehicle.Health = vehicle.MaxHealth;

            usr.LastRepairTick = Generic.timestamp + 2;

            sendBlocks[7] = vehicle.Health;
            sendBlocks[8] = vehicle.MaxHealth;

            /* Important */

            sendPacket = true;
        }
    }
}
