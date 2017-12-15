using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Game_Server.Game;

namespace Game_Server.Managers
{
    class ClanManager
    {
        public static Dictionary<int, Clan> Clans = new Dictionary<int, Clan>();

        public static void loadClan(int id)
        {
            try
            {
                DataTable dt = DB.RunReader("SELECT name, maxusers, win, lose, exp, announcment, description, iconid FROM clans WHERE id=" + id.ToString());
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    string Announcment = row["announcment"].ToString();
                    string Description = row["description"].ToString();

                    string master = "??";
                    string masterexp = "0";
                    
                    //DataTable mdt = DB.RunReader("SELECT * FROM users WHERE clanid='" + id + "' AND clanrank='2'");
                    //if (mdt.Rows.Count > 0)
                    //{
                    //    DataRow mrow = mdt.Rows[0];
                    //    master = mrow["nickname"].ToString();
                    //    masterexp = mrow["exp"].ToString();
                    //}

                    Clan Clan = new Clan(id, row["name"].ToString(), uint.Parse(row["iconid"].ToString()), int.Parse(row["maxusers"].ToString()), int.Parse(row["win"].ToString()), int.Parse(row["lose"].ToString()), int.Parse(row["exp"].ToString()), Announcment, Description, master, masterexp, long.Parse(row["creationtime"].ToString()));

                    Clans.Add(id, Clan);
                }
            }
            catch { }
        }

        public static void Load()
        {
            try
            {
                Clans.Clear();
                DataTable dt = DB.RunReader("SELECT * FROM clans");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    int id = int.Parse(row["id"].ToString());
                    string Announcment = row["announcment"].ToString();
                    string Description = row["description"].ToString();

                    string master = "??";
                    string masterexp = "0";

                    //DataTable mdt = DB.RunReader("SELECT * FROM users WHERE clanid='" + id + "' AND clanrank='2'");
                    //if (mdt.Rows.Count > 0)
                    //{
                    //    DataRow mrow = mdt.Rows[0];
                    //    master = mrow["nickname"].ToString();
                    //    masterexp = mrow["exp"].ToString();
                    //}

                    Clan Clan = new Clan(id, row["name"].ToString(), uint.Parse(row["iconid"].ToString()), int.Parse(row["maxusers"].ToString()), int.Parse(row["win"].ToString()), int.Parse(row["lose"].ToString()), int.Parse(row["exp"].ToString()), Announcment, Description, master, masterexp, Generic.timestamp);

                    Clans.Add(id, Clan);
                }
                Log.WriteLine("Successfully loaded [" + dt.Rows.Count + "] Clans");
            }
            catch
            {
                Log.WriteError("Error while loading Clans");
            }
        }

        public static void CheckForDuplicate(User usr, string ClanName)
        {
            try
            {
                int count = ClanManager.Clans.Values.Where(v => string.Compare(v.name, ClanName, true) == 0).Count();
                if (count > 0)
                {
                    usr.send(new SP_Clan.CheckClan(SP_Clan.CheckClan.ErrorCodes.Exist));
                }
                else
                {
                    usr.send(new SP_Clan.CheckClan(SP_Clan.CheckClan.ErrorCodes.NotExist));
                }
            }
            catch
            { }
        }

        public static int GetClanRank(int ID)
        {
            try
            {
                int myExp = Clans[ID].exp;
                int index = 0;
                foreach (Clan clan in Clans.Values.Where(c => c != null))
                {
                    index++;
                    if (clan.exp <= myExp)
                    {
                        return index;
                    }
                }
            }
            catch { }

            return 0;
        }

        public static Clan GetClan(int ID)
        {
            try
            {
                if (Clans.ContainsKey(ID))
                {
                    return (Clan)Clans[ID];
                }
                return null;
            }
            catch { return null; }
        }

        public static Clan GetClanByName(string cl)
        {
            try
            {
                return Clans.Values.Where(v => string.Compare(v.name, cl, true) == 0).First();
            }
            catch { return null; }
        }

        public static void AddClan(User usr, string Name)
        {
            try
            {
                int count = ClanManager.Clans.Values.Where(v => string.Compare(v.name, Name, true) == 0).Count();
                if (count == 0)
                {
                    uint price = (uint)Configs.Server.Clan.CreationCost;
                    int result = (int)(usr.dinar - price);
                    if (result > 0)
                    {
                        if (usr.clan == null) // If user hasn't a clan 
                        {
                            string ActualTime = DateTime.Now.ToString("dd/MM/yyyy");
                            DB.RunQueryNotAsync("INSERT INTO clans (name, maxusers, count, win, lose, description, announcment, iconid, creationtime) VALUES ('" + DB.Stripslash(Name) + "', '20', '1', '0', '0', 'Welcome on Clan System!', 'Troopers, lets go to the attack!', '1001001', '" + Generic.timestamp + "')");

                            int clanId = int.Parse(DB.RunReaderOnce("id", "SELECT * FROM clans WHERE name='" + Name + "'").ToString());
                            DB.RunQuery("UPDATE users SET clanid='-1' WHERE clanid='" + clanId + "'");
                            Clan c = new Clan(clanId, Name, 1001001, 20, 0, 0, 0, "Welcome on Clan System", "Troopers, lets go to the attack!", usr.nickname, usr.exp.ToString(), Generic.timestamp, true);
                            Clans.Add(clanId, c);
                            usr.clan = c;
                            usr.dinar -= (int)price;
                            c.Users.TryAdd(usr.userId, usr);
                            c.ClanUsers.TryAdd(usr.userId, new ClanUsers(usr.userId, usr.nickname, usr.exp.ToString(), ActualTime, 2));
                            DB.RunQuery("UPDATE users SET dinar=" + usr.dinar + ", clanid='" + clanId + "', clanrank='2', clanjoindate='" + ActualTime + "' WHERE id='" + usr.userId + "'");
                            usr.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Whisper, "SYSTEM >> Successfully created the clan (" + Name + ")!", usr.sessionId, usr.nickname));
                            usr.send(new SP_Clan.CreateClan(Name, clanId, (uint)usr.dinar));
                        }
                        else // else if user has already a clan
                        {
                            int clanrank = usr.clan.clanRank(usr);
                            string Rank = (clanrank == 2 ? "own" : "are in"); // Calculate if is a master or an normal clan member
                            usr.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Whisper, "SYSTEM >> Cannot create the clan because you " + Rank + " a clan!", usr.sessionId, usr.nickname));
                        }
                    }
                    else
                    {
                        usr.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Whisper, "SYSTEM >> You havent enough dinars (" + price.ToString("N0") + " needed!)", usr.sessionId, usr.nickname));
                    }
                }
                else
                {
                    usr.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Whisper, "SYSTEM >> Cannot create the clan because this clan name is already in use!", usr.sessionId, usr.nickname));
                }
            }
            catch { }
        }

        public static void RemoveClan(User usr)
        {
            try
            {
                if (usr.clan != null) // If user has a clan
                {
                    int clanrank = usr.clan.clanRank(usr);
                    if (clanrank == 2) // If user is the master
                    {
                        byte[] buffer = (new SP_Chat("ClanSystem", SP_Chat.ChatType.Clan, "ClanSystem >> " +usr.nickname + " disbanded the clan :(", (uint)usr.clan.id, "NULL")).GetBytes();


                        Clan c = usr.clan;
                        Clans.Remove(c.id);

                        usr.send(new SP_Clan(SP_Clan.ClanCodes.DisbandClan));
                        DB.RunQuery("DELETE FROM clans WHERE id='" + usr.clan.id + "'");
                        DB.RunQuery("DELETE FROM clans_clanwars WHERE clanid1='" + usr.clan.id + "' OR clanid2='" + usr.clan.id + "'");
                        DB.RunQuery("UPDATE users SET clanid='-1', clanrank='0' WHERE clanid='" + usr.clan.id + "'");

                        foreach (User u in usr.clan.Users.Values)
                        {
                            u.sendBuffer(buffer);
                            u.clan = null;
                        }

                        c = null;
                        return;
                    }
                    else // If user has clan but isn't the master
                    {
                        usr.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Whisper, "SYSTEM >> Cannot delete the clan because you're not the master!", usr.sessionId, usr.nickname));
                    }
                }
            }
            catch { }
        }

        public static void UpgradeClan(User usr)
        {
            try
            {
                if (usr.clan == null)
                {
                    usr.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Whisper, "SYSTEM >> You doesn't own a clan!", usr.sessionId, usr.nickname));
                }
                else
                {
                    int clanrank = usr.clan.clanRank(usr);
                    if (clanrank != 2)
                    {
                        usr.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Whisper, "SYSTEM >> You're not the owner of the clan!", usr.sessionId, usr.nickname));
                    }
                    else if (usr.cash - 10000 < 0)
                    {
                        usr.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Whisper, "SYSTEM >> Not enough money!", usr.sessionId, usr.nickname));
                    }
                    else if (usr.clan.maxUsers >= 100)
                    {
                        usr.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Whisper, "SYSTEM >> Your clan cannot be extended more!!", usr.sessionId, usr.nickname));
                    }
                    else
                    {
                        int MaxUsers = usr.clan.maxUsers;
                        usr.clan.maxUsers += 20;

                        byte[] buffer = (new SP_Chat("ClanSystem", SP_Chat.ChatType.Clan, "ClanSystem >> " + usr.nickname + " has upgraded the clan slots from " + MaxUsers + " to " + usr.clan.maxUsers + ":)!", (uint)usr.clan.id, "NULL")).GetBytes();

                        foreach (User u in usr.clan.Users.Values)
                        {
                            u.sendBuffer(buffer);
                        }

                        DB.RunQuery("UPDATE clans SET maxusers=maxusers+20 WHERE id='" + usr.clan.id + "'");
                    }
                }
            }
            catch { }
        }
    }
}
