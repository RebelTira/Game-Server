using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Game_Server.Room_Data
{
    class RoomHandler_CaptureModeRequest : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (!room.gameactive || room.capturemode == null || usr.currentVehicle == null) return;
            
            if (usr.currentVehicle.Code == "ED13") //>--- ATV-C
            {
                sendBlocks[3] = (int)Subtype.CaptureModeResponse;
                sendBlocks[8] = 7; // ??
                int side = int.Parse(getBlock(9));
                if (room.GetSide(usr) == (int)Room.Side.NIU && side == (int)Room.Side.NIU)
                {
                    room.capturemode.NIUPoints += 20;
                }
                else
                {
                    room.capturemode.DerbaranPoints += 20;
                }
                usr.rPoints += 50;
                usr.rAssist++;
            }
        }
    }
}
