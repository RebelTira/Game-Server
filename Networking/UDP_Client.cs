using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections;
using System.Threading.Tasks;

using Game_Server.Managers;

namespace Game_Server.Networking
{
    /// <summary>
    /// This class handles UDP requests
    /// </summary>
    class UDP
    {
        private int port;
        private UdpClient socket;
        private EndPoint endPoint;
        private byte[] buffer = new byte[1024];

        public bool InitializeSocket()
        {
            if (socket != null)
            {
                socket.Close();
                this.socket = null;
            }

            try
            {
                this.socket = new UdpClient(this.port);
                socket.BeginReceive(new AsyncCallback(OnReceive), null);
                return true;
            }
            catch { }
            return false;
        }

        public bool Start(int port)
        {
            this.port = port;
            this.endPoint = new IPEndPoint(IPAddress.Any, this.port);
            bool status = InitializeSocket();
            if (status)
            {
                Log.WriteLine("Binded the UDP socket to port " + port);
            }
            return status;
        }

        public void OnReceive(IAsyncResult iAr)
        {
            try
            {

                this.endPoint = new IPEndPoint(IPAddress.Any, this.port);
                socket.BeginReceive(new AsyncCallback(OnReceive), null);

                try
                {
                    IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] pBuffer = socket.EndReceive(iAr, ref receiveEndPoint);
                    if (pBuffer.Length > 0)
                    {
                        byte[] response = AnalyzePacket(pBuffer, receiveEndPoint);
                        socket.BeginSend(response, response.Length, receiveEndPoint, SendProc, socket);
                        //socket.Send(response, response.Length, receiveEndPoint);
                        //socket.BeginSend(response, response.Length, endPoint, SendProc, socket);
                    }
                }
                catch
                {
                    //Log.WriteError("Error handling UDP packet: " + ex.Message);
                }
            }
            catch
            {
                InitializeSocket();
            }
        }

        void SendProc(IAsyncResult t)
        {
            try
            {
                UdpClient a = (UdpClient)t.AsyncState;
                if (a.Available > 0)
                {
                    a.EndSend(t);
                }
            }
            catch (Exception ex)
            {
                Log.WriteError("Error while 'SendProc':\n" + ex.Message);
            }
        }

        private byte[] AnalyzePacket(byte[] packet, IPEndPoint IPeo)
        {
            byte[] response = packet;
            uint type = packet.ToUShort(0);
            ushort sessionID = packet.ToUShort(4);
            User usr = Managers.UserManager.getTargetUser(sessionID);
            if (usr == null)
            {
                return null;
            }

            switch (type)
            {
                case 0x1001: // Initial packet
                    packet.WriteUShort((ushort)(this.port + 1), 4);
                    //usr.setRemoteEndPoint(IPeo);
                    //Log.WriteDebug(usr.nickname + " -> UDP Connected & Set Remote E  nd Point " + IPeo.Address.ToString());
                    break;
                case 0x1010: // UDP Ping packet
                    if (packet[14] == 0x21)
                    {
                        /*if (usr.DetectionCount < 2 && usr.room != null)
                        {
                            usr.DetectionCount++;
                            if (usr.DetectionCount >= 0)
                            {
                                TimeSpan span = (DateTime.Now - usr.DetectionSecond);
                                int c = (int)span.TotalMilliseconds;
                                usr.DetectionSecond = DateTime.Now;
                                if (c < 9750 && c > 2000)
                                {
                                    //Log.WriteError(usr.nickname + " -> Overclock detected, packet time: " + c.ToString().Substring(0, 1) + "." + c.ToString().Substring(1, 2));
                                    //DB.runQuery("INSERT INTO anticheat_logs (userid, description, timestamp) VALUES ('" + usr.userId + "', '" + usr.nickname + " - Overclock detected, packet time: " + c.ToString().Substring(0, 1) + "." + c.ToString().Substring(1, 2) + "', '" + Generic.timestamp + "')");
                                }
                            }
                        }*/

                        usr.setLocalEndPoint(packet.ToIPEndPoint(32));
                        usr.setRemoteEndPoint(IPeo);

                        //Log.WriteDebug(usr.nickname + " -> UDP Connected & Set Local End Point " +usr.localEndPoint.Address.ToString());

                        response = packet.Extend(65);

                        #region old response bytes
                        /*
                            byte[] response = new byte[65]
                                {
                                    0x10, 0x10, //0
                                    0x0, 0x0, //2
                                    0x0, 0x0, //4
                                    0xFF, 0xFF, 0xFF, 0xFF, //6
                                    0x0, 0x0, 0x0, 0x0, //10
                                    0x21, 0x0, 0x0, 0x41, //14
                                    0x0, 0x0, 0x0, 0x0, //18
                                    0x0, 0x0, 0x0, 0x0, //22
                                    0x0, 0x0, //26
                                    0x1, 0x11, 0x13, 0x11, //28
                                    0x0, 0x0, 0x0, 0x0, 0x0, 0x0, //32 remoteip
                                    0x11, 0x11, 0x11, 0x11, //38
                                    0x11, 0x11, 0x11, 0x11, //42
                                    0x01, 0x11, 0x13, 0x11, //48
                                    0x0, 0x0, 0x0, 0x0, 0x0, 0x0, //50 localip
                                    0x19, 0x19, 0x19, 0x19, //56
                                    0x19, 0x19, 0x19, 0x19, //60
                                    0x11 //64
                                };
                             */
                        #endregion

                        response[17] = 0x41;
                        response[response.Length - 1] = 0x11;
                        response.WriteUShort((ushort)usr.sessionId, 4); //Not really necessary
                        response.WriteIPEndPoint(usr.remoteEndPoint, 32);
                        response.WriteIPEndPoint(usr.localEndPoint, 50);
                    }
                    else if (packet[14] == 0x31 || packet[14] == 0x32 || packet[14] == 0x34)
                    {
                        if (usr.room != null)
                        {
                            ushort targetID = packet.ToUShort(22);
                            User player = Managers.UserManager.getTargetUser(targetID);
                            if (player != null && player.room.id == usr.room.id)
                            {
                                IPEndPoint tunnelEndPoint = (usr.remoteEndPoint != player.remoteEndPoint ? player.remoteEndPoint : player.localEndPoint);

                                socket.BeginSend(packet, packet.Length, tunnelEndPoint, SendProc, socket);

                                foreach (User u in player.room.spectators.Values)
                                {
                                    socket.BeginSend(packet, packet.Length, tunnelEndPoint, SendProc, socket);
                                }
                            }
                        }

                        #region Daan Tunneling
                        //if (usr.room == null) return;
                        //uint room = packet.ToUShort(6);
                        //if (usr.room.ID == room) {
                        //    ushort targetID = packet.ToUShort(22);
                        //    player target = playerManager.GetUser(targetID);
                        //    if (target != null) {
                        //        _server.SendTo(packet, target.RemoteEP);
                        //    } else {
                        //        Console.WriteLine("UDP TUNNEL PACKET FAULT - TARGET DOES NOT EXIST");
                        //        Console.WriteLine("Press enter to continue...");
                        //        Console.ReadLine();
                        //    }
                        //} else {
                        //    Console.WriteLine("UDP TUNNEL PACKET FAULTY - ROOM DID NOT MATCH usr");
                        //    Console.WriteLine("Press enter to continue...");
                        //    Console.ReadLine();
                        //}
                        #endregion
                    }
                    break;
            }
            return response;
        }
    }

    public static class UDPReader
    {
        private const byte xOrSendKey = Game.Cryption.serverXor;
        private const byte xOrReceiveKey = Game.Cryption.clientXor;

        public static ushort ToUShort(this byte[] packet, int offset)
        {
            byte[] value = new byte[2];
            Array.Copy(packet, offset, value, 0, 2);
            Array.Reverse(value);
            return BitConverter.ToUInt16(value, 0);
        }

        public static uint ToUInt(this byte[] packet, int offset)
        {
            byte[] value = new byte[4];
            Array.Copy(packet, offset, value, 0, 4);
            Array.Reverse(value);
            return BitConverter.ToUInt32(value, 0);
        }

        public static IPEndPoint ToIPEndPoint(this byte[] packet, int offset)
        {
            for (int i = offset; i < offset + 6; i++)
            {
                packet[i] ^= xOrSendKey;
            }
            ushort port = BitConverter.ToUInt16(packet, offset);
            uint ip = BitConverter.ToUInt32(packet, offset + 2);
            return new IPEndPoint(ip, port);
        }

        public static void WriteUShort(this byte[] packet, ushort value, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            Array.Copy(bytes, 0, packet, offset, 2);
        }

        public static void WriteUInt(this byte[] packet, uint value, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            Array.Copy(bytes, 0, packet, offset, 4);
        }

        public static void WriteIPEndPoint(this byte[] packet, IPEndPoint endpoint, int offset)
        {
            byte[] value = new byte[6];
            Array.Copy(BitConverter.GetBytes(endpoint.Port), 0, value, 0, 2);
            Array.Copy(endpoint.Address.GetAddressBytes(), 0, value, 2, 4);
            Array.Reverse(value);
            for (int i = offset; i < offset + 6; i++)
            {
                packet[i] = (byte)(value[i - offset] ^ xOrReceiveKey);
            }
        }

        public static byte[] Extend(this byte[] packet, int length)
        {
            byte[] newPacket = new byte[length];
            Array.Copy(packet, newPacket, packet.Length);
            return newPacket;
        }
    }
}
