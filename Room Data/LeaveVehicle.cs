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
    class RoomHandler_LeaveVehicle : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (!room.gameactive || usr.currentVehicle == null) return;

            int vehicleId = int.Parse(getBlock(6));
            Vehicle vehicle = room.GetVehicleByID(vehicleId);

            if (vehicle == null || usr.currentVehicle != vehicle) return;

            vehicle.TimeWithoutOwner = 0;
            
            usr.currentSeat.MainCT = int.Parse(getBlock(8));
            usr.currentSeat.MainCTMag = int.Parse(getBlock(9));
            usr.currentSeat.SubCT = int.Parse(getBlock(10));
            usr.currentSeat.SubCTMag = int.Parse(getBlock(11));

            sendBlocks[6] = vehicleId;
            sendBlocks[7] = usr.currentSeat.ID;
            vehicle.Leave(usr);

            /* Important */

            sendPacket = true;
        }
    }
}
