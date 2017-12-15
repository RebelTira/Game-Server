/*
 _____   ___ __  __  _____ _____   ___  _  __              ___   ___   __    __ 
/__   \ /___\\ \/ /  \_   \\_   \ / __\( )/ _\            / __\ /___\ /__\  /__\
  / /\///  // \  /    / /\/ / /\// /   |/ \ \            / /   //  /// \// /_\  
 / /  / \_//  /  \ /\/ /_/\/ /_ / /___    _\ \          / /___/ \_/// _  \//__  
 \/   \___/  /_/\_\\____/\____/ \____/    \__/          \____/\___/ \/ \_/\__/  
__________________________________________________________________________________

Created by: ToXiiC
Thanks to: CodeDragon, Kill1212, CodeDragon

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.GameModes
{
    class TotalWar
    {
        ~TotalWar()
        {
            GC.Collect();
        }
        Room room = null;

        public TotalWar(Room room)
        {
            this.room = room;
        }

        public void Update()
        {
            if (room != null)
            {
                if (room.timeleft <= 0 || (room.TotalWarDerb >= room.kills || room.TotalWarNIU >= room.kills)) { room.EndGame(); return; } //>--- Asi, funciona el spawn. 
                //if (room.timeleft <= 0 || (room.TotalWarDerb >= room.kills || room.TotalWarNIU >= room.kills) || room.KillsNIULeft <= 0 || room.KillsDerbaranLeft <= 0) { room.EndGame(); return; }

                //>--- Falta por terminar

            }
        }
    }
}
