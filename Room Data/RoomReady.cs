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
    class RoomHandler_RoomReady : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            sendBlocks[3] = (int)Subtype.ServerRoomReady;
            sendBlocks[6] = "3";
            sendBlocks[7] = "882";
            sendBlocks[8] = "0";
            sendBlocks[9] = "1";

            if (room.mode == 8 && room.channel == 2) //>--- TotalWar, Deathmach 
            {
                sendBlocks[10] = room.kills;
                sendBlocks[11] = "20";
                usr.TotalWarPoint = 20;
            }
            //Log.WriteInfo(">---RoomReady-36 room.channel: " + room.channel + ", room.mode: " + room.mode);
            /* Important */
            //Log.WriteInfo(">---RoomReady- send CustomCRCCheck 45820-PKT ");
            usr.send(new Game.SP_CustomCRCCheck());

            sendPacket = true;
        }
    }
}
