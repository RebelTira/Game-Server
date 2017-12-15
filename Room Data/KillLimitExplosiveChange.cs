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
    class RoomHandler_KillLimitExplosiveChange : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (room.master == usr.roomslot && !room.gameactive)
            {
                lobbychanges = true;
                if (room.mode == 1) //>--- FFA
                {
                    if (usr.premium > 0)
                    {
                        room.rounds = int.Parse(getBlock(6));
                    }
                    else
                    {
                        room.rounds = 0;
                    }
                }
                else
                {
                    room.rounds = int.Parse(getBlock(6));
                }
            }
            else
            {
                usr.disconnect();
            }

            /* Important */

            sendPacket = true;
        }
    }
}
