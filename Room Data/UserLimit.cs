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
    class RoomHandler_UserLimit : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (room.gameactive || usr.premium == 0) return;
            if (usr.roomslot == room.master)
            {
                room.userlimit = !room.userlimit;
                sendBlocks[6] = room.userlimit ? "1" : "0";
                lobbychanges = true;
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
