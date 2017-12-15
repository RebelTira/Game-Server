using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Game_Server.Anti_Cheat
{
    class Client
    {
        private Socket socket;
        public int sessionId;
        private byte[] buffer = new byte[1024];

        public Client(Socket socket, int sessionId)
        {
            this.socket = socket;
            this.sessionId = sessionId;

            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
        }

        void disconnect()
        {
            try { socket.Close(); }
            catch { }

            if (this.socket != null)
            {
                this.socket = null;
            }
        }

        public void send(Structure.Packet p)
        {
            try
            {
                byte[] sendBuffer = p.GetBytes();
                if (sendBuffer != null && sendBuffer.Length > 0)
                {
                    socket.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, new AsyncCallback(sendCallBack), null);
                }
            }
            catch
            {
                disconnect();
            }
        }

        private void sendCallBack(IAsyncResult iAr)
        {
            if (socket != null)
            {
                try { socket.EndSend(iAr); }
                catch { }
            }
        }

        void OnReceive(IAsyncResult iAr)
        {
            try
            {
                int length = socket.EndReceive(iAr);

                if (length > 0)
                {
                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);

                    byte[] packetBuffer = new byte[length];
                    Array.Copy(buffer, 0, packetBuffer, 0, length);
                    try
                    {
                        string packet = Encoding.GetEncoding("Windows-1250").GetString(packetBuffer);
                        Structure.Handler handler = Structure.PacketManager.ParsePacket(packet);
                        Log.WriteInfo(">---Client-77 packet: " + packet); //>--- +++
                        if (handler != null)
                        {
                            new Thread(() => handler.Handle(this)).Start();
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    disconnect();
                }
            }
            catch
            {
                disconnect();
            }
        }
    }
}
