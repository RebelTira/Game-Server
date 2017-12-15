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

using Game_Server.Game;

namespace Game_Server.Room_Data
{
    class RoomHandler_WorldDamage : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            int fallType = int.Parse(getBlock(6));
            int Damage = int.Parse(getBlock(9));
            if (usr.Health <= 0 || !usr.IsAlive()) return;
            if (!room.gameactive || Damage <= 0) { sendPacket = false; return; };

            if (fallType == 1)
            {
                usr.Health -= Damage; //>--- Daño en caidas

                if (usr.Health <= 0)
                {
                    sendBlocks[3] = (int)Subtype.Suicide;
                    sendBlocks[6] = usr.roomslot;
                    usr.OnDie();
                }
                else
                {
                    sendBlocks[11] = Damage;
                    sendBlocks[12] = usr.Health;
                }
            }
            else
            {
                int vehicleId = int.Parse(getBlock(8));
                bool underWater = int.Parse(getBlock(10)) == 1;

                Vehicle vehicle = room.GetVehicleByID(vehicleId);

                if (vehicle == null) { sendPacket = false; return; };

                Damage = (int)Math.Ceiling((decimal)(vehicle.MaxHealth * (int.Parse(getBlock(9)) / int.Parse(getBlock(10)))) / 100);

                if (underWater)
                {
                    Damage = (int)Math.Truncate((double)(vehicle.MaxHealth * 60) / 100);
                }
                //if (usr.GodMode) { Damage = 0; }  //>---  +++ 
                vehicle.Health -= Damage;

                sendBlocks[9] = Damage;
                sendBlocks[11] = Damage;
                sendBlocks[12] = vehicle.Health;

                if (vehicle.Health <= 0)
                {
                    foreach (VehicleSeat Seat in vehicle.Seats.Values)
                    {
                        if (Seat.seatOwner != null)
                        {
                            if (Seat.seatOwner.Health <= 0) { sendPacket = false; return; };
                            Seat.seatOwner.OnDie();
                            room.send(new SP_RoomData(usr.roomslot, room.id, 2, 157, 0, 1, usr.roomslot, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "$"));
                        }
                    }
                    room.updateTime();
                    room.send(new SP_RoomVehicleExplode(room.id, vehicleId, usr.roomslot));
                    vehicle.Health = 0;
                    return;
                }
            }

            /* Important */

            sendPacket = true;
        }
    }
}
