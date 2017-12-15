using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Game_Server.Managers
{
    class VehicleManager
    {
        public string Code;
        public string Name;
        public int MaxHealth;
        public int RespawnTime;
        public string Seats;
        public bool isJoinable;
        public static List<VehicleManager> CollectedVehicles = new List<VehicleManager>();

        public static void Load()
        {
            CollectedVehicles.Clear();

            DataTable dt = DB.RunReader("SELECT * FROM vehicles");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                string code = row["code"].ToString();
                string name = row["name"].ToString();
                int health = int.Parse(row["maxhealth"].ToString());
                int respawn = int.Parse(row["respawntime"].ToString());
                string seats = null;
                if (row["seats"].ToString() != "-1")
                {
                    seats = row["seats"].ToString();
                }
                bool joinable = row["joinable"].ToString() == "1" ? true : false;
                VehicleManager v = new VehicleManager(code, name, health, respawn, seats, joinable);
                CollectedVehicles.Add(v);
            }
            Log.WriteLine("Successfully loaded [" + dt.Rows.Count + "] Vehicle Informations");
        }

        public static VehicleManager GetVehicleInfoByCode(string Code)
        {
            foreach (VehicleManager VehicleInfo in CollectedVehicles)
                if (VehicleInfo.Code == Code)
                    return VehicleInfo;
            return null;
        }

        public VehicleManager(string Code, string Name, int MaxHealth, int RespawnTime, string Seats, bool isJoinable)
        {
            this.Code = Code;
            this.Name = Name;
            this.MaxHealth = MaxHealth;
            this.RespawnTime = RespawnTime;
            this.Seats = Seats;
            this.isJoinable = isJoinable;
        }
    }
}
