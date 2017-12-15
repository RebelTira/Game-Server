using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Game_Server.Managers
{
    class BanManager
    {
        static List<string> BannedHWID = new List<string>();
        static List<string> BannedMAC = new List<string>();

        public static void Load()
        {
            LoadBannedMAC();
            LoadBannedHWID();
        }

        public static void LoadBannedMAC()
        {
            BannedMAC.Clear();

            DataTable dt = DB.RunReader("SELECT * FROM macs_ban");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                BannedMAC.Add(row["mac"].ToString());
            }
        }

        public static void LoadBannedHWID()
        {
            BannedHWID.Clear();

            DataTable dt = DB.RunReader("SELECT * FROM hwid_bans");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                BannedHWID.Add(row["hwid"].ToString());
            }
        }

        public static bool isMacBanned(string mac)
        {
            return BannedMAC.Cast<string>().Where(r => r == mac).Count() > 0;
        }

        public static bool isHWIDBanned(string hwid)
        {
            return BannedHWID.Cast<string>().Where(r => r == hwid).Count() > 0;
        }
    }
}
