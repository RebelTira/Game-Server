using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;

using Game_Server.Game;

namespace Game_Server.Managers
{
    class UserManager
    {
        private static Thread UserRoutineThread = null;
        public static readonly ConcurrentDictionary<uint, User> ServerUsers = new ConcurrentDictionary<uint, User>();
        private static Random r = new Random();

        public static void setup()
        {
            //Log.WriteLine(ServerUsers.Length.ToString());
            UserRoutineThread = new Thread(UserRoutine);
            UserRoutineThread.Start();
        }

        private static void UserRoutine()
        {
            while (true)
            {
                foreach (User usr in ServerUsers.Values)
                {
                    if (!usr.IsConnectionAlive)
                    {
                        Log.WriteLine("Kick request due to no received packet anymore for " + usr.nickname);
                        usr.disconnect();
                        continue;
                    }
                    else if (usr.lastShoxTick + 15 < Generic.timestamp && usr.lastShoxTick > 0)
                    {
                        Log.WriteError(usr.nickname + " has been kicked out reason: No ShoxGuard Packet");
                        usr.disconnect();
                        continue;
                    }
                    //else if (usr.heartBeatTime < Generic.timestamp && usr.heartBeatTime > 0)
                    //{
                    //    Log.WriteError(usr.nickname + "has been kicked out reason: No heartbeat");
                    //    usr.disconnect();
                    //    continue;
                    //}
                    usr.RetrievePing();
                    usr.send(new Game.SP_PingInformation(usr));
                    //usr.PingTime = DateTime.Now.AddSeconds(5);
                }
                GC.Collect();
                Thread.Sleep(Configs.Server.PingRequestTick);
            }
        }

        public static List<User> getAllUsers()
        {
            return new List<User>(ServerUsers.Values.ToArray());
        }

        public static List<User> getAllUsers(int c)
        {
            return new List<User>(ServerUsers.Values.Take(c).ToArray());
        }

        public static User GetUser(int id)
        {
            if (ServerUsers.Count > 0)
            {
                return ServerUsers.Values.Where(u => u.userId == id).FirstOrDefault();
            }
            return null;
        }

        public static User GetUser(ushort connectionId)
        {
            if (ServerUsers.Count > 0)
            {
                return ServerUsers.Values.Where(u => u.connectionId == connectionId).FirstOrDefault();
            }
            return null;
        }

        public static List<User> GetUserByClan(int clanid)
        {
            return ServerUsers.Values.Where(p => p.clan != null && p.clan.id == clanid).ToList();
        }

        public static User GetRandomUser()
        {
            return ServerUsers.Values.OrderBy(u => Guid.NewGuid()).FirstOrDefault();
        }

        public static int GetUsersWithIP(string ip)
        {
            return ServerUsers.Values.Where(r => r.IP == ip).Count();
        }

        public static User GetUserByRoomSlot(Room Room, int Roomslot)
        {
            if (ServerUsers.Count > 0)
            {
                return Room.users.Values.Where(p => p != null && p.roomslot == Roomslot).FirstOrDefault();
            }
            return null;
        }

        public static User GetUser(string nickname)
        {
            if (ServerUsers.Count > 0)
            {
                return ServerUsers.Values.Where(p => string.Compare(p.nickname, nickname, true) == 0).FirstOrDefault();
            }
            return null;
        }

        public static User getTarGetUser(uint sessionId)
        {
            if (ServerUsers.ContainsKey(sessionId))
            {
                return (User)ServerUsers[sessionId];
            }
            return null;
        }

        public static void SetOnlineToFriends(User usr, bool status)
        {
            foreach (Messenger m in usr.Friends.Values)
            {
                if (m != null)
                {
                    User u = GetUser(m.id);
                    if (u != null)
                    {
                        Messenger f = u.GetFriend(usr.userId);
                        if (f != null)
                        {
                            f.isOnline = status;
                        }
                        m.isOnline = true;

                        u.RefreshFriends();
                    }
                }
            }
        }

        public static bool addUser(User usr)
        {
            for (uint i = 1; i <= Configs.Server.MaxSessions; i++)
            {
                if (!ServerUsers.ContainsKey(i))
                {
                    usr.sessionId = i;
                    break;
                }
            }

            if (usr.sessionId > 0)
            {
                DB.RunQuery("UPDATE users SET online = '1', serverid = '" + Configs.Server.serverId + "' WHERE id=" + usr.userId);
                ServerUsers.TryAdd(usr.sessionId, usr);
                Log.WriteLine(usr.nickname + " logged in!");
                return true;
            }
            Log.WriteError("Cannot add user " + usr.nickname + " to the stuck!");
            return false;
        }

        public static bool RemoveUser(User usr)
        {
            if (usr == null) return false; // Avoid double remove user issue that will cause an NullReferenceException
            if (ServerUsers.ContainsKey(usr.sessionId))
            {
                User u = null;
                
                DB.RunQuery("UPDATE users SET online = '0', Lastmac='" + usr.macAddress + "', country='" + usr.country + "', coupons='" + usr.coupons + "', todaycoupon='" + usr.todaycoupons + "', coupontime='" + usr.coupontime + "', Lasthwid='" + usr.hwid + "', lastjoin='" + Generic.timestamp + "', Lastipaddress='" + (usr.rank < 5 ? usr.IP : "U mad bro?") + "',  serverid='" + Configs.Server.serverId + "' WHERE id='" + usr.userId + "'");
                SetOnlineToFriends(usr, false);
                Log.WriteLine(usr.nickname + " logged out.");
                ServerUsers.TryRemove(usr.sessionId, out u);
                return true;
            }
            return false;
        }

        public static List<User> GetUsersInChannel(int ChannelID, bool inRoom)
        {
            return ServerUsers.Values.Where(p => p.channel == ChannelID && (!inRoom && p.room == null)).ToList();
        }

        public static void sendToServer(Packet p)
        {
            byte[] pBuffer = p.GetBytes();

            foreach (User usr in ServerUsers.Values)
            {
                usr.sendBuffer(pBuffer);
            }
        }

        public static void sendToChannel(int channelId, bool inRoom, Packet p)
        {
            byte[] pBuffer = p.GetBytes();

            ServerUsers.Values.Where(r => r.channel == channelId && (!inRoom && r.room == null)).ToList().ForEach(u => u.sendBuffer(pBuffer));
        }

        public static void UpdateUserlist(User u)
        {
            var tempList = Managers.UserManager.GetUsersInChannel(u.channel, false);

            byte[] buffer = (new SP_UserList(SP_UserList.Type.Wait, tempList)).GetBytes();

            foreach (User usr in Managers.UserManager.ServerUsers.Values.Where(r => r.channel == u.channel))
            {
                if (usr.room == null || !usr.room.gameactive)
                {
                                      
                    if (usr.actualUserlistType == (int)SP_UserList.Type.Wait && usr.userId != u.userId)
                    {
                        usr.sendBuffer(buffer);
                    }
                }
                if (usr.actualUserlistType == (int)SP_UserList.Type.Friends)
                {
                    if (u.GetFriend(usr.userId) != null)
                    {
                        usr.RefreshFriends();
                    }
                }
            }

            u.sendBuffer(buffer);

            if (u.clan != null)
            {
                List<User> clanList = u.clan.Users.Values.ToList();
                buffer = (new SP_UserList(SP_UserList.Type.Clan, clanList)).GetBytes();
                foreach (User usr in clanList)
                {
                    if (usr.actualUserlistType == (int)SP_UserList.Type.Clan)
                    {
                        usr.sendBuffer(buffer);
                    }
                }
            }
        }
    }
}