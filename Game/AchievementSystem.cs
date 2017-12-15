using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class CP_AchievementSystem : Handler
    {
        public override void Handle(User usr)
        {
            /* TO DO */
            usr.send(new SP_CustomMessageBox("The achievement system isn't available yet."));
        }
    }
}
