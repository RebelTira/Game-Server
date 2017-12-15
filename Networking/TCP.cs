using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Game_Server.Networking
{
    class TCP_Client : IDisposable
    {
        enum TcpPacket : ushort
        {
            Authentication = 22039, // Upon log in the server
            HackShield = 31264,
            UpdatePlayerStatus = 12544,
            PlayerRoll = 12545,
            WeaponZoom = 12546,
            UpdateVehicleStatus = 12801,
            ThrowGranadeRocket = 13312,
            SwitchWeapon = 13313,
            WeaponExplosion = 13315,
            PlayerEmotion = 12547, // When you press F9 / F10 / F11 / F12 (Animation),
            ObjectMove = 12800,
            HackInfo = 13314,
            TextChat = 13824,
            Bullet = 13312,
            Explosion = 13315,
            ClientHearthBeat = 22040,
        }

        public User usr = null;
        private Socket socket;
        private byte[] buffer = new byte[1024];

        public ushort connectionId = 0;
        public string remoteIp;

        public bool disconnected = false;

        public TCP_Client(Socket socket)
        {
            this.socket = socket;

            remoteIp = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();

            Thread t = new Thread(OnReceive); // Should improve a lot the gameplay instead of using callback that is always handled in a generic thread
            t.Priority = ThreadPriority.Highest;
            t.Start();
        }

        public void Send(byte[] buffer)
        {
            try { if (buffer != null && buffer.Length > 0) { socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(sendCallBack), null); } }
            catch { }
        }

        private void sendCallBack(IAsyncResult iAr)
        {
            if (socket != null)
            {
                try { socket.EndSend(iAr); }
                catch { }
            }
        }
        private List<TCP_Client> GetUsersInMyRoom
        {
            get
            {
                if (this.usr.room != null)
                {
                    return TCP.userConnections.Where(r =>
                        r.usr != null && this.usr != null &&
                        r.usr.userId != this.usr.userId &&
                        r.usr.room != null &&
                        r.usr.room.id == this.usr.room.id)
                        .ToList();
                }
                return null;
            }
        }

        private void SendToRoom(byte[] data)
        {
            var users = GetUsersInMyRoom;
            foreach (TCP_Client c in users)
            {
                c.Send(data);
            }
        }

        private TcpPacket AnalyzePacket(byte[] data)
        {
            ushort signature = data[0];
            ushort length = Generic.ByteToUShort(data, 1); // 3 = Header
            ushort packetId = Generic.ByteToUShort(data, 3);
            TcpPacket packet = (TcpPacket)packetId;

            if (data.Length == length + 3) // + 3 equals to the header
            {
                if (packet == TcpPacket.Authentication)  //>---  22039
                {
                    if (this.usr == null)
                    {
                        this.connectionId = Generic.ByteToUShort(data, 7); // Sent on 24832 (last block)
                        Log.WriteLine("> -- TCP-108 connectionId: " + connectionId);
                        this.usr = Managers.UserManager.GetUser(this.connectionId);

                        if (this.usr == null)
                        {
                            disconnect("No valid p2s user");
                            return packet;
                        }

                        this.usr.tcpClient = this;
                    }
                }
                else if (packet == TcpPacket.HackShield)
                {
                    /* Just handle */
                }
                else if (packet == TcpPacket.ClientHearthBeat)
                {
                    usr.heartBeatTime = Generic.timestamp + 60;
                }
                else if (packet == TcpPacket.UpdatePlayerStatus || packet == TcpPacket.Bullet || packet == TcpPacket.Explosion || packet == TcpPacket.HackInfo ||
                        packet == TcpPacket.ObjectMove || packet == TcpPacket.PlayerEmotion || packet == TcpPacket.PlayerRoll || packet == TcpPacket.UpdateVehicleStatus ||
                        packet == TcpPacket.SwitchWeapon || packet == TcpPacket.ThrowGranadeRocket || packet == TcpPacket.WeaponExplosion ||
                        packet == TcpPacket.WeaponZoom || packet == TcpPacket.TextChat)
                {
                    if (usr != null && usr.room != null)
                    {
                        if (usr.room.gameactive && (usr.room.users.Count > 1 || usr.room.spectators.Count > 0))
                        {
                            byte playerSlot = (byte)data[9];

                            if (playerSlot == usr.roomslot && usr.IsAlive())
                            {
                                if (packet == TcpPacket.ThrowGranadeRocket)
                                {
                                    Managers.Item item = Managers.ItemManager.GetItemByID(usr.weapon);

                                    if (item != null)
                                    {
                                        bool snowFight = usr.room.new_mode == 6 && usr.room.new_mode_sub == 2; //>--- 6=snowFight
                                        bool grenadeOnly = usr.room.new_mode == 4; //>--- Solo granadas

                                        if (!snowFight)
                                        {
                                            if (item.UseableBranch(4) && (item.UseableSlot(2) || item.UseableSlot(5) || item.UseableSlot(7)))
                                            {
                                                usr.throwRockets++;
                                            }
                                            else if (item.UseableSlot(3) || item.UseableBranch(4))
                                            {
                                                usr.throwNades++;
                                            }
                                        }

                                        if ((usr.throwNades >= 100 || usr.throwRockets >= 100) && !grenadeOnly) //>--- Cambio 10 nades y 50 rockets por 100
                                        {
                                            Log.WriteError("User " + usr.nickname + " throw more than " + (usr.throwRockets >= 100 ? "100 rockets" : "10 nades"));
                                            usr.disconnect();
                                            return packet;
                                        }
                                    }
                                }

                                SendToRoom(data);
                            }
                        }
                    }
                }
                else
                {
                    Log.WriteError("Unhandled TCP Packet (" + packetId + ") " + usr.nickname + " " + usr.room.id);
                }
            }
            return packet;
        }

        private void OnReceive()
        {
            try
            {
                while (!disconnected && socket.Connected)
                {
                    if (usr == null && connectionId > 0) break;
                    int length = socket.Receive(buffer);

                    if (length > 0)
                    {
                        byte[] packetBuffer = new byte[length];
                        Array.Copy(buffer, 0, packetBuffer, 0, length);

                        AnalyzePacket(packetBuffer);
                    }
                    else
                    {
                        disconnect();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                disconnect();
                //disconnect("ERROR: " + ex.Message);
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            GC.Collect();
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        private void disconnect(string reason = null)
        {
            if (disconnected) return;
            
            try { socket.Close(); }
            catch { }

            disconnected = true;

            if (usr != null) { usr.disconnect(); }

            if(reason != null)
            {
                Log.WriteDebug(usr.nickname + " has been disconnected [Reason: " + reason + "]");
            }

            TCP.RemoveConnection(this);

            this.Dispose();
        }
    }

    class TCP
    {
        private static Socket socket;
        private static ushort port;
        private static EndPoint endPoint;
        public static List<TCP_Client> userConnections = new List<TCP_Client>();
        private static object lockobj = new object();
        private static ushort lastConnectionId = 0;

        public static byte[] ClientHearthBeatPacket = new byte[] { 0xCC, 0x04, 0x00, 0x18, 0x56, 0x00, 0x00 };


        private static void HearthBeat()
        {
            while (true)
            {
                List<TCP_Client> clients = userConnections.ToList();

                foreach (TCP_Client c in clients)
                {
                    if (c.usr != null)
                    {
                        c.Send(ClientHearthBeatPacket);
                        //c.usr.heartBeatTime = Generic.timestamp + 10;
                    }
                }
                Thread.Sleep(60000);
            }
        }

        private static bool Initialize()
        {
            endPoint = new IPEndPoint(IPAddress.Any, (int)port);
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(endPoint);
                socket.Listen(0);
                socket.BeginAccept(new AsyncCallback(OnNewConnection), socket);
                Thread t = new Thread(HearthBeat);
                t.Start();
                return true;
            }
            catch { }
            return false;
        }

        public static bool Start(int srvPort)
        {
            port = (ushort)srvPort;
            bool status = Initialize();
            if (status)
            {
                Log.WriteLine("Listening TCP connections on port " + port);
            }
            return status;
        }

        public static void RemoveConnection(TCP_Client client)
        {
            if (userConnections.Contains(client))
            {
                userConnections.Remove(client);
            }
        }

        public static ushort GetFreeConnectionID
        {
            get
            {
                lastConnectionId++;
                if (lastConnectionId >= ushort.MaxValue) lastConnectionId = 1;
                return lastConnectionId;
            }
        }

        private static void OnNewConnection(IAsyncResult iAr)
        {
            socket.BeginAccept(new AsyncCallback(OnNewConnection), socket);
            try
            {
                Socket remoteSocket = ((Socket)(iAr.AsyncState)).EndAccept(iAr);
                TCP_Client usr = new TCP_Client(remoteSocket);
                userConnections.Add(usr);
            }
            catch { }
        }
    }
}
