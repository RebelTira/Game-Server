using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

using System.Linq;
using Game_Server.Managers;
using Game_Server.Game;

namespace Game_Server.Managers
{
    class RoutineManager
    {
        private static int ActualDay = 0;
        private static uint LastTick = 0;
        private static List<int> users = new List<int>();
        private static List<int> ResetRandomBoxIDs = new List<int>();

        private static Thread DailyCheckThread, UserRoutineThread;

        public static void Load()
        {
            ActualDay = DateTime.Now.Day;

            UserRoutineThread = new Thread(UserRoutine);
            UserRoutineThread.Start();
            
            DailyCheckThread = new Thread(DailyCheck);
            DailyCheckThread.Start();
        }

        private static void UserRoutine()
        {
            while (true)
            {
                foreach (User usr in UserManager.ServerUsers.Values)
                {
                    try
                    {
                        if (Configs.Server.Player.CouponEvent)
                        {
                            if (usr.room != null) { if (usr.room.gameactive) { usr.coupontime += 5; } }

                            if (usr.todaycoupons < 5 && usr.coupontime >= 1800)
                            {
                                usr.todaycoupons++;
                                usr.coupons++;
                                usr.coupontime = 0;
                                DB.RunQuery(string.Format("UPDATE users SET coupons='{0}', todaycoupon='{1}' WHERE id='{2}'", usr.coupons, usr.todaycoupons, usr.userId));
                                usr.send(new SP_CouponEvent(usr.todaycoupons, usr.coupons));
                            }
                        }

                        if (Configs.Server.AntiCheat.enabled && usr.AntiCheatTick <= Generic.timestamp && !Configs.Server.Debug)
                        {
                            if (!usr.AntiCheatCheck && usr.channel != -1)
                            {
                                Log.WriteError(usr.nickname + " has been kicked out from " + Configs.Server.AntiCheat.name + " [ NO FUNCTION RECEIVED ]");
                                usr.disconnect();
                            }

                            if (usr.hwid == "")
                            {
                                Log.WriteError(usr.nickname + " has been kicked out from " + Configs.Server.AntiCheat.name + " [ NO HWID ]");
                                usr.disconnect();
                            }
                        }

                        if (Configs.Server.RandomBoxEvent.Enabled && DateTime.Now.Hour == Configs.Server.RandomBoxEvent.hour)
                        {
                            if (!usr.RandomBoxToday)
                            {
                                string item = Configs.Server.RandomBoxEvent.BoxCode;
                                usr.RandomBoxToday = true;
                                Inventory.AddItem(usr, item, -1);
                                usr.send(new SP_RandomBoxEvent(usr, item));
                                if (!ResetRandomBoxIDs.Contains(usr.userId))
                                {
                                    ResetRandomBoxIDs.Add(usr.userId);
                                }
                            }
                        }
                        else if (Configs.Server.ChristmasBoxEvent.Enabled && DateTime.Now.Hour == Configs.Server.ChristmasBoxEvent.hour)
                        {
                            if (!usr.RandomBoxToday)
                            {
                                string item = Configs.Server.ChristmasBoxEvent.BoxCode;
                                usr.RandomBoxToday = true;
                                Inventory.AddItem(usr, item, -1);
                                usr.send(new SP_RandomBoxEvent(usr, item));
                                if (!ResetRandomBoxIDs.Contains(usr.userId))
                                {
                                    ResetRandomBoxIDs.Add(usr.userId);
                                }
                            }
                        }
                    }
                    catch { }
                }

                if (Configs.Server.RandomBoxEvent.Enabled)
                {
                    string querystr = "";

                    int c = ResetRandomBoxIDs.Count;
                    if (c > 0)
                    {
                        querystr = string.Join(",", ResetRandomBoxIDs.Select(x => x.ToString()).ToArray());

                        DB.RunQuery("UPDATE users SET randombox='1' WHERE id IN (" + querystr + ")");
                        ResetRandomBoxIDs.Clear();
                    }
                }

                Thread.Sleep(5000);
            }
        }

        static object obj = new object();

        private static void DailyCheck()
        {
            while (true)
            {
                if (ActualDay != DateTime.Now.Day)
                {
                    ActualDay = DateTime.Now.Day;
                    DB.RunQuery("DELETE FROM users_events WHERE permanent = '0'");
                    DB.RunQuery("UPDATE users SET todaycoupon = '0', coupontime = '0', killcount='0', randombox = '0', loginEventToday = '0'");
                    DB.RunQuery("UPDATE users SET loginEventProgress = '0' WHERE loginEventProgress = '7'");

                    List<int> loginEventResetUsers = new List<int>();

                    foreach (User u in UserManager.ServerUsers.Values)
                    {
                        u.dailystats = false;
                        u.todaycoupons = 0;
                        u.coupontime = 0;
                        u.eventcount = 0;
                        u.rewardEvent.doneToday = false;
                        if (u.rewardEvent.progress >= 7)
                        {
                            u.rewardEvent.progress = 0;
                            loginEventResetUsers.Add(u.userId);
                            DB.RunQuery("UPDATE users SET loginEventProgress = '0' WHERE id = '" + u.userId+ "'");
                        }
                        /*if(u.room != null && u.room.HasChristmasMap && u.IsAlive())
                        {
                            u.send(new SP_KillCount(SP_KillCount.ActionType.Show, 0));
                        }*/
                        u.RandomBoxToday = false;
                    }

                    if (loginEventResetUsers.Count > 0)
                    {
                        string querystr = string.Join(",", users.Select(x => x.ToString()).ToArray());

                        DB.RunQuery("UPDATE users SET loginEventProgress = '0' WHERE id IN (" + querystr + ")");
                    }

                    Log.WriteError("Daily reset done at " + DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy"));
                }

                if(RankingList.hour != DateTime.Now.Hour)
                {
                    RankingList.Load();
                }

                GC.Collect();

                if (LastTick >= 10)
                {
                    LastTick = 0;
                    
                    DateTime current = DateTime.Now;
                    int ActualTime = int.Parse(String.Format("{0:yyMMddHH}", current));

                    lock (obj)
                    {
                        foreach (User usr in UserManager.ServerUsers.Values)
                        {
                            users.Add(usr.userId);
                        }

                        string querystr = "";

                        if (users.Count > 0)
                        {
                            querystr = string.Join(",", users.Select(x => x.ToString()).ToArray());

                            DB.RunQuery("UPDATE users SET online = '0', serverid = '-1' WHERE id NOT IN (" + querystr + ") AND serverid = '" + Configs.Server.serverId + "'");
                        }
                        users.Clear();
                    }

                    DB.RunQuery("UPDATE users SET banned = '0', bantime = '-1' WHERE banned = '1' AND bantime <= " + ActualTime + " AND bantime != '-1'");

                    BanManager.Load();

                    Log.WriteLine("10 minute routine done.");
                }
                LastTick++;

                DB.RunQuery("UPDATE serverinfo SET value = '" + Generic.runningSinceWeb + "' WHERE name = 'uptime'");
                DB.RunQuery("UPDATE serverinfo SET value = '" + Generic.timestamp + "' WHERE name = 'Lastupdate'");
                DB.RunQuery("UPDATE users SET premium = '0' WHERE premiumExpire < " + Generic.timestamp);

                Thread.Sleep(60000);
            }
        }
    }
}
