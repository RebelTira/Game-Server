using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Managers;

namespace Game_Server.Game
{
    class CP_UserList : Handler
    {
        public override void Handle(User usr)
        {
            SP_UserList.Type type = (SP_UserList.Type)int.Parse(getBlock(0));
            usr.actualUserlistType = (int)type;
            List<User> usersInLobby = new List<User>();
            switch (type)
            {
                case SP_UserList.Type.Friends: // Friends
                    usr.RefreshFriends();
                    return;
                case SP_UserList.Type.Clan: // Clan
                    if (usr.clan != null)
                    {
                        usersInLobby = usr.clan.Users.Values.ToList();
                    }
                    break;
                case SP_UserList.Type.Wait: // Wait (In lobby)
                    usersInLobby = Managers.UserManager.ServerUsers.Values.Where(u => u.channel == usr.channel && u.room == null).ToList();
                    break; // point
                default:
                    usr.disconnect();
                    break;
            }
            usr.send(new SP_UserList(type, usersInLobby));
        }
    }

    class SP_UserList : Packet
    {
        internal enum Type
        {
            Friends,
            Clan,
            Wait
        }
        public SP_UserList(Type type, List<User> users)
        {
            newPacket(28928);
            addBlock(1);
            addBlock((int)type);
            addBlock(users.Count);
            foreach (User usr in users)
            {
                //23154251 1733 35821 20011002 DutchYoungGamers 0 Liiisten 45 1064 -1 3 1 1 -1 
                addBlock(usr.userId);
                addBlock(usr.sessionId);
                addBlock((usr.clan != null ? usr.clan.id : -1)); // Clan ID
                
                if (usr.clan != null) addBlock(usr.clan.iconid); else addBlock(-1); // Clan Icon ID

                addBlock((usr.clan != null ? usr.clan.name : "NULL")); // Clan Name
                addBlock(4);
                addBlock(usr.nickname);
                addBlock(usr.level);
                addBlock(0); // Medal ID
                addBlock(usr.HasSmileBadge);
                addBlock(usr.premium);
                addBlock(1);
                addBlock(usr.channel);
                addBlock((usr.room != null ? usr.room.id : -1));
            }
        }
    }
}