using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class CP_RoomKick : Handler
    {
        public override void Handle(User usr)
        {
            if (usr.room != null && usr != null)
            {
                if (usr.room.master == usr.roomslot)
                {
                    byte Slot = byte.Parse(getBlock(0));
                    User target = usr.room.GetUser(Slot);
                    if (target != null)
                    {
                        target.send(new SP_RoomKick(Slot));
                    }
                }
            }
        }
    }

    class SP_RoomKick : Packet
    {
        public SP_RoomKick(int slot)
        {
            newPacket(29505); // Packet ID
            addBlock(1); // Error Code = 1 = Success
            addBlock(slot); // Slot
        }
    }
}
