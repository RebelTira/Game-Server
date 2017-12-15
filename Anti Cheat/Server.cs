using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Game_Server
{
    class AntiCheatServer
    {
        private Socket socket;
        private int port;
        private int sessionId = 0;

        public bool Initialize(int port)
        {
            this.port = port;
            try
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socket.Bind(new IPEndPoint(IPAddress.Any, port));
                this.socket.Listen(0);
                this.socket.BeginAccept(new AsyncCallback(OnNewClient), socket);
                Log.WriteLine("Listening AntiCheat Client(s) connections on port " + port);
                return true;
            }
            catch
            {
            }
            return false;
        }

        public void OnNewClient(IAsyncResult iAR)
        {
            try
            {
                Socket s = ((Socket)(iAR.AsyncState)).EndAccept(iAR);

                Anti_Cheat.Client client = new Anti_Cheat.Client(s, sessionId);
                sessionId++;

                Log.WriteLine("New AntiCheat Connection from " + s.RemoteEndPoint.ToString());
                this.socket.BeginAccept(new AsyncCallback(OnNewClient), socket);
            }
            catch { }
        }
    }
}
