using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Managers;

namespace Game_Server.Game
{
    class CP_Spectate : Handler
    {
        internal enum Type
        {
            LeaveRoom = 0,
            JoinRoom = 1
        }

        public override void Handle(User usr)
        {
            if (usr.rank > 2 && usr.channel != -1)
            {
                int b = int.Parse(getBlock(0));
                Type subtype = (Type)b;
                switch (subtype)
                {
                    case Type.JoinRoom:
                        {
                            int roomId = int.Parse(getBlock(1));

                            Room room = ChannelManager.channels[usr.channel].GetRoom(roomId);
                            if (room != null)
                            {
                                if (room.AddSpectator(usr))
                                {
                                    usr.send(new SP_Spectate(usr, room));
                                    room.InitializeSpectatorUDP(usr);
                                }
                                else
                                {
                                    usr.send(new SP_Chat("SPECTATE", SP_Chat.ChatType.Room_ToAll, "SPECTATE >> There is no empty slot for this room!", 999, usr.nickname));
                                }
                            }
                            break;
                        }
                    case Type.LeaveRoom:
                        {
                            if (usr.room != null)
                            {
                                usr.room.RemoveSpectator(usr);
                            }
                            usr.lobbypage = 0;
                            usr.send(new SP_RoomList(usr, usr.lobbypage, false));
                            break;
                        }
                }
            }
            else
            {
                /* Fake administrator */
                Log.WriteError(usr.nickname + " tried to spectate (Rank: " + usr.rank + " - AccessLevel: " + usr.accesslevel + ")");
                usr.disconnect();
            }
        }
    }

    class SP_Spectate : Packet
    {
        public SP_Spectate() // Leave
        {
            newPacket(29488);
            addBlock(0);
        }

        public SP_Spectate(User usr, Room room) // Join
        {
            newPacket(29488);
            addBlock(1);
            addBlock(1);
            addBlock(usr.roomslot); // Spectator ID
            addRoomInfo(room);
        }
    }
}
