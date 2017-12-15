using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Managers;

namespace Game_Server.Game
{
    class CP_JoinRoom : Handler
    {
        public override void Handle(User usr)
        {
            if (usr.room != null) { usr.disconnect(); return; }

            int roomId = int.Parse(getBlock(0));
            string password = getBlock(1);
            int side = int.Parse(getBlock(2)); //>---  0 Derb - 1 NIU - 2 Random

            Room room = ChannelManager.channels[usr.channel].GetRoom(roomId);
            try
            {
                if (room != null)
                {
                    if (room.isJoinable)
                    {
                        if (!room.EndGamefreeze)
                        {
                            bool levelLimit = (usr.level >= (10 * (room.levellimit - 1)) + 1 || usr.level <= 10 && room.levellimit == 1 || room.levellimit == 0);

                            int clanrank = (usr.clan != null ? usr.clan.clanRank(usr) : 9);

                            if (!levelLimit)
                            {
                                /* Room level limit restriction*/
                                usr.send(new SP_JoinRoom(SP_JoinRoom.ErrorCodes.BadLevel));
                            }
                            else if (room.enablepassword == 1 && (password != room.password))
                            {
                                /* Invalid password */
                                usr.send(new SP_JoinRoom(SP_JoinRoom.ErrorCodes.InvalidPassword));
                            }
                            else if (usr.premium < 1 && room.premiumonly == 1)
                            {
                                /* Premium only room */
                                usr.send(new SP_JoinRoom(SP_JoinRoom.ErrorCodes.OnlyPremium));
                            }
                            else if (room.type == 1 && (clanrank == -1 || clanrank == 9))
                            {
                                /* Clan War and user is not yet member - Generic error */
                                usr.send(new SP_JoinRoom(SP_JoinRoom.ErrorCodes.GenericError));
                            }
                            else if (room.type == 1 && (room.SideCountDerb > 0 && room.SideCountNIU > 0 && !room.isMyClan(usr) || usr.clan == null))
                            {
                                /* Clan War and i'm not a member of both clan - Generic error */
                                usr.send(new SP_JoinRoom(SP_JoinRoom.ErrorCodes.GenericError));
                            }
                            else if (room.users.Count >= room.maxusers || (room.userlimit && usr.rank < 3))
                            {
                                /* Generic error */
                                usr.send(new SP_JoinRoom(SP_JoinRoom.ErrorCodes.GenericError));
                            }
                            else
                            {
                                if (room.JoinUser(usr, side))
                                {
                                    Log.WriteInfo(">---JoinRoom-70 room.JoinUser(usr, side) " + usr +", " + side);
                                    room.ch.UpdateLobby(room);
                                    room.InitializeUDP(usr);
                                    Managers.UserManager.UpdateUserlist(usr);
                                }
                                else
                                {
                                    /* Generic error */
                                    usr.send(new SP_JoinRoom(SP_JoinRoom.ErrorCodes.GenericError));
                                }
                            }
                        }
                        else
                        {
                            /* Avoid join while game end is sent */
                            usr.send(new SP_JoinRoom(SP_JoinRoom.ErrorCodes.GameIsEnding));
                        }
                    }
                    else
                    {
                        if (room.isGameEnding && room.gameactive)
                        {
                            /* Avoid join game is ending */
                            usr.send(new SP_JoinRoom(SP_JoinRoom.ErrorCodes.GameIsEnding));
                        }
                        else
                        {
                            /* Generic error */
                            usr.send(new SP_JoinRoom(SP_JoinRoom.ErrorCodes.GenericError));
                        }
                    }
                }
                else
                {
                    /* Generic error */
                    usr.send(new SP_JoinRoom(SP_JoinRoom.ErrorCodes.GenericError));
                }
            }
            catch (Exception ex)
            {
                Log.WriteError("Cannot join in the room:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                usr.send(new SP_JoinRoom(SP_JoinRoom.ErrorCodes.GenericError));
            }
        }
    }

    class SP_JoinRoom : Packet
    {
        internal enum ErrorCodes
        {
            GenericError = 94010,
            InvalidPassword = 94030,
            BadLevel = 94300,
            OnlyPremium = 94301,
            GameIsEnding = 94120
        }

        public SP_JoinRoom(User usr, Room r)
        {
            newPacket(29456);
            addBlock(1);
            addBlock(usr.roomslot);
            addRoomInfo(r);
        }

        public SP_JoinRoom(ErrorCodes ErrCode)
        {
            newPacket(29456);
            addBlock((int)ErrCode);
        }
    }
}