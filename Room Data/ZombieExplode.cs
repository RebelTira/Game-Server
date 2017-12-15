using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Room_Data
{
    class RoomHandler_ZombieExplode : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            /* 30000 1 0 1 2 900 0 0 1 0 7 0 0 0 0 0 
               30000 1 7 1 2 157 0 0 7 0 0 0 0 0 0 0 */

            int slot = int.Parse(getBlock(8));
            Zombie z = room.GetZombieByID(slot);

            if (z.Name != "Breaker") room.send(new SP_EntitySuicide(slot)); //> -- +++ Se suicidaba pero no contaba como kill
            if (z != null && z.Health > 0 && z.Name != "Breaker") //> -- +++

                if (z != null && z.Health > 0)
            {
                z.Health = 0;
                z.respawn = Generic.timestamp + 4;
                room.KilledZombies++;
            }

            /* Important */
            sendPacket = true;
        }
    }
}
