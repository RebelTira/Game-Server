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
    class RoomHandler_RoomStart : RoomDataHandler 
    {
        public override void Handle(User usr, Room room)
        {
            Log.WriteInfo(">---RoomStart-26  timestamp: " + Generic.timestamp);
            if (usr.LastStartTick >= Generic.timestamp || room.gameactive) return;

            if (!room.gameactive && room.users.Values.Where(r => r.playing == false).Count() < room.users.Count)
            {
                usr.send(new Game.SP_Chat(usr, Game.SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> There is still someone in game, you must wait that everyone is back in lobby!", 999, Configs.Server.SystemName));
                return;
            }

            usr.LastStartTick = Generic.timestamp + 0.10;

            if (room.master == usr.roomslot)
            {

                int derb = room.SideCountDerb;
                int niu = room.SideCountNIU;
                if (room.isPremMap(room.mapid) && usr.premium < 1)
                {
                    usr.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> You cannot start a premium map as free user!", 999, "NULL"));
                    return;
                }
                else if (room.type == 1)
                {
                    if (room.GetSideCount(0) != room.GetSideCount(1))
                    {
                        room.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> Teams need to be balanced.", 998, "NULL"));
                        return;
                    }
                }
                //>--- No Start si... (( Users <= 1, o teams no balanceados, o un team vacio )) Y No.Zombies Y No.FFA Y No.Debug
                //else if ((room.users.Count <= 1 || (derb > niu + 1 || niu > derb + 1 || derb == 0 || niu == 0)) && (usr.channel != 3 && room.mode != 1) && !Configs.Server.Debug)
                else if ((room.users.Count <= 1 || (derb > niu + 1 || niu > derb + 1 || derb == 0 || niu == 0)) && (usr.channel != 3 && room.mode != 1) && !usr.AloneMode)
                {
                    usr.send(new Game.SP_Chat(usr, Game.SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> No puedes entrar en estas condiciones!!", 999, Configs.Server.SystemName));
                    return;
                   
                }

                if (!room.Start())
                {
                    /* Avoid send useless packet */
                    return;
                }
                //Log.WriteInfo(">---RoomStart-65  room.Start-PKT ");
                sendBlocks[3] = (int)Subtype.ServerStart;
                sendBlocks[6] = room.mapid;
                room.status = 2;
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
