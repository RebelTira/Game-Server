using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Game_Server.Networking
{
    /// <summary>
    /// This class listen's connections
    /// </summary>
    class NetworkSocket
    {
        private static Socket socket;
        private static uint acceptedConnections = 0;

        public static bool InitializeSocket(int port)
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                socket.Listen(0);
                socket.BeginAccept(new AsyncCallback(OnReceive), socket);
                Log.WriteLine("Listening connections on port " + port);
                Log.WriteInfo("> -- Network_Socket-27 InitializeSocket(int port) " + port);  //>---  10375
                return true;
            }
            catch { }
            return false;
        }

        private static void OnReceive(IAsyncResult iAr)
        {
            if (!Program.running) return;
            socket.BeginAccept(new AsyncCallback(OnReceive), socket);
            Socket remoteSocket = ((Socket)iAr.AsyncState).EndAccept(iAr);
            string ip = remoteSocket.RemoteEndPoint.ToString().Split(':')[0];
            Log.WriteLine("Accepted connection from " + ip);
            acceptedConnections++;
            if (acceptedConnections >= Configs.Server.MaxSessions)
            {
                acceptedConnections = 1;
            }
            User usr = new User(acceptedConnections, remoteSocket);
        }
    }
}