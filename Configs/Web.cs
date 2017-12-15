using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Configs
{
    class Web
    {
        public static bool allow = false;
        public static int port = 7352;
        public static bool remote = false;

        public static void Load()
        {
            try
            {
                allow = bool.Parse(IO.ReadValue("WebServer", "Enabled"));
                port = int.Parse(IO.ReadValue("WebServer", "Port"));
                remote = bool.Parse(IO.ReadValue("WebServer", "AllowRemoteRequest"));
            }
            catch (Exception ex)
            {
                Log.WriteError("Couldn't Load server info " + ex.Message);
            }
        }
    }
}
