using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Game_Server
{
    /// <summary>
    /// Server Informations such as ip, flag, min rank for access are stored here
    /// </summary>
    class Server
    {
        public int id = -1;
        public int flag, minrank, slot = 0;
        public string name;
        public string ip;
    }

    class GlobalServers
    {
        public static List<Server> servers = new List<Server>();

        public static void LoadServers()
        {
            DataTable dt = DB.RunReader("SELECT * FROM servers");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                int serverId = int.Parse(row["serverid"].ToString());

                Server server = new Server();
                server.id = serverId;
                server.name = row["name"].ToString();
                server.ip = row["ip"].ToString();
                server.flag = int.Parse(row["flag"].ToString());
                server.minrank = int.Parse(row["minrank"].ToString());
                server.slot = int.Parse(row["slot"].ToString());

                if (serverId == Configs.Server.serverId)
                {
                    Program.server = server;
                }
                servers.Add(server);
            }

            if (Program.server == null)
            {
                Log.WriteError("ServerID has not been binded");
            }
            Log.WriteLine("Successfully loaded " + servers.Count + " servers.");
        }

        public static Server GetServer(string ip)
        {
            foreach(Server s in servers)
            {
                if (s.ip == ip) return s;
            }
            return null;
        }
    }
}
