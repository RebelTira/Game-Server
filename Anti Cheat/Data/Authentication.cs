using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Anti_Cheat.Data
{
    class CP_Authentication : Structure.Handler
    {
        public override void Handle(Client usr)
        {
            Log.WriteDebug("Received authentication packet from session " + usr.sessionId);
            usr.send(new Data.SP_Connect(usr));
        }
    }
}
