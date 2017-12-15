using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Managers
{
    class CP_RoomInviteOrQuickJoin : Handler
    {
        public override void Handle(User usr)
        {
            int channel = int.Parse(getBlock(0));
            int roomId = int.Parse(getBlock(1));
            string password = "NULL";
            
            if(getAllBlocks.Length >= 2)
            {
                password = getBlock(2);
            }

            Room room = Managers.ChannelManager.channels[usr.channel].GetRoom(roomId);

            if (room != null)
            {
                if (usr.room != null || room.users.Count >= room.maxusers || room.type == 1 || !room.isJoinable || room.voteKick.lockuser.IsLockedUser(usr))
                {
                    /* Room cannot be joined */
                    return;
                }

                if (room.enablepassword == 0 || room.enablepassword == 1 && room.password == password)
                {
                    bool levelLimit = (usr.level >= (10 * (room.levellimit - 1)) + 1 || usr.level <= 10 && room.levellimit == 1 || room.levellimit == 0);

                    if (!levelLimit)
                    {
                        /* Ping & Level Limit */
                        return;
                    }

                    if (room.JoinUser(usr, 2))
                    {
                        room.InitializeUDP(usr);
                        room.ch.UpdateLobby(room);
                        Managers.UserManager.UpdateUserlist(usr);
                    }
                }
            }
        }
    }
}
