/*
 _____   ___ __  __  _____ _____   ___  _  __              ___   ___   __    __ 
/__   \ /___\\ \/ /  \_   \\_   \ / __\( )/ _\            / __\ /___\ /__\  /__\
  / /\///  // \  /    / /\/ / /\// /   |/ \ \            / /   //  /// \// /_\  
 / /  / \_//  /  \ /\/ /_/\/ /_ / /___    _\ \          / /___/ \_/// _  \//__  
 \/   \___/  /_/\_\\____/\____/ \____/    \__/          \____/\___/ \/ \_/\__/  
__________________________________________________________________________________

Created by: ToXiiC
Thanks to: CodeDragon, Kill1212, CodeDragon

*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Game_Server.Web
{
    class WebManager
    {
        private static Socket socket;
        private static byte[] buffer = new byte[1024];

        public static bool openSocket(int port)
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                socket.Listen(0);
                socket.BeginAccept(new AsyncCallback(acceptConnection), socket);
                Log.WriteLine("Listening web connections from " + port);
                return true;
            }
            catch
            {
                Log.WriteError("Error while setting up asynchronous the socket server for connections on port " + port + ".");
                Log.WriteError("Port " + port + " could be invalid or in use already.");
                Thread.Sleep(2500);
                Environment.Exit(0);
            }
            return false;
        }

        private static void acceptConnection(IAsyncResult iAr)
        {
            if (!Program.running) return;
            socket.BeginAccept(new AsyncCallback(acceptConnection), socket);
            Socket uSocket = ((Socket)iAr.AsyncState).EndAccept(iAr);
            string IP = uSocket.RemoteEndPoint.ToString().Split(':')[0];
            Log.WriteLine("Web Server Connection from: " + IP);
            WebServer ws = new WebServer(uSocket);
        }
    }
}
