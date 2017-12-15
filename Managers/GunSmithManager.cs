using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Game;
using System.Data;

namespace Game_Server.Managers
{
    class GunSmithManager
    {
        public static Dictionary<int, GunSmith> items = new Dictionary<int, GunSmith>();
        public static void Load()
        {
            items.Clear();
            DataTable dt = DB.RunReader("SELECT * FROM gunsmith");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                int gameid = int.Parse(row["gameid"].ToString());
                int cost = 0;

                string item = row["item"].ToString();
                string rare = row["rare"].ToString();

                string[] required_materials = row["required_materials"].ToString().Split(',');

                int.TryParse(row["cost"].ToString(), out cost);

                string[] required_items = row["required_items"].ToString().Split(',');
                string[] lose_items = row["lose_items"].ToString().Split(',');

                GunSmith gunsmith = new GunSmith(gameid, cost, item, rare, required_materials, required_items, lose_items);
                if (!items.ContainsKey(gameid))
                {
                    items.Add(gameid, gunsmith);
                }
                else
                {
                    Log.WriteError("Couldn't add GunSmith ID " + gameid + " [DUPLICATE]");
                }
            }
            Log.WriteLine("Successfully loaded [" + items.Count + "] GunSmith Items");
        }

        public static GunSmith GetGunSmithByGameId(int id)
        {
            if (items.ContainsKey(id))
            {
                return (GunSmith)items[id];
            }
            return null;
        }
    }
}
