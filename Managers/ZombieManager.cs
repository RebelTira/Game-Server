using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Game_Server.Managers
{
    internal enum ZombieType
    {
        Madman = 0,
        Maniac = 1,
        Grinders = 2,
        Grounders = 3,
        Growlers = 4,
        Heavys = 5,
        Lovers = 6,
        Handgeman = 7,
        Chariot = 8,
        Crushers = 9,
        //>--- +++
        Buster = 10,
        Crasher = 11,
        Envy = 12,
        Claw = 13,
        Bomber = 14,
        Defeder = 15,
        Breaker = 16,
        //>--- +++
        MadSoldier = 17,
        MadPrisoner = 18,

    }

    class ZombieData
    {
        public int Type = 0;
        public string Name = null;
        public int Health = 0;
        public int Points = 1;
        public int Damage = 150;
        public bool SkillPoint = false;

        public ZombieData(int Type, string Name, int Health, int Points, int Damage, bool SkillPoint)
        {
            this.Health = Health;
            this.Name = Name;
            this.Points = Points;
            this.Damage = Damage;
            this.SkillPoint = SkillPoint;
            this.Type = Type;
        }
    }

    class ZombieManager
    {
        static Dictionary<int, ZombieData> Datas = new Dictionary<int, ZombieData>();

        public static void Load()
        {
            Datas.Clear();
            Log.WriteInfo(">---Z.Manager-59 Load Zombies de la BD ");
            DataTable dt = DB.RunReader("SELECT * FROM zombies");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                int type = int.Parse(row["type"].ToString());
                string name = row["name"].ToString();
                int health = int.Parse(row["health"].ToString());
                int points = int.Parse(row["points"].ToString());
                int damage = int.Parse(row["damage"].ToString());
                int skillpoints = int.Parse(row["skillpoint"].ToString());
                ZombieData Data = new ZombieData(type, name, health, points, damage, skillpoints > 0 ? true : false);
                if (!Datas.ContainsKey(type))
                {
                    Datas.Add(type, Data);
                }
                else
                {
                    Log.WriteError("Duplicate Zombie Type [" + type + "]");
                }
            }
        }
       
        public static ZombieData GetZombieDataByType(int Type)
        {
            if (Datas.ContainsKey(Type))
            {
                return (ZombieData)Datas[Type];
            }
            return null;
        }

        public static void GetZombieData(Zombie Zombie)
        {
            ZombieData Data = GetZombieDataByType(Zombie.Type);
            if (Data != null)
            {
                Zombie.Name = Data.Name;
                Zombie.Health = Data.Health;
                Zombie.Points = Data.Points;
                Zombie.Damage = Data.Damage;
                Zombie.SkillPoint = Data.SkillPoint;
            }
        }
    }
}