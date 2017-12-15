using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server
{
    class CP_ClanRanking : Handler
    {
        public override void Handle(User usr)
        {
            if (usr.room != null) return;

            if (ClanRanking.LastUpdate != DateTime.Now.Hour)
                ClanRanking.refreshclans();

            usr.send(new SP_ClanRanking());
        }
    }

    class SP_ClanRanking : Packet
    {
        public SP_ClanRanking()
        {
            newPacket(26464);
            addBlock(1);
            addBlock(DateTime.Now.Hour + 1); // Refresh Time
            addBlock(ClanRanking.clans.Count);
            foreach (Clan c in ClanRanking.clans.Values.Take(30))
            {
                if (c != null)
                {
                    addBlock(c.iconid);
                    addBlock(c.name);
                    addBlock(c.exp);
                    addBlock(c.ClanUsers.Count);
                    addBlock(c.maxUsers);
                }
            }
        }
    }
}
