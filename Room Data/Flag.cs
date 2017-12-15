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
    class RoomHandler_Flag : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (!room.gameactive) return;
            int flagId = int.Parse(getBlock(6));
            if (room.MapData != null)
            {
                if (flagId == room.MapData.derb || flagId == room.MapData.niu) return;
            }
            int flagSide = room.flags[flagId];
            int mySide = room.GetSide(usr);

            Log.WriteInfo(mySide + " -> taking " + flagId + " [" + flagSide + "]");

            if (flagSide == mySide) return;

            bool RemovePoints = (room.rounds > 2);

            if (flagSide == (int)Room.Side.Neutral) // Neutral
            {
                room.flags[flagId] = mySide;
                if (usr.rFlags < Configs.Server.Experience.MaxFlags) usr.rPoints += Configs.Server.Experience.OnTakeFlag;
                usr.rFlags++;
            }
            else
            {
                room.flags[flagId] = (int)Room.Side.Neutral; // Make it neutral before acquiring it
            }

            if (room.mode != 8) //>--- NO TotalWar 
            {
                if (RemovePoints)
                {
                    switch (mySide)
                    {
                        case 0: room.KillsNIULeft--; break;
                        case 1: room.KillsDerbaranLeft--; break;
                    }
                }
            }
            else
            {
                int totalWarPoints = (flagId == 8 ? 30 : 15); //>--- La bandera 8 es la del centro ???
                usr.TotalWarPoint += totalWarPoints;
            }

            sendBlocks[6] = flagId; // Flag ID
            sendBlocks[7] = room.flags[flagId]; // Actual State
            sendBlocks[8] = flagSide; // Old State
            sendBlocks[9] = flagId; // Flag ID
            sendBlocks[11] = (room.mode == 8 ? usr.TotalWarPoint : 0); //>--- TotalWar 

            room.updateTime();

            /* Important */

            sendPacket = true;
        }
    }
}
