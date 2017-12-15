using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Room_Data
{
    internal class SP_EntitySuicide : Packet
    {
        internal enum SuicideType
        {
            Suicide = 0,
            KilledByNotHavinHealTreatment = 1,

        }
        public SP_EntitySuicide(int slotId, SuicideType type = SuicideType.Suicide, bool outofworld = false)
        {
            newPacket(30000);
            addBlock(1);
            addBlock(slotId);
            addBlock(-1);
            addBlock(2);
            addBlock((int)Subtype.Suicide);
            addBlock(0);
            addBlock((int)type);
            addBlock((outofworld ? 2 : slotId));
            Fill(0, 7);
        }
    }
}
