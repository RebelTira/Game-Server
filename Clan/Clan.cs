using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;

namespace Game_Server
{
    public class ClanPendingUsers
    {
        public int id;
        public string nickname, EXP, ClanJoinDate;

        public ClanPendingUsers(int id, string nickname, string exp, string ClanJoinDate)
        {
            this.id = id;
            this.nickname = nickname;
            this.EXP = exp;
            this.ClanJoinDate = ClanJoinDate;
        }
    }

    public class ClanUsers
    {
        public int id, clanrank;
        public string nickname, EXP, ClanJoinDate;

        public ClanUsers(int id, string nickname, string exp, string ClanJoinDate, int clanrank)
        {
            this.id = id;
            this.nickname = nickname;
            this.EXP = exp;
            this.ClanJoinDate = ClanJoinDate;
            this.clanrank = clanrank;
        }
    }

    class ClanRanking
    {
        public static ConcurrentDictionary<int, Clan> clans = new ConcurrentDictionary<int, Clan>();
        public static int LastUpdate = -1;

        public static void refreshclans()
        {
            clans.Clear();
            DataTable dt = DB.RunReader("SELECT * FROm clans ORDER BY exp DESC LIMIT 0, 30");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                int id = int.Parse(row["id]"].ToString());
                Clan c = Managers.ClanManager.GetClan(id);
                if (c != null)
                {
                    clans.TryAdd(id, c);
                }
            }
        }
    }

    class ClanWar
    {
        public int id;
        public string versusClan;
        public string score;
        public bool won;

        public ClanWar(int id, string vsclan, string score, bool won)
        {
            this.id = id;
            this.versusClan = vsclan;
            this.score = score;
            this.won = won;
        }
    }

    class Clan
    {
        public int id = 0;
        public string name = null;
        public uint iconid;
        public int maxUsers = 0;
        public int win = 0;
        public int lose = 0;
        public int exp = 0;
        public string Announcment = null;
        public string Description = null;
        public string Master;
        public string MasterEXP;
        public long creation;

        public enum Rank : int
        {
            Recon = 1,
            Squad = 2,
            Platoon = 3,
            Company = 4,
            Battalion = 5,
            Regiment = 6,
            Brigade = 7,
            Division = 8,
            Corps = 9
        }

        public int GetRank()
        {
            return (int)Rank.Division;
            if (ClanUsers.Count >= 81) return (int)Rank.Corps;
            else if (ClanUsers.Count >= 61) return (int)Rank.Division;
            else if (ClanUsers.Count >= 51) return (int)Rank.Brigade;
            else if (ClanUsers.Count >= 41) return (int)Rank.Regiment;
            else if (ClanUsers.Count >= 31) return (int)Rank.Battalion;
            else if (ClanUsers.Count >= 21) return (int)Rank.Company;
            else if (ClanUsers.Count >= 11) return (int)Rank.Platoon;
            else if (ClanUsers.Count >= 6) return (int)Rank.Squad;
            else return (int)Rank.Recon;
        }
        public string GetCreationDate()
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(creation).ToString("yyyy.mm.dd");
        }

        public ConcurrentDictionary<int, User> Users = new ConcurrentDictionary<int, User>();
        public ConcurrentDictionary<int, ClanPendingUsers> pendingUsers = new ConcurrentDictionary<int, ClanPendingUsers>();
        public ConcurrentDictionary<int, ClanUsers> ClanUsers = new ConcurrentDictionary<int, ClanUsers>();
        public ConcurrentDictionary<int, ClanWar> ClanWars = new ConcurrentDictionary<int, ClanWar>();

        public Clan(int id, string name, uint iconId, int maxusers, int win, int lose, int exp, string Announcment, string Description, string Master, string MasterEXP, long creation, bool created = false)
        {
            this.id = id;
            this.name = name;
            this.iconid = iconId;
            this.maxUsers = maxusers;
            this.win = win;
            this.lose = lose;
            this.exp = exp;
            this.Announcment = Announcment;
            this.Description = Description;
            this.Master = Master;
            this.MasterEXP = MasterEXP;
            this.creation = creation;

            loadUsers(id, created);

            loadClanWar(id);
        }

        public void loadUsers(int clanId, bool created)
        {
            DataTable dt = DB.RunReader("SELECT * FROM users WHERE clanid='" + clanId + "'");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                int clanrank = int.Parse(row["clanrank"].ToString());

                int userId = int.Parse(row["id"].ToString());
                string nickname = row["nickname"].ToString();
                string exp = row["exp"].ToString();
                string join = row["clanjoindate"].ToString();


                if (clanrank == -1 || clanrank == 9)
                {
                    ClanPendingUsers c = new ClanPendingUsers(userId, nickname, exp, join);
                    pendingUsers.TryAdd(userId, c);
                }
                else
                {
                    ClanUsers c = new ClanUsers(userId, nickname, exp, join, clanrank);
                    ClanUsers.TryAdd(userId, c);
                }

                if(clanrank == 2)
                {
                    this.Master = nickname;
                    this.MasterEXP = exp.ToString();
                }
            }

            if (dt.Rows.Count <= 0 && !created)
            {
                DB.RunQuery("DELETE FROM clans WHERE id='" + clanId + "'");
                DB.RunQuery("UPDATE users SET clanid='-1', clanrank='0' WHERE clanid='" + clanId + "'");
                Managers.ClanManager.Clans.Remove(id);
            }
        }

        public int clanRank(User u)
        {
            foreach (ClanUsers c in ClanUsers.Values)
            {
                if (c.id == u.userId)
                {
                    return c.clanrank;
                }
            }

            foreach (ClanPendingUsers c in pendingUsers.Values)
            {
                if (c.id == u.userId)
                {
                    return 9;
                }
            }

            return 0;
        }

        public void loadClanWar(int clanId)
        {
            DataTable dt = DB.RunReader("SELECT * FROM clans_clanwars WHERE clanid1='" + clanId + "' OR clanid2='" + clanId + "' ORDER BY timestamp DESC LIMIT 0, 3");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    DataRow row = dt.Rows[i];

                    string vsclan = (row["clanid1"].ToString() == id.ToString() ? row["clanid2"] : row["clanid1"]).ToString();
                    string name = DB.RunReaderOnce("name", "SELECT * FROM clans WHERE id='" + vsclan + "'").ToString();

                    ClanWar clanwar = new ClanWar(i, name, row["score"].ToString(), row["clanwon"].ToString() == id.ToString() ? true : false);
                    ClanWars.TryAdd(i, clanwar);
                }
                catch { }
            }
        }

        //WonClan.AddClanWar(LoseClan.clanName, DerbScore + "-" + NIUScore, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Versus clan name</param>
        /// <param name="score">Final score</param>
        /// <param name="won">Has won battle</param>
        public void AddClanWar(string name, string score, bool won)
        {
            int length = ClanWars.Count;

            var cws = ClanWars.Values.ToArray();

            ClanWars.Clear();

            ClanWar cw = new ClanWar(0, name, score, won);

            for (int i = 0; i < (length > 1 ? 1 : length); i++)
            {
                ClanWars.TryAdd(i + 1, cws[i]);
            }
        }

        public void sendToClan(Packet p)
        {
            byte[] buffer = p.GetBytes();
            foreach (User u in Users.Values)
            {
                if (u != null)
                {
                    u.sendBuffer(buffer);
                }
            }
        }

        public ClanPendingUsers getPendingUser(int id)
        {
            if (pendingUsers.ContainsKey(id))
            {
                return (ClanPendingUsers)pendingUsers[id];
            }
            return null;
        }

        public ClanPendingUsers getPendingUser(string nickname)
        {
            return pendingUsers.Values.Where(x => string.Compare(x.nickname, nickname, true) == 0).First();
        }

        public ClanUsers GetUser(int id)
        {
            if (Users.ContainsKey(id))
            {
                return (ClanUsers)ClanUsers[id];
            }
            return null;
        }

        public ClanUsers GetUser(string nickname)
        {
            return ClanUsers.Values.Where(x => string.Compare(x.nickname, nickname, true) == 0).First();
        }

        public List<ClanPendingUsers> pusers()
        {
            return new List<ClanPendingUsers>(pendingUsers.Values);
        }

        public List<User> GetUsers()
        {
            return new List<User>(Users.Values);
        }

        public List<ClanUsers> getAllUsers()
        {
            return new List<ClanUsers>(ClanUsers.Values);
        }
    }
}
