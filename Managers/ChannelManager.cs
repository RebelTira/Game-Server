using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Text;

namespace Game_Server.Managers
{
    class ChannelManager
    {
        private static int chCount = 4;

        public static ConcurrentDictionary<int, Channel> channels = new ConcurrentDictionary<int, Channel>();

        public static Thread updateThread;

        public static void Setup()
        {
            for (int i = 0; i < chCount; i++)
            {
                Channel ch = new Channel();
                ch.channelId = i;
                channels.TryAdd(i, ch);
            }

            updateThread = new Thread(UpdateRooms);
            updateThread.Start();
        }

        private static void UpdateRooms()
        {
            while (true)
            {
                foreach (Room r in GetAllRooms())
                {
                    new Thread(() => { try { r.update(); } catch { } });

                    try
                    {
                        bool isSomeoneInRoom = r.users.Values.Where(u => u != null && u.IsConnectionAlive).Count() > 0 && r.users.Count > 0;

                        if (!isSomeoneInRoom) r.remove();

                        if (!Configs.Server.Debug)
                        {
                            foreach (User usr in r.users.Values.Where(u => u.tcpClient == null))
                            {
                                Log.WriteDebug("[DEBUG] " + usr.nickname + " has been kicked out from the server because hasn't a tcp connection");
                                usr.disconnect();
                            }
                        }
                    }
                    catch { }
                }
                Thread.Sleep(200);
            }
        }

        public static List<Room> GetAllRooms()
        {
            List<Room> rooms = new List<Room>();
            foreach (Channel ch in channels.Values)
            {
                rooms.AddRange(ch.rooms.Values);
            }
            return rooms;
        }
    }

    class Channel
    {
        public int channelId;
        public int maxRooms = 500;
        public ConcurrentDictionary<int, Room> rooms = new ConcurrentDictionary<int, Room>();

        public Room GetRoom(int roomId)
        {
            if (rooms.ContainsKey(roomId))
            {
                return (Room)rooms[roomId];
            }
            return null;
        }

        // (int)Math.Ceiling((decimal)ch.rooms.Values.Where(u => u != null).Count() / 15);

        public int roomToPageCount
        {
            get
            {
                return (int)Math.Ceiling((decimal)rooms.Values.Where(u => u != null).Count() / 13);
            }
        }
        public int availableRoomToPageCount
        {
            get
            {
                return (int)Math.Ceiling((decimal)rooms.Values.Where(u => u != null && u.isJoinable).Count() / 13);
            }
        }

        public int GetOpenID { get { for (int i = 0; i < maxRooms; i++) { if (!rooms.ContainsKey(i)) { return i; } } return -1; } }

        public List<Room> GetRoomListByPage(int p)
        {
            return rooms.Values.OrderBy(r => r.id).Skip(p * 13).Take(13).ToList();
        }

        public List<Room> GetAvailableRoomListByPage(int p)
        {
            return rooms.Values.Where(r => r.isJoinable).OrderBy(r => r.id).Skip(p * 13).Take(13).ToList();
        }

        public List<Room> GetAvailableRoomList()
        {
            return rooms.Values.Where(r => r.isJoinable).OrderBy(r => r.id).ToList();
        }

        public void UpdateLobby(Room room)
        {
            byte[] buffer = (new Game.SP_RoomListUpdate(room)).GetBytes();
            foreach (User usr in UserManager.GetUsersInChannel(room.channel, false))
            {
                if (usr.lobbypage == Math.Floor((decimal)(room.id / 13)))
                {
                    usr.sendBuffer(buffer);
                }
            }
        }

        public bool AddRoom(int roomId, Room r)
        {
            if (!rooms.ContainsKey(roomId))
            {
                return rooms.TryAdd(roomId, r);
            }
            return false;
        }

        public bool RemoveRoom(int roomId)
        {
            if (rooms.ContainsKey(roomId))
            {
                Room r = (Room)rooms[roomId];

                return rooms.TryRemove(roomId, out r);
            }
            return false;
        }
    }
}
