using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Managers;

namespace Game_Server.Game
{
    class CP_RoomInvite : Handler
    {
        public override void Handle(User usr)
        {
            if (usr.room != null)
            {
                if (usr.room.users.Count < usr.room.maxusers)
                {
                    string Nickname = getBlock(0);
                    string Message = getBlock(1);

                    if (Nickname == "NULL") // Send Random User
                    {
                        byte[] buffer = (new SP_RoomInvite(usr, Message)).GetBytes();

                        User ServerUser = UserManager.GetRandomUser();
                        if (ServerUser != null)
                        {
                            if (ServerUser.room == null && ServerUser.channel == usr.channel && ServerUser.userId != usr.userId)
                            {
                                ServerUser.sendBuffer(buffer);
                                usr.sendBuffer(buffer);
                            }
                        }
                    }
                    else
                    {
                        byte[] buffer = (new SP_RoomInvite(usr, Message)).GetBytes();

                        User ServerUser = UserManager.GetUser(Nickname);
                        if (ServerUser != null)
                        {
                            if (ServerUser.room == null)
                            {
                                if (ServerUser.channel == usr.channel)
                                {
                                    ServerUser.sendBuffer(buffer);
                                    usr.sendBuffer(buffer);
                                }
                            }
                            else
                            {
                                usr.send(new SP_RoomInvite(SP_RoomInvite.ErrorCodes.IsPlaying));
                            }
                        }
                        else
                        {
                            usr.send(new SP_RoomInvite(SP_RoomInvite.ErrorCodes.GenericError));
                        }
                    }
                }
            }
        }
    }

    class SP_RoomInvite : Packet
    {
        internal enum ErrorCodes
        {
            GenericError = 93020,
            IsPlaying = 93030,
        }

        public SP_RoomInvite(ErrorCodes ErrCode)
        {
            newPacket(29520);
            addBlock((int)ErrCode);
        }

        public SP_RoomInvite(User usr, string Message)
        {
            //29520 1 0 -1 13949431 19 gn0m3x -1 -1 -1 -1 0 19 68066 0 0 LassunseineRundezusammenspielen.Kommrein 3 NULL 
            //29520 1 0 -1 20475287 52 Relity -1 -1 -1 -1 0 1 0 399 3 7 -1 Let'splayaroundtogether.Comein!!! 115 NULL
            //29520 1 0 -1 23827334 72 Relity -1 -1 -1 -1 1 1 0 274 0 1 -1 Giochiamounroundinsieme.Venite!!! 1 4 asdfasdf
            newPacket(29520);
            addBlock(1);
            addBlock(0);
            addBlock(-1);
            addBlock(usr.userId);
            addBlock(usr.sessionId);
            addBlock(usr.nickname);
            // Clan
            addBlock((usr.clan != null ? usr.clan.id : 0));
            addBlock((usr.clan != null && !usr.clanPending ? usr.clan.iconid : 0));
            addBlock((usr.clan != null ? usr.clan.name : "NULL"));
            addBlock(-1);
            addBlock((usr.clan != null && !usr.clanPending ? usr.clan.clanRank(usr) : 0));
            // End Clan
            addBlock(1);
            addBlock(0);
            addBlock(usr.exp);
            addBlock(0);
            addBlock(1);
            addBlock(-1);
            addBlock(Message);
            addBlock(1);
            addBlock(usr.room.id);
            addBlock(usr.room.password);
        }
    }
}
