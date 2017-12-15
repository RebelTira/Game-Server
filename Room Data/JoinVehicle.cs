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
    class RoomHandler_JoinVehicle : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (!room.gameactive) return;

            int vehicleId = int.Parse(getBlock(6));
            Vehicle vehicle = room.GetVehicleByID(vehicleId);

            if (vehicle == null || usr.currentVehicle != null || vehicle.Seats.Count < 1 || vehicle.Side != room.GetSide(usr) && vehicle.Side != -1 || vehicle.Health <= 0 || usr.Health <= 0 || !usr.IsAlive() || !vehicle.isJoinable) return;

            usr.currentVehicle = vehicle;
            vehicle.TimeWithoutOwner = 0;

            vehicle.Join(usr);

            sendBlocks[6] = vehicle.ID;
            sendBlocks[7] = vehicle.GetSeatByUser(usr).ID;
            sendBlocks[8] = vehicle.Health;
            sendBlocks[9] = vehicle.MaxHealth;
            sendBlocks[10] = usr.currentSeat.MainCT;
            sendBlocks[11] = usr.currentSeat.MainCTMag;
            sendBlocks[12] = usr.currentSeat.SubCT;
            sendBlocks[13] = usr.currentSeat.SubCTMag;

            /* Important */

            sendPacket = true;
        }
    }
}
