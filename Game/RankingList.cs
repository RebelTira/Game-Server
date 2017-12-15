using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;

namespace Game_Server.Game
{
    class RankingList
    {
        internal class User
        {
            public uint id, kills, exp, deaths, wins, loses;
            public int claniconid;
            public string nickname, clanname;
        }
        internal class Clan
        {
            public uint id, wins, loses, usercount, exp;
            public int claniconid;
            public string name;
            public int GetRank()
            {
                if (usercount >= 81) return (int)Game_Server.Clan.Rank.Corps;
                else if (usercount >= 61) return (int)Game_Server.Clan.Rank.Division;
                else if (usercount >= 51) return (int)Game_Server.Clan.Rank.Brigade;
                else if (usercount >= 41) return (int)Game_Server.Clan.Rank.Regiment;
                else if (usercount >= 31) return (int)Game_Server.Clan.Rank.Battalion;
                else if (usercount >= 21) return (int)Game_Server.Clan.Rank.Company;
                else if (usercount >= 11) return (int)Game_Server.Clan.Rank.Platoon;
                else if (usercount >= 6) return (int)Game_Server.Clan.Rank.Squad;
                else return (int)Game_Server.Clan.Rank.Recon;
            }
        }
        public static short hour = -1;
        public static List<User> UserByEXP = new List<User>();
        public static List<User> UserByWins = new List<User>();
        public static List<User> UserByKills = new List<User>();

        public static List<Clan> ClanByEXP = new List<Clan>();
        public static List<Clan> ClanByWins = new List<Clan>();
        public static List<Clan> ClanByMembers = new List<Clan>();

        public static void Load()
        {
            if (hour == DateTime.Now.Hour) return;
            hour = (short)DateTime.Now.Hour;

            UserByEXP.Clear();
            UserByWins.Clear();
            UserByKills.Clear();

            ClanByEXP.Clear();
            ClanByWins.Clear();
            ClanByMembers.Clear();

            DataTable dt = DB.RunReader("SELECT * FROM users WHERE rank < 4 AND rank > 0 AND banned != '1' ORDER BY exp DESC LIMIT 0, 100");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row != null)
                {
                    RankingList.User usr = new RankingList.User();
                    usr.nickname = row["nickname"].ToString();
                    int clanId = int.Parse(row["clanid"].ToString());
                    Game_Server.Clan c = Managers.ClanManager.GetClan(clanId);
                    if (clanId >= 0 && c != null)
                    {
                        usr.clanname = c.name;
                        usr.claniconid = (int)c.iconid;
                    }
                    else
                    {
                        usr.clanname = "NULL";
                        usr.claniconid = -1;
                    }
                    usr.exp = uint.Parse(row["exp"].ToString());
                    usr.kills = uint.Parse(row["kills"].ToString());
                    usr.deaths = uint.Parse(row["deaths"].ToString());
                    usr.wins = uint.Parse(row["wonMatchs"].ToString());
                    usr.loses = uint.Parse(row["lostMatchs"].ToString());

                    UserByEXP.Add(usr);
                }
            }
            dt = DB.RunReader("SELECT * FROM users WHERE rank < 4 AND rank > 0 AND banned != '1' ORDER BY wonMatchs DESC LIMIT 0, 100");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row != null)
                {
                    RankingList.User usr = new RankingList.User();
                    usr.nickname = row["nickname"].ToString();
                    int clanId = int.Parse(row["clanid"].ToString());
                    Game_Server.Clan c = Managers.ClanManager.GetClan(clanId);
                    if (clanId >= 0 && c != null)
                    {
                        usr.clanname = c.name;
                        usr.claniconid = (int)c.iconid;
                    }
                    else
                    {
                        usr.clanname = "NULL";
                        usr.claniconid = -1;
                    }
                    usr.exp = uint.Parse(row["exp"].ToString());
                    usr.kills = uint.Parse(row["kills"].ToString());
                    usr.deaths = uint.Parse(row["deaths"].ToString());
                    usr.wins = uint.Parse(row["wonMatchs"].ToString());
                    usr.loses = uint.Parse(row["lostMatchs"].ToString());

                    UserByWins.Add(usr);
                }
            }

            dt = DB.RunReader("SELECT * FROM users WHERE rank < 4 AND rank > 0 AND banned != '1' ORDER BY kills DESC LIMIT 0, 100");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row != null)
                {
                    RankingList.User usr = new RankingList.User();
                    usr.nickname = row["nickname"].ToString();
                    int clanId = int.Parse(row["clanid"].ToString());
                    Game_Server.Clan c = Managers.ClanManager.GetClan(clanId);
                    if (clanId >= 0 && c != null)
                    {
                        usr.clanname = c.name;
                        usr.claniconid = (int)c.iconid;
                    }
                    else
                    {
                        usr.clanname = "NULL";
                        usr.claniconid = -1;
                    }
                    usr.exp = uint.Parse(row["exp"].ToString());
                    usr.kills = uint.Parse(row["kills"].ToString());
                    usr.deaths = uint.Parse(row["deaths"].ToString());
                    usr.wins = uint.Parse(row["wonMatchs"].ToString());
                    usr.loses = uint.Parse(row["lostMatchs"].ToString());

                    UserByKills.Add(usr);
                }
            }


            dt = DB.RunReader("SELECT * FROM clans ORDER BY exp DESC LIMIT 0, 100");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row != null)
                {
                    RankingList.Clan clan = new RankingList.Clan();
                    clan.id = uint.Parse(row["iconid"].ToString());
                    clan.name = row["name"].ToString();
                    clan.claniconid = int.Parse(row["iconid"].ToString());
                    clan.wins = uint.Parse(row["win"].ToString());
                    clan.loses = uint.Parse(row["lose"].ToString());
                    clan.exp = uint.Parse(row["exp"].ToString());
                    clan.usercount = uint.Parse(row["count"].ToString());

                    ClanByEXP.Add(clan);
                }
            }


            dt = DB.RunReader("SELECT * FROM clans ORDER BY win DESC LIMIT 0, 100");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row != null)
                {
                    RankingList.Clan clan = new RankingList.Clan();
                    clan.id = uint.Parse(row["iconid"].ToString());
                    clan.name = row["name"].ToString();
                    clan.claniconid = int.Parse(row["iconid"].ToString());
                    clan.wins = uint.Parse(row["win"].ToString());
                    clan.loses = uint.Parse(row["lose"].ToString());
                    clan.exp = uint.Parse(row["exp"].ToString());
                    clan.usercount = uint.Parse(row["count"].ToString());

                    ClanByWins.Add(clan);
                }
            }


            dt = DB.RunReader("SELECT * FROM clans ORDER BY count DESC LIMIT 0, 100");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row != null)
                {
                    RankingList.Clan clan = new RankingList.Clan();
                    clan.id = uint.Parse(row["iconid"].ToString());
                    clan.name = row["name"].ToString();
                    clan.claniconid = int.Parse(row["iconid"].ToString());
                    clan.wins = uint.Parse(row["win"].ToString());
                    clan.loses = uint.Parse(row["lose"].ToString());
                    clan.exp = uint.Parse(row["exp"].ToString());
                    clan.usercount = uint.Parse(row["count"].ToString());

                    ClanByMembers.Add(clan);
                }
            }
        }
    }

    class SP_MyRank : Packet
    {
        public SP_MyRank(User usr)
        {
            newPacket(30816);
            addBlock(2);
            addBlock(usr.clan == null ? 1 : 2); // 1 = No clan, 2 = With clan
            addBlock(1); // rank
            addBlock(1);
            addBlock(usr.exp);
            addBlock(usr.kills);
            addBlock(usr.deaths);
            addBlock(usr.wonMatchs);
            addBlock(usr.lostMatchs);
            addBlock(usr.clan != null ? (int)usr.clan.iconid : -1);
            addBlock(usr.nickname);
            addBlock(usr.clan != null ? usr.clan.name : "NULL");
            if (usr.clan != null)
            {
                addBlock(4); // rank

                addBlock((usr.clan.maxUsers / 20) - 1);
                addBlock(usr.clan.exp);
                addBlock(usr.clan.win);
                addBlock(usr.clan.lose);
                addBlock(usr.clan.ClanUsers.Count);
                addBlock(usr.clan.iconid);
                addBlock(usr.clan.name);
                addBlock(usr.clan.GetRank());
            }
        }
    }

    class SP_RankingList : Packet
    {
        public SP_RankingList (ushort tab, ushort page, ushort sortType)
        {
            newPacket(30816);
            addBlock(tab);
            addBlock(page);
            addBlock(sortType);

            if (tab == 0) // User
            {
                List<RankingList.User> type = new List<RankingList.User>();
                switch (sortType)
                {
                    case 0:
                        {
                            type = RankingList.UserByEXP;
                            break;
                        }
                    case 1:
                        {
                            type = RankingList.UserByWins;
                            break;
                        }
                    case 2:
                        {
                            type = RankingList.UserByKills;
                            break;
                        }
                }

                ushort maxPages = (ushort)(type.Count / 10);
                if (page >= maxPages) page = maxPages;

                int index = page * 10;

                int pCount = (type.Count - index) > 10 ? 10 : (type.Count - index);
                if (pCount < 0) pCount = 0;

                addBlock(pCount);

                for (int i = index; i < index + pCount; i++)
                {
                    RankingList.User u = type[i];
                    if (u != null)
                    {
                        addBlock(i + 1);
                        addBlock(100);
                        addBlock(u.exp);
                        addBlock(u.kills);
                        addBlock(u.deaths);
                        addBlock(u.wins);
                        addBlock(u.loses);
                        addBlock(u.claniconid);
                        addBlock(u.nickname);
                        addBlock(u.clanname);
                    }
                }
            }
            else if (tab == 1) // Clan
            {
                List<RankingList.Clan> type = new List<RankingList.Clan>();
                switch (sortType)
                {
                    case 0:
                        {
                            type = RankingList.ClanByEXP;
                            break;
                        }
                    case 1:
                        {
                            type = RankingList.ClanByWins;
                            break;
                        }
                    case 2:
                        {
                            type = RankingList.ClanByMembers;
                            break;
                        }
                }

                ushort maxPages = (ushort)(type.Count / 10);
                if (page >= maxPages) page = maxPages;
                int index = page * 10;

                int pCount = (type.Count - index) > 10 ? 10 : (type.Count - index);
                if (pCount < 0) pCount = 0;

                addBlock(pCount);

                for (int i = index; i < index + pCount; i++)
                {
                    RankingList.Clan u = type[i];
                    if (u != null)
                    {
                        addBlock(i + 1);
                        addBlock(u.GetRank());
                        addBlock(u.exp);
                        addBlock(u.wins);
                        addBlock(u.loses);
                        addBlock(u.usercount);
                        addBlock(u.claniconid);
                        addBlock(u.name);
                    }
                }
            }
        }
    }

    class CP_RankingList : Handler
    {
        public override void Handle(User usr)
        {
            usr.send(new SP_MyRank(usr));

            ushort tab = ushort.Parse(getBlock(0));
            ushort page = ushort.Parse(getBlock(1));
            ushort sortType = ushort.Parse(getBlock(2));

            usr.send(new SP_RankingList(tab, page, sortType));
        }
    }
}
