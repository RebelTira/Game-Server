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

namespace Game_Server.Room_Data
{
    class RoomHandler_WeaponSwitch : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (sendBlocks.Length < 6) return;
            if (!room.gameactive || !usr.IsAlive() || usr.Health <= 0) return;
            //if (room.mode == (int)RoomMode.FFA && room.ffa != null && room.ffa.isGunGame) return;

            usr.weapon = int.Parse(getBlock(6));
            
            /* Important */

            sendPacket = true;
        }
    }
}
