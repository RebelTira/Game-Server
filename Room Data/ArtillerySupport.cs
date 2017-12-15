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

using Game_Server.Game;

namespace Game_Server.Room_Data
{
    class RoomHandler_ArtillerySupport : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (room.channel != 2 || usr.Class != 2 || !usr.HasItem("DX01")) return; //>--- 2=Sniper   DX01=DS_TELESCOPE

            room.send(new SP_Unknown(30000, 1, usr.roomslot, room.id, 2, 159, 0, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, sendBlocks[19], sendBlocks[20], sendBlocks[21], 0, 0, 0, 0, 0, "$"));

            /* Important */

            sendPacket = true;
        }
    }
}
