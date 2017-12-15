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
    class RoomHandler_ChangeVehicleSeat : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (!room.gameactive || usr.currentVehicle == null) return;

            int vehicleId = int.Parse(getBlock(6));
            int tarGetSeat = int.Parse(getBlock(7));
            Vehicle vehicle = room.GetVehicleByID(vehicleId);

            if (vehicle == null || usr.currentVehicle != vehicle || usr.currentSeat.ID == tarGetSeat || vehicle.Side != room.GetSide(usr) || !vehicle.FreeSeat(tarGetSeat)) return;

            int oldSeat = usr.currentSeat.ID;

            usr.currentSeat.MainCT = int.Parse(getBlock(8));
            usr.currentSeat.MainCTMag = int.Parse(getBlock(9));
            usr.currentSeat.SubCT = int.Parse(getBlock(10));
            usr.currentSeat.SubCTMag = int.Parse(getBlock(11));

            vehicle.SwitchSeat(tarGetSeat, usr);

            sendBlocks[6] = vehicleId;
            sendBlocks[7] = tarGetSeat;
            sendBlocks[8] = oldSeat;
            sendBlocks[9] = usr.currentSeat.MainCT;
            sendBlocks[10] = usr.currentSeat.MainCTMag;
            sendBlocks[11] = usr.currentSeat.SubCT;
            sendBlocks[12] = usr.currentSeat.SubCTMag;

            /* Important */

            sendPacket = true;
        }
    }
}
