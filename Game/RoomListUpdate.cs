using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_RoomListUpdate : Packet
    {
        public SP_RoomListUpdate(Room room, int exist = 1)
        {
            //29200 76 1 76 2 2 0 Aufgeht's! 0 8 2 43 0 0 2 2 3 1 0 1 0 0 0 1 0 30 0 -1
            //29200 12 1 16 8 2 4 5 TUFAOE 0 16 13 22 4 2 3 0 3 0 0 2 11110 0 0 0 0 1 0 0 -1
            //Log.WriteInfo(">---RoomListUpdt-29200-PKT ");
            int p = room.ch.roomToPageCount - 1;
            p = p < 0 ? 0 : p;
            newPacket(29200);
            addBlock(room.id);
            addBlock(exist);
            addBlock(p);
            if (exist != 2)
            {
                addRoomInfo(room);
            }
        }
    }
}