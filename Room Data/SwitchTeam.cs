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
    class RoomHandler_SwitchTeam : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (usr.spectating || room.gameactive) return;

            int before = usr.roomslot;
            int after = room.SwitchSide(usr);
            if (before == after || room.type == 1) return;

            if (!room.gameactive && room.users.Values.Where(r => r.playing == false).Count() < room.users.Count)
            {
                usr.send(new Game.SP_Chat(usr, Game.SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> There is still someone in game, you must wait that everyone is back in lobby!", 999, Configs.Server.SystemName));
                return;
            }

            usr.roomslot = after;

            User u = null;
            room.users.TryRemove(before, out u);
            room.users.TryAdd(after, usr);

            sendBlocks[7] = usr.roomslot;
            sendBlocks[8] = room.master;
            
            /* Important */

            sendPacket = true;
        }
    }
}
