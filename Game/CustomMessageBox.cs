using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_CustomMessageBox : Packet
    {
        public SP_CustomMessageBox(string message)
        {
            newPacket(25820);
            addBlock(0); // Error code - Success
            addBlock(message);
        }
    }
}
