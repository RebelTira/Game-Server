using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_KillCount : Packet
    {
        internal enum ActionType
        {
            Hide = 0,
            Show
        }

        public SP_KillCount(ActionType t)
        {
            newPacket(45656);
            addBlock((int)t);
            addBlock(0);
        }

        public SP_KillCount(ActionType t, int kills)
        {
            newPacket(45656);
            addBlock((int)t);
            addBlock(kills);
        }
    }
}
