using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_Unknown : Packet
    {
        public SP_Unknown(ushort packetId, params object[] Params)
        {
            //Log.WriteInfo(">---UnknowPacket-12  Params: " + Params.ToList());
            //string m = "";
            //m = Params.ToString();
            //Log.WriteInfo(">---UnknowPacket-15  packetId: " + packetId + " Params: " + m);
            newPacket(packetId);
            Params.ToList().ForEach(p => { addBlock(p); });
            //Log.WriteInfo(">---UnknowPacket-19  Params.ToList(): " + Params.ToList());
        }
    }
}
