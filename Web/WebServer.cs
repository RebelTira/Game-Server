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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Data;
using Game_Server.Managers;

namespace Game_Server.Web
{
    class WebServer
    {
        private Socket socket;
        private byte[] buffer = new byte[1024];

        public WebServer(Socket s)
        {
            this.socket = s;
            s.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
        }

        #region Dispose

        public virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void OnReceive(IAsyncResult iAr)
        {
            try
            {
                int DataLength = socket.EndReceive(iAr);

                if (DataLength > 0)
                {
                    this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
                    byte[] packetBuffer = new byte[DataLength];
                    Array.Copy(buffer, 0, packetBuffer, 0, DataLength);
                    
                    string IP = socket.RemoteEndPoint.ToString().Split(':')[0];
                    string Data = Encoding.UTF8.GetString(packetBuffer);
                    if (Data.Length > 0)
                    {
                        if (GlobalServers.GetServer(IP) != null || Configs.Web.remote) // Avoid fake requests
                        {
                            string[] splittedData = Data.Split('|');
                            string nickname = splittedData[0];
                            DataTable dt = DB.RunReader("SELECT * FROM users WHERE nickname='" + nickname + "'");
                            if (dt.Rows.Count > 0)
                            {
                                DataRow row = dt.Rows[0];
                                int rank = int.Parse(row["rank"].ToString());
                                string operation = splittedData[1];
                                switch (operation)
                                {
                                    case "BROADCAST":
                                        {
                                            string message = splittedData[2];
                                            message = WordFilterManager.Replace(message);
                                            Log.WriteLine(nickname + " broadcasted " + message);
                                            message = message.Replace((char)0x20, (char)0x1D);
                                            UserManager.sendToServer(new Game.SP_Chat(nickname, Game.SP_Chat.ChatType.Notice1, message, (uint)(rank > 2 ? 999 : 0), "NULL"));
                                            break;
                                        }
                                    case "RELOAD_OUTBOX":
                                        {
                                            User u = UserManager.GetUser(nickname);
                                            if (u != null)
                                            {
                                                u.LoadOutboxItems();
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        else
                        {
                            Log.WriteError("Connection refused by IP: " + IP + " - Request: " + Data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteError("WebServer Socket error: " + ex.Message);
            }
        }
    }
}
