using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Managers;

namespace Game_Server.Game
{
    class CP_QuickJoinRoom : Handler
    {
        public override void Handle(User usr)
        {
            Channel ch = ChannelManager.channels[usr.channel];
            List<Room> Rooms = ch.GetRoomListByPage(new Random().Next(0, ch.roomToPageCount));
            foreach (Room room in Rooms)
            {
                if (room != null)
                {
                    if (usr.room != null || room.users.Count >= room.maxusers || room.enablepassword == 1 || room.type == 1 || !room.isJoinable || room.voteKick.lockuser.IsLockedUser(usr))
                    {
                        /* Room cannot be joined */
                        return;
                    }

                    bool levelLimit = (usr.level >= (10 * (room.levellimit - 1)) + 1 || usr.level <= 10 && room.levellimit == 1 || room.levellimit == 0);

                    if (levelLimit)
                    {
                        /* Ping & Level Limit */
                        return;
                    }

                    if (room.JoinUser(usr, 2))
                    {
                        //Log.WriteInfo(">---JoinRoom-37 room.JoinUser(usr), (room) " + usr + ", " + room);
                        room.InitializeUDP(usr);
                        room.ch.UpdateLobby(room);
                        Managers.UserManager.UpdateUserlist(usr);
                        break;
                    }
                }
            }
        }
    }
}
