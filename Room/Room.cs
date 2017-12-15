/*
 _____   ___ __  __  _____ _____   ___  _  __              ___   ___   __    __ 
/__   \ /___\\ \/ /  \_   \\_   \ / __\( )/ _\            / __\ /___\ /__\  /__\
  / /\///  // \  /    / /\/ / /\// /   |/ \ \            / /   //  /// \// /_\  
 / /  / \_//  /  \ /\/ /_/\/ /_ / /___    _\ \          / /___/ \_/// _  \//__  
 \/   \___/  /_/\_\\____/\____/ \____/    \__/          \____/\___/ \/ \_/\__/  
__________________________________________________________________________________

Created by: ToXiiC
Thanks to: CodeDragon, Kill1212, CodeDragon

*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;


using Game_Server.Game;
using Game_Server.Managers;
using Game_Server.Room_Data;
using Game_Server.GameModes;
using System.Threading;

namespace Game_Server
{
    /// <summary>
    /// This class stores all informations about a room
    /// </summary>
    class Room : IDisposable
    {
        ~Room()
        {
            GC.Collect(); // Clear Garbage Collector
        }
        internal enum Side : int
        {
            Neutral = -1,
            Derbaran = 0,
            NIU = 1,
            Random = 2
        }

        public Thread updateThread;
        public Room()
        {
            disposed = false;
            this.voteKick = new VoteKick(this);
            updateThread = new Thread(update);
            updateThread.Start();
        }

        // Basic Room Informations

        public int id;
        public Channel ch;
        public int channel;
        public string name;
        public int enablepassword;
        public string password;
        public bool supermaster = false;
        public bool autostart = false;
        public bool userlimit = false;

        // Game Mode Informations

        public int mapid = 0;
        public int kills = 2;
        public int premiumonly = 0;
        public int status = 1; // 1 Waiting - 0 & 2 Playing
        public int mode = 0;
        public int type = 0;
        public int levellimit = 0;
        public int ping = 0;
        public int rounds = 3;
        public int ffakillpoints = 10;
        public int zombiedifficulty = 0;

        public int votekickOption = 0; // CP1/2 Creation Room
        public bool sleep = false; // Sleep room update

        public int timeleft = 180000;
        public int timespent = 0;
        public int[] flags = new int[32];

        public bool cwcheck = false;

        public bool gameactive = false;
        public bool bombPlanted = false;
        public bool bombDefused = false;
        public int explosiveRounds = 0;
        public int dmRounds = 0;

        public int new_mode = 0;
        public int new_mode_sub = 0;

        public int highestkills = 0;
        public int SpawnLocation = -1;

        public int KillsNIULeft = 10;
        public int KillsDerbaranLeft = 10;
        public int timelimit = 4;

        public int master = 0;
        public int maxusers = 0;

        public int RespawnVehicleCount = 120;

        public int waitExplosiveTime = 0;
        public bool isNewRound = false;
        public int SpawnedZombieplayers = 0;

        public bool EndGamefreeze = false; //>--- Se pone true al acabar el juego. Se pone false al pasar a status 1(Waiting). Si es true, no join
        public bool firstingame = false; //>--- True cuando entra el primero
        public bool firstspawn = false;

        public int DerbRounds = 0;
        public int NIURounds = 0;

        public int firstInGameTS = 0;
        public MapData MapData = null;
        
        // Explosive CQC Mode

        public int NIUExplosivePoints = 0;
        public int DerbExplosivePoints = 0;

        // Custom Informations
        public bool firstblood = false;

        public VoteKick voteKick;
        public ConcurrentDictionary<int, User> users = new ConcurrentDictionary<int, User>();
        public ConcurrentDictionary<int, User> spectators = new ConcurrentDictionary<int, User>();

        //>---  añadido para timeattack +++ 

        public Dictionary<string, int> SupplyBox = new Dictionary<string, int>();
        public int PowSupply = 0;
        public bool IsTimeOpenDoor = false;
        public int TimeToOpenDoor = 0;
        public int PowPlayer = 0; //Time Attack

        public bool Destructed = false;
        public bool BossKilled = false;
        public bool WeaponsGot = false;

        public bool firstStage = true;

        public int IntoPassing = 0;
        public int ReadyToNext = 0;
        public bool Newstage = false;
        public int timeAttackSpawns = 0;

        //>---  añadido para Escape +++ 
        public bool InHacking = false;
        public int EscapeHack = 10;
        public int ReverseCount = 20;
        public int HackingPause = 0;
        public int EscapeZombie = 0;
        public int EscapeHuman = 0;
         //>---  añadido para Escape ------------


        public void send(Packet p)
        {
            try
            {
                byte[] buffer = p.GetBytes();
                foreach (User usr in users.Values)
                {
                    usr.sendBuffer(buffer);
                }
                foreach (User usr in spectators.Values)
                {
                    usr.sendBuffer(buffer);

                }
            }
            catch { }
        }
        //>--- +++ 
        public int AliveUsers(int Side)
        {
            int Count = 0;
            foreach (User usr in users.Values)
                if (usr.Health > 0 && GetSide(usr) == Side && usr.Alive) Count++;
            return Count;
        }
        public int AliveEscape(bool IsZombie)
        {
            int Count = 0;
            foreach (User usr in users.Values)
                if (usr.Health > 0 && usr.IsEscapeZombie == IsZombie && usr.Alive) Count++;
            return Count;
        }
        //>--- +++ 
        public bool RemoveUser(int slotId) // --- Remover usuario
        {
            if (slotId >= 0 && users.ContainsKey(slotId))
            {
                User usr = GetUser(slotId);
                if (usr != null)
                {
                    int usrSide = GetSide(usr);
                    if (usr.currentVehicle != null)
                    {
                        usr.currentVehicle.Leave(usr);
                    }
                    //>--- Realmente esto va aqui ????? ------------------ Lo desactivo
                    /*
                    if (usr.channel == 2 || usr.channel == 3)
                    {
                        if (usr.isHacking)
                        {
                            usr.isHacking = false;
                            send(new SP_RoomHackMission(usr.roomslot, (usr.hackingBase == 0 ? HackPercentage.BaseA : HackPercentage.BaseB), 3, usr.hackingBase));
                        }
                        if (usr.hasC4)
                        {
                            send(new SP_Unknown(29985, 0, 0, 1, 0, 0, 0, 0, 0)); // Remove C4 from the user 
                            usr.hasC4 = false;
                            PickuppedC4 = false;
                        }
                    }
                    //>--- ---------------  ????? --------------------
                    */
                    if (channel == 3) // --- Zombies
                    {
                        usr.rKills = usr.rDeaths = usr.rHeadShots = -1;
                    }
                    else // --- Actualiza las estadisticas
                    {
                        usr.kills += usr.rKills;
                        usr.deaths += usr.rDeaths;
                        usr.headshots += usr.rHeadShots;
                    }

                    usr.SaveStats(); // Runs the query for update the user row
                    
                    int roomslot = usr.roomslot;

                    foreach (User u in users.Values)
                    {
                        if (u.lastKillUser == roomslot)
                        {
                            u.lastKillUser = -1;
                        }
                    }

                    usr.room = null;

                    usr.roomslot = -1;

                    users[slotId] = null;

                    User ur;

                    users.TryRemove(slotId, out ur);

                    if (slotId == master)
                    {
                        supermaster = false; /* Remove EXP Buff */
                        foreach (User vu in users.Values.OrderByDescending(r => r.premium).ThenByDescending(r => r.exp).ToArray())
                        {
                            if (master != vu.roomslot)
                            {
                                master = vu.roomslot;
                                break;
                            }
                        }
                    }

                    if (channel == 3 && users.Count >= 1) //>--- Si hay mas de un jugador en zombies ....
                    {
                        send(new SP_ZombieChangeTarget(this, roomslot));  //>---  para controlar los ZombieFollowers
                    }
                    /*else if (this.mode == (int)RoomMode.FFA && this.ffa != null && this.ffa.isGunGame)
                    {
                        this.ffa.GunGameLeave(usr);
                    }*/

                    send(new SP_LeaveRoom(usr, this, slotId, master)); // Send to the room about the user left

                    usr.send(new SP_LeaveRoom(usr, this, slotId, master));

                    usr.send(new SP_LobbyInfoUpdate(usr));

                    //usr.send(new SP_KillCount(SP_KillCount.ActionType.Hide));

                    if (status != 1 && users.Values.Where(u => u != null).Count() <= 1 && channel != 3)
                    {
                        Log.WriteInfo(">---Room-248 Si no esta en Waiting, solo queda un jugador y no es Zombies...  ");
                        EndGame();
                    }
                    else if (users.Values.Where(u => u != null).Count() <= 0)
                    {
                        disposed = true;
                        remove();
                    }

                    Managers.UserManager.UpdateUserlist(usr);

                    #region ClanWar

                    if (type == 1 && GetSideCount(usrSide) < 4 && cwcheck && gameactive && (DerbRounds >= 3 || NIURounds >= 3 || KillsDerbaranLeft <= kills - 5 || KillsNIULeft <= kills - 5))
                    {
                        cwcheck = false;
                        int vsClanSide = (usrSide == 1 ? 0 : 1);

                        Clan myClan = usr.clan;
                        Clan vsClan = GetClan(vsClanSide);

                        myClan.lose++;
                        DB.RunQuery("UPDATE clans SET lose=lose+1 WHERE id='" + vsClan.id + "'");
                        myClan.AddClanWar(vsClan.name, "0-0", false);

                        vsClan.win++;
                        vsClan.exp += 250;
                        DB.RunQuery("UPDATE clans SET win=win+1, exp=exp+250 WHERE id='" + vsClan.id + "'");
                        vsClan.AddClanWar(myClan.name, "0-0", true);

                        DB.RunQuery("INSERT INTO clans_clanwars (clanid1, clanid2, score, clanwon, timestamp) VALUES ('" + GetClan((int)Side.Derbaran).id + "', '" + GetClan((int)Side.NIU).id + "', '" + "0-0" + "', '" + vsClan.id + "', '" + Generic.timestamp + "')");
                    }
                    #endregion
                    Log.WriteInfo(">---Room-285 ch.UpdateLobby");
                    ch.UpdateLobby(this);

                    return true;
                }
            }

            send(new SP_LeaveRoom(0, this, slotId, master)); // Send to the room about the user left

            return false;
        }
        /// <summary>
        /// Get side of a user
        /// </summary>
        /// <param name="usr">User</param>
        /// <returns></returns>
        public int GetSide(User usr)
        { 	//>--- Modos para un solo jugador = return 0. El resto return 1 o 0
            if (usr.spectating || usr.channel == 3 && (mode == (int)RoomMode.Survival || mode == (int)RoomMode.Defense ||  mode == (int)RoomMode.TimeAttack)) return 0;
            return (usr.roomslot < (maxusers / 2) ? 0 : 1);
        }

        /// <summary>
        /// Get how many users are in the side
        /// </summary>
        /// <param name="side">Side id (0, 1)</param>
        /// <returns></returns>

        public int GetSideCount(int side)
        {
            return users.Values.Where(r => GetSide(r) == side).Count();
        }

        /// <summary>
        /// Returns users and spectators in a list array
        /// </summary>
        public List<User> tempPlayers
        {
            get
            {
                List<User> r = new List<User>();
                r.AddRange(users.Values);
                r.AddRange(spectators.Values);
                return r;
            }
        }

        /// <summary>
        /// Ends the game
        /// </summary>
        public void EndGame() 
        {
            if (EndGamefreeze || !gameactive) return;
            EndGamefreeze = true;
            gameactive = false;
            bombPlanted = false;
            bombDefused = false;
            timeleft = 180000;
            Log.WriteInfo(">---Room-388 EndGamefreeze=true ");
            List<User> tempPlayers = new List<User>();
            tempPlayers.AddRange(users.Values);
            tempPlayers.AddRange(spectators.Values);

            //>--- Score = Rounds si es Explosive o Kills si es otro modo
            int DerbScore = ((mode == (int)RoomMode.Explosive || mode == (int)RoomMode.HeroMode) ? DerbRounds : KillsDerbaranLeft);
            int NIUScore = ((mode == (int)RoomMode.Explosive || mode == (int)RoomMode.HeroMode) ? NIURounds : KillsNIULeft);
            int wonTeam = DerbScore > NIUScore ? 0 : (DerbScore < NIUScore ? 1 : -1); //>--- Gana Derb:0 - Gana Niu=1 - Si Zombies, -1
            Log.WriteInfo(">---Room-397 DerbScore: " + DerbScore + " - NIUScore: "+ NIUScore + " - wonTeam: "+ wonTeam);

            foreach (User usr in tempPlayers)
            {
                int explosivePoints = (GetSide(usr) == (int)Side.Derbaran ? DerbExplosivePoints : NIUExplosivePoints);

                /* PX's */

                double[] PremiumBonus = new double[] { 0, 0.20, 0.30, 0.50, 0.75 };

                bool HaveDoubleUp = usr.HasItem("CC05"); //>--- UP_DOUBLE
                bool Exp30Up = usr.HasItem("CD01"); //>--- EXP_30UP_SET
                bool Exp20Up = usr.HasItem("CD02"); //>--- EXP_20UP_SET
                bool Exp50Up = usr.HasItem("CD03"); //>--- EXP_50UP_SET
                bool Dinar30Up = usr.HasItem("CE02"); //>--- DINAR_30UP_SET
                bool Dinar20Up = usr.HasItem("CE01"); //>--- DINAR_20UP_SET
                bool Dinar50Up = usr.HasItem("CE03"); //>--- ???

                double DinarRate = (supermaster && master == usr.roomslot ? 1.10 : 1);
                double ExpRate = (supermaster ? 1.05 : 1);

                ExpRate += PremiumBonus[usr.premium];

                if (HaveDoubleUp)
                {
                    ExpRate += 0.25;
                    DinarRate += 0.25;
                }

                if (Exp20Up) ExpRate += 0.20;
                if (Exp30Up) ExpRate += 0.30;
                if (Exp50Up) ExpRate += 0.50;
                if (Dinar20Up) DinarRate += 0.20;
                if (Dinar30Up) DinarRate += 0.30;
                if (Dinar50Up) DinarRate += 0.50;

                /*if (mapid == 000 || mapid == 000) // Halloween event until 12 nov ...
                {
                    ExpRate += 0.10;
                }*/
                
                if (Configs.Server.Christmas.enabled && Configs.Server.Christmas.IsChristmas)
                {
                    ExpRate += Configs.Server.Christmas.ExpRate;
                    DinarRate += Configs.Server.Christmas.DinarRate;
                }

                ExpRate += Configs.Server.Experience.ExpRate;
                DinarRate += Configs.Server.Experience.DinarRate;

                if (EXPEventManager.isRunning)
                {
                    ExpRate += EXPEventManager.EXPRate;
                    DinarRate += EXPEventManager.DinarRate;
                }
                Log.WriteInfo(">---Room-449 mode: " + mode);
                if (this.mode == (int)RoomMode.Explosive)
                {
                    ExpRate += 2;
                    DinarRate += 2;
                }
                /*else if (this.mode == (int)RoomMode.FFA && this.ffa != null && this.ffa.isGunGame)
                {
                    this.ffa.GunGameLeave(usr);
                }*/

                if(mapid == 93)  //>--- MAPA EVENTO ------------ Actual - GuardPost
                {
                    ExpRate += 0.25;
                }
                
                if(usr.clan != null)
                {
                    #region Clan Rank EXP
                    switch (usr.clan.GetRank())
                    {
                        case (int)Clan.Rank.Squad:
                            {
                                ExpRate += 0.01;
                                DinarRate += 0.01;
                                break;
                            }
                        case (int)Clan.Rank.Platoon:
                            {
                                ExpRate += 0.02;
                                DinarRate += 0.02;
                                break;
                            }
                        case (int)Clan.Rank.Company:
                            {
                                ExpRate += 0.03;
                                DinarRate += 0.03;
                                break;
                            }
                        case (int)Clan.Rank.Battalion:
                            {
                                ExpRate += 0.04;
                                DinarRate += 0.04;
                                break;
                            }
                        case (int)Clan.Rank.Regiment:
                            {
                                ExpRate += 0.05;
                                DinarRate += 0.05;
                                break;
                            }
                        case (int)Clan.Rank.Brigade:
                            {
                                ExpRate += 0.06;
                                DinarRate += 0.06;
                                break;
                            }
                        case (int)Clan.Rank.Division:
                            {
                                ExpRate += 0.07;
                                DinarRate += 0.07;
                                break;
                            }
                        case (int)Clan.Rank.Corps:
                            {
                                ExpRate += 0.08;
                                DinarRate += 0.08;
                                break;
                            }
                    }
                    #endregion
                }
                int points = usr.rPoints + explosivePoints;
                Log.WriteInfo(">---Room-522 points: " + points);
                if (channel != 3) //>--- Si no es zombies
                {
                    usr.ExpEarned = 1 + (int)Math.Round(points * ExpRate);
                    usr.DinarEarned = 50 + (int)Math.Round((points / 2.5) * DinarRate);

                }
                else // if(zombie.Wave > 1) //>--- Cuentan los puntos a partir de la segunda oleada                  
                {

                    usr.ExpEarned = 1 + (int)Math.Round(1 + (points / 2.5) * ExpRate);
                    usr.DinarEarned = 50 + (int)Math.Round((points / 5) * DinarRate);
                    Log.WriteInfo(">---Room-534 channel 3 ExpEarned: " + usr.ExpEarned + " DinarEarned: " + usr.DinarEarned);
                }

                if (usr.ExpEarned > Configs.Server.Experience.MaxExperience) usr.ExpEarned = Configs.Server.Experience.MaxExperience; //>--- Limita la exp. ganada
                if (usr.DinarEarned > Configs.Server.Experience.MaxDinars) usr.DinarEarned = Configs.Server.Experience.MaxDinars; //>--- Limita los dinares ganados

                int actualLevel = usr.level;
                usr.exp += usr.ExpEarned;
                usr.dinar += usr.DinarEarned;
                Log.WriteInfo(">---Room-543 Exp: " + usr.exp + " - Dinar: "+ usr.dinar + " - Level: " + usr.level);
                int newLevel = usr.level;

                if (channel != 3) //>--- Actualiza estadisticas excepto en Zombies
                {
                    usr.kills += usr.rKills;
                    usr.deaths += usr.rDeaths;
                    usr.headshots = usr.rHeadShots++;
                }
                if (!usr.spectating)
                {
                    if (channel != 3)
                    {
                        //>--- Partidas ganadas o perdidas
                        if (mode != 1) //>--- NO.FFA
                        {
                            if (GetSide(usr) == wonTeam) //>--- Gana Derb:0 - Gana Niu=1 
                            {
                                usr.wonMatchs++;
                            }
                            else if (wonTeam != -1) //>---  Zombies, -1 - Si no es zombies... 
                            {
                                usr.lostMatchs++;
                            }
                        }
                        else //>--- FFA
                        {
                            if (usr.rKills >= highestkills)
                            {
                                usr.wonMatchs++;
                            }
                            else
                            {
                                usr.lostMatchs++;
                            }
                            Log.WriteInfo(">---Room-578 usr.wonMatchs: " + usr.wonMatchs + " - usr.lostMatchs: " + usr.lostMatchs );
                        }
                    }
                }

                if (usr.exp >= LevelCalculator.getExpForLevel(actualLevel + 1) && actualLevel < 101)
                {
                    List<LevelUPItem> Items = new List<LevelUPItem>();

                    int dinarPrice = (Configs.Server.Player.LevelupDinar * newLevel);
                    //>--- Premios por subir nivel
                    #region Prizes 

                    switch (newLevel)
                    {
                        case 13:
                            {
                                Items.Add(new LevelUPItem("D602", 7));
                                Items.Add(new LevelUPItem("CA04", 7));
                                Items.Add(new LevelUPItem("DS10", 7));
                                break;
                            }
                        case 23:
                            {
                                Items.Add(new LevelUPItem("D901", 7));
                                Items.Add(new LevelUPItem("CA04", 7));
                                Items.Add(new LevelUPItem("DS05", 7));
                                break;
                            }
                        case 29:
                            {
                                Items.Add(new LevelUPItem("DF35", 7));
                                Items.Add(new LevelUPItem("DG28", 7));
                                Items.Add(new LevelUPItem("DF14", 7));
                                break;
                            }
                        case 35:
                            {
                                Items.Add(new LevelUPItem("DG28", 15));
                                Items.Add(new LevelUPItem("DC70", 15));
                                Items.Add(new LevelUPItem("DS05", 30));
                                break;
                            }
                        case 41:
                            {
                                Items.Add(new LevelUPItem("DB10", 30));
                                Items.Add(new LevelUPItem("DC64", 15));
                                Items.Add(new LevelUPItem("DU02", 15));
                                Items.Add(new LevelUPItem("DS05", 15));
                                break;
                            }
                        case 48:
                            {
                                Items.Add(new LevelUPItem("DB10", 30));
                                Items.Add(new LevelUPItem("DF12", 30));
                                Items.Add(new LevelUPItem("DC76", 15));
                                Items.Add(new LevelUPItem("DF71", 15));
                                break;
                            }
                        case 55:
                            {
                                Items.Add(new LevelUPItem("DB17", 30));
                                Items.Add(new LevelUPItem("DH06", 30));
                                Items.Add(new LevelUPItem("DH03", 15));
                                Items.Add(new LevelUPItem("DJ93", 15));
                                break;
                            }
                        case 62:
                            {
                                Items.Add(new LevelUPItem("DF35", 7));
                                Items.Add(new LevelUPItem("DG13", 7));
                                Items.Add(new LevelUPItem("DJ33", 7));
                                Items.Add(new LevelUPItem("DC64", 7));
                                Items.Add(new LevelUPItem("DB16", 7));
                                break;
                            }
                        case 70:
                            {
                                Items.Add(new LevelUPItem("DF35", 15));
                                Items.Add(new LevelUPItem("DG13", 15));
                                Items.Add(new LevelUPItem("DJ33", 15));
                                Items.Add(new LevelUPItem("DC64", 15));
                                Items.Add(new LevelUPItem("DB16", 15));
                                break;
                            }
                        case 78:
                        case 86:
                        case 94:
                        case 100:
                            {
                                Items.Add(new LevelUPItem("DF35", 30));
                                Items.Add(new LevelUPItem("DG13", 30));
                                Items.Add(new LevelUPItem("DJ33", 30));
                                Items.Add(new LevelUPItem("DC64", 30));
                                Items.Add(new LevelUPItem("DB16", 30));
                                break;
                            }
                    }

                    foreach (LevelUPItem item in Items)
                    {
                        Inventory.AddItem(usr, item.Code, item.Days);
                    }

                    #endregion

                    #region Clan

                    if (usr.clan != null)
                    {
                        int clanrank = usr.clan.clanRank(usr);
                        if (clanrank == 2)
                        {
                            usr.clan.MasterEXP = usr.exp.ToString();
                        }
                        else if (clanrank != 9)
                        {
                            ClanUsers u = usr.clan.GetUser(usr.userId);
                            if (u != null)
                            {
                                u.EXP = usr.exp.ToString();
                            }
                        }
                        else
                        {
                            ClanPendingUsers u = usr.clan.getPendingUser(usr.userId);
                            if (u != null)
                            {
                                u.EXP = usr.clan.ToString();
                            }
                        }
                    }

                    #endregion

                    usr.dinar += dinarPrice;
                    usr.cash += Configs.Server.Player.LevelupCash * newLevel;
                    usr.send(new SP_LevelUp(usr, dinarPrice, Items));
                    DB.RunQuery("INSERT INTO levelups (userid, oldlevel, newlevel, premium, timestamp) VALUES ('" + usr.userId + "', '" + actualLevel + "', '" + newLevel + "', '" + usr.premium + "', '" + Generic.timestamp + "')");
                    Log.WriteLine(">----Room-717  " + usr.nickname + " ha subido nivel " + newLevel + " "); //>--- Subida de nivel
                }

                if (HasChristmasMap)
                {
                    usr.send(new SP_KillCount(SP_KillCount.ActionType.Hide));
                }

                usr.actualUserlistType = 0;
                usr.RefreshFriends();

                Log.WriteLine(usr.nickname + " -> Exp: " + usr.ExpEarned + "/ Dinars: " + usr.DinarEarned + " & ExpRate: " + ExpRate + " & points: " + points);
                DB.RunQuery("UPDATE users SET dinar = '" + usr.dinar + "', cash = '" + usr.cash + "' WHERE id = '" + usr.userId + "'");

                
                usr.send(new SP_LobbyInfoUpdate(usr));
				//>--- ----------------------------------------  Añade lo conseguido excepto los kills y deaths en zombies.
                usr.AddDailyStats(channel != 3 ? usr.rKills : 0, usr.channel != 3 ? usr.rDeaths : 0, channel != 3 ? usr.rHeadShots : 0, usr.ExpEarned, usr.DinarEarned);
                usr.SaveStats(); // Runs the query for update the user row
                Log.WriteInfo(">---Room-736 Estadisticas guardadas. EndGame ");
                usr.send(new SP_EndGame(usr));
            }

            NIUExplosivePoints = DerbExplosivePoints = 0;

            #region ClanWar

            if (type == 1 && users.Count >= 8 && cwcheck && (DerbRounds >= 3 || NIURounds >= 3 || KillsDerbaranLeft <= kills - 5 || KillsNIULeft <= kills - 5))
            {
                cwcheck = false;
                int winteam = (int)Side.Neutral;
                int winclanid = -1;

                string date = DateTime.Now.ToString("dd/MM/yyyy");

                Clan derbclan = GetClan((int)Side.Derbaran);
                Clan niuclan = GetClan((int)Side.NIU);

                if (derbclan != null && niuclan != null)
                {
                    if (DerbScore != NIUScore)
                    {
                        winteam = DerbScore > NIUScore ? (int)Side.Derbaran : (int)Side.NIU;
                    }

                    winclanid = DerbScore > NIUScore ? derbclan.id : niuclan.id;

                    for (int i = 0; i < 1; i++)
                    {
                        Clan clan = (i == (int)Side.Derbaran ? derbclan : niuclan);
                        Clan vsClan = (i == (int)Side.Derbaran ? niuclan : derbclan);
                        if (winteam != (int)Side.Neutral)
                        {
                            if (winclanid == clan.id)
                            {
                                clan.win++;
                                clan.exp += 1000;
                                DB.RunQuery("UPDATE clans SET win=win+1, exp=exp+1000 WHERE id='" + clan.id + "'");
                            }
                            else
                            {
                                clan.lose++;
                                clan.exp += 500;
                                DB.RunQuery("UPDATE clans SET lose=lose+1, exp=exp+500 WHERE id='" + clan.id + "'");
                            }
                            clan.AddClanWar(vsClan.name, DerbScore + "-" + NIUScore, winclanid == clan.id);
                        }
                        else
                        {
                            clan.exp += 250;
                            DB.RunQuery("UPDATE clans SET exp=exp+250 WHERE id='" + clan.id + "'");
                        }
                    }
                }

                DB.RunQuery("INSERT INTO clans_clanwars (clanid1, clanid2, score, clanwon, timestamp) VALUES ('" + derbclan.id + "', '" + niuclan.id + "', '" + DerbScore + "-" + NIUScore + "', '" + winclanid + "', '" + Generic.timestamp + "')");
            }




            #endregion
            if (channel != 3)
            {
                Log.WriteInfo(">---Room-801 SP_ScoreBoard. EndGame ");
                send(new SP_ScoreBoard(this));
            }
            else
            {
                Log.WriteInfo(">---Room-806 SP_ScoreBoard_AI. EndGame ");
                send(new SP_ScoreBoard_AI(this)); //>--- +++

            }

            send(new SP_Unknown(30000, 1, (int)Subtype.MapChange, master, this.id, 2, 1, 0, mapid, 0, 0, 0, 0, 0, 0, 0));

            highestkills = NIURounds = DerbRounds = KillsNIULeft = KillsDerbaranLeft = 0;

            status = 1; //>--- Waiting
            EndGamefreeze = false;
        } //>------------------------------------------------------ Fin de EndGame --------------------------------------

        /// <summary>
        /// Get a free roomslot (if it is available)
        /// </summary>
        /// <param name="side">Side (0, 1)</param>
        /// <returns></returns>
        public int FreeRoomSlotBySide(int side)
        {
            lock (this)
            {
                for (int i = (side == (int)Room.Side.NIU ? (maxusers / 2) : 0); i < (side == (int)Room.Side.NIU ? maxusers : (maxusers / 2)); i++)
                {
                    if (users.ContainsKey(i) == false)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// Reset user statistics
        /// </summary>
        /// <param name="usr">User</param>
        public void ResetUserStats(User usr)
        {
            usr.isReady = usr.isSpawned = usr.isHacking = usr.hasC4 = usr.ExplosiveAlive = usr.SelBox = false;
            usr.Health = 1000;
            usr.mapLoaded = false;
            usr.rKillSinceSpawn = 0;
            usr.rKills = usr.rDeaths = usr.rHeadShots = usr.rPoints = usr.rAssist = usr.weapon = usr.Class = usr.skillPoints = 0;
            usr.droppedAmmo = usr.droppedFlash = usr.droppedM14 = usr.droppedMedicBox = 0;
            usr.LastRepairTick = 0;
            usr.BoxChoose = -1;
            usr.LastHackTick = 0;
            usr.HPLossTick = 0;
            usr.classCode = "-1";
            usr.TotalWarPoint = 0;
            usr.currentVehicle = null;
            usr.currentSeat = null;
            usr.playing = false;
            usr.Alive = false; //>--- +++ 

            usr.lastKillUser = -1;
            usr.DinarEarned = usr.ExpEarned = 0;
            
            if (timeattack != null)
            {
                timeAttackSpawns = (zombiedifficulty > 0 ? 3 : 5); //>--- Numero de vidas del jugador. Easy=4, Hard=3
            }
            
            /*if (this.mode == (int)RoomMode.FFA && gameactive && ffa != null && ffa.isGunGame)
            {
                this.ffa.GunGameJoin(usr);
            }*/
        }

        /// <summary>
        /// Switch side of a user (0 to 1 / 1 to 0)
        /// </summary>
        /// <param name="usr">User</param>
        /// <returns></returns>
        public int SwitchSide(User usr)
        {
            if (status == 2) return -1;
            if (usr != null && usr.room != null && usr.room.id == this.id)
            {
                int oldSlot = usr.roomslot;
                int Side = GetSide(usr);
                usr.Alive = false;
                for (int I = (Side == 0 ? (maxusers / 2) : 0); I < (Side == 0 ? maxusers : (maxusers / 2)); I++)
                {
                    if (users.ContainsKey(I) == false)
                    {
                        if (usr.roomslot == master)
                            master = I;

                        return I;
                    }
                }
            }
            return -1;
        }

        public ConcurrentDictionary<int, Vehicle> Vehicles = new ConcurrentDictionary<int, Vehicle>();

        /// <summary>
        /// Spawn all vehicles
        /// </summary>
        private void SpawnVehicles()
        {
            if (MapData != null)
            {
                
                int VehicleCount = 0;
                Vehicles.Clear();
                if (MapData.vehicleString != null && MapData.vehicleString != string.Empty && MapData != null)
                {
                    string[] VehiclesCodes = MapData.vehicleString.Split(new char[] { ';' });
                    foreach (string sCode in VehiclesCodes)
                    {
                        VehicleManager VehicleInfo = VehicleManager.GetVehicleInfoByCode(sCode);
                        if (VehicleInfo != null)
                        {
                            Vehicle newVehicle = new Vehicle(VehicleCount, sCode, VehicleInfo.Name, VehicleInfo.MaxHealth, VehicleInfo.MaxHealth, VehicleInfo.RespawnTime, VehicleInfo.Seats, VehicleInfo.isJoinable);
                            Vehicles.TryAdd(VehicleCount, newVehicle);
                            VehicleCount++;
                            //Log.WriteInfo(">---Room-926 SpawnVehicles() - VehicleCount:  " + VehicleCount);
                        }
                        else
                        {
                            Log.WriteError("Could not find the vehicle with the code " + sCode + "!");
                        }
                    }
                }
            }
        }

        public void RespawnAllVehicles()
        {
            for (int I = 0; I < Vehicles.Count; I++)
            {

                RespawnVehicle(I);
            }
        }

        public Vehicle GetVehicleByID(int ID)
        {
            if (Vehicles.ContainsKey(ID))
            {
                return Vehicles[ID];
            }
            return null;
        }
  //>--- Mapas Navideños
		public bool HasChristmasMap
        {
            get
            {
                return false; // (mapid == 91 || mapid == 92 || mapid == 93 || mapid == 94 || mapid == 78);
            }
        } 


        public void RespawnVehicle(int ID)
        {
            
            Vehicle Vehicle = GetVehicleByID(ID);

            if (Vehicle != null)
            {
                if (Vehicle.Users.Count == 0)
                {
                    Log.WriteInfo(">---Room-973 SP_RoomRespawnVehicle ID: " + ID);
                    if (ID == GetIncubatorVehicleId() || (mapid == 42 && (Vehicle.Code == "EN02"))) //>--- +++  
                    { //>--- +++ Para que no se reinicie la incubadora con cada wave, ni las puertas de SiegeWar
                        return;
                    }
                    Vehicle.RespawnTick = 0;
                    Vehicle.SpawnProtection = 5;
                    Vehicle.LoadSeats(Vehicle.SeatString);
                    Vehicle.ChangedCode = string.Empty;
                    Vehicle.TimeWithoutOwner = 0;
                    
                    Vehicle.Health = Vehicle.MaxHealth;
                    send(new SP_RoomRespawnVehicle(ID, this));

                }
            }
        }

        #region TotalWar (To finish)

        public int TotalWarDerb = 0;
        public int TotalWarNIU = 0;
        #endregion

        #region Capture

        public int DerbaranPoints = 0;
        public int NIUPoints = 0;
        #endregion

        #region Zombie              //>--- ZOMBIES ---<

        public bool SendFirstWave = false;
        public bool FirstWaveSent = false;

        public bool zombieRunning = false;
        public int ZombiePoints = 0;
        public int SpawnedZombies = 0;

        public Dictionary<int, Zombie> Zombies = new Dictionary<int, Zombie>();



        public int spawnedztipo0 = 0;
        public int spawnedztipo1 = 0;
        public int spawnedztipo2 = 0;
        public int spawnedztipo3 = 0;

        public int spawnedMadmans = 0;
        public int spawnedManiacs = 0;
        public int spawnedGrinders = 0;
        public int spawnedGrounders = 0;
        public int spawnedHeavys = 0;
        public int spawnedGrowlers = 0;
        public int spawnedLovers = 0;
        public int spawnedHandgemans = 0;
        public int spawnedChariots = 0;
        public int spawnedCrushers = 0;
        //>--- +++
        public int spawnedBusters = 0;
        public int spawnedCrashers = 0;
        public int spawnedEnvys = 0;
        public int spawnedClaws = 0;
        public int spawnedBombers = 0;
        public int spawnedDefenders = 0;
        public int spawnedBreakers = 0;
        public int spawnedMadSoldiers = 0;
        public int spawnedMadPrisoners = 0;
        public int spawnedBreaker2s = 0;
        public int spawnedSuperHeavys = 0;
        public int spawnedLadys = 0;
        public int spawnedMidgets = 0;

        public int ztipo0 = 0;
        public int ztipo1 = 0;
        public int ztipo2 = 0;
        public int ztipo3 = 0;

        public int KilledZombies = 0;

        //public int SleepTime = 15;
        public int SleepTime = 15;  //>--- *** 15

        public int KillsBeforeDrop = 0;
        
        public int DropID = 0;

        public int ZombieID = 3; // 4 Slot Index but we start from 3 because on newwave we add plus before setting the zombie id

        public int GetIncubatorVehicleId()
        {
            if (channel == 3 && mode == (int)RoomMode.Defense) 
            {
                
                var vehicle = Vehicles.Values.Where(r => r.Code == "EN16"); //>--- FIXED_BRK_INCUBATOR
                if (vehicle.Count() > 0)
                {
                    //Log.WriteInfo(">---Room-1067 Defense-Vehicle: " + vehicle);
                    return vehicle.FirstOrDefault().ID;
                }
            }
            return -1;
        }
      
        public int ZombieSpawnPlace = 0;
        
        public Zombie GetAvailableZombie()
        {
            //Log.WriteInfo(">---Room-1078 - GetAvailableZombie ");
            foreach (Zombie z in Zombies.Values)
            {
               
                if(z.respawn < Generic.timestamp && z.Health == 0) //> Tiempo de respawn de los Zs 
                {
                    return z; //>--- Health debe ser 0 para return
                }
            }
            return null;
        }
        public void SpawnZombie(int Type)
        {
            Log.WriteInfo(">---Room-1091 - SpawnZombie Type: " + Type);
            lock (this)
            {
                /*
                int zn = (Zombies.Where(r => r.Value.Health > 0).Count());  // >--- +++ Numero de Zs con Salud => 0
                Log.WriteInfo(">---Room-1095 zn: " + zn);
                if (zn >= 20) { return; } // >--- ++ De momento ...
                */
                //>---  Añadir variable segun dificultad
                int d = zombiedifficulty > 0 ? 1 : 0 ; //>--- +++

                if (Type == 5 && Zombies.Values.Count(r => r.Type == 5 && r.Health > 0 ) >= 5 + d) return; //>--- Heavy
                if (Type == 6 && Zombies.Values.Count(r => r.Type == 6 && r.Health > 0) >= 3 + d) return; //>--- Lover
                if (Type == 8 && Zombies.Values.Count(r => r.Type == 8 && r.Health > 0) >= 2 + d) return; //>--- Chariot
                if (Type == 9 && Zombies.Values.Count(r => r.Type == 9 && r.Health > 0) >= 1 + d) return; //>--- Crusher
                if (Type == 10 && Zombies.Values.Count(r => r.Type == 10 && r.Health > 0) >= 2) return; //>--- Buster
                if (Type == 11 && Zombies.Values.Count(r => r.Type == 11 && r.Health > 0) >= 1) return; //>--- Crasher
                if (Type == 14 && Zombies.Values.Count(r => r.Type == 14 && r.Health > 0) >= 2) return; //>--- Bomber
                if (Type == 15 && Zombies.Values.Count(r => r.Type == 15 && r.Health > 0) >= 1) return; //>--- Defender
                if (Type == 16 && Zombies.Values.Count(r => r.Type == 16 && r.Health > 0) >= 1) return; //>--- Breaker
                /*
                if (Type == 5 && Zombies.Values.Count(r => r.Type == 5) >= 5+d) return; //>--- Heavy
                if (Type == 6 && Zombies.Values.Count(r => r.Type == 6) >= 3+d) return; //>--- Lover
                if (Type == 8 && Zombies.Values.Count(r => r.Type == 8) >= 2+d) return; //>--- Chariot
                if (Type == 9 && Zombies.Values.Count(r => r.Type == 9) >= 1+d) return; //>--- Crusher
                if (Type == 10 && Zombies.Values.Count(r => r.Type == 10) >= 2) return; //>--- Buster
                if (Type == 11 && Zombies.Values.Count(r => r.Type == 11) >= 1) return; //>--- Crasher
                if (Type == 14 && Zombies.Values.Count(r => r.Type == 14) >= 2) return; //>--- Bomber
                if (Type == 15 && Zombies.Values.Count(r => r.Type == 15) >= 1) return; //>--- Defender
                if (Type == 16 && Zombies.Values.Count(r => r.Type == 16) >= 1) return; //>--- Breaker
                */

                Zombie z = GetAvailableZombie();
                
                
                if (z != null) 
                {
                   
                    if (Type >= 0 && Type <= 22) // >--- Estaba a 9. Aumento a 22

                    {
                        if (ZombieSpawnPlace >= 12) ZombieSpawnPlace = 0;
                        //>--- +++ Para que salgan donde deben...
                        if (Type == 14) ZombieSpawnPlace = 4; //>--- Bomber
                        if (Type == 15) ZombieSpawnPlace = 5; //>--- Defender
                        if (this.mapid == 55 && Type == 16) ZombieSpawnPlace = 1; //>--- Breaker
                        if (Type == 10 && spawnedBusters % 2 != 0) ZombieSpawnPlace = 25; //>--- Impares
                        if (Type == 10 && spawnedBusters % 2 == 0) ZombieSpawnPlace = 26; //>--- Pares
                         Log.WriteInfo(">---Room-1126 Type: " + Type + " ZombieSpawnPlace: " + ZombieSpawnPlace);

                        int foll = (Zombies.Count >= 32 % users.Count && users.Count > 1 ? RandomTargetRoomSlot : master); //>--- Comentada en el original
                        z.FollowUser = foll;
                        z.timestamp = Generic.timestamp + 1;
                        z.Type = Type;
                        
                        z.Reset(); //>--- Pone las caracteristicas al zombie z

                        SpawnedZombies++;
                        ZombieSpawnPlace++;

                        send(new Game.SP_ZombieSpawn(z.ID, z.FollowUser, ZombieSpawnPlace, Type, z.Health));
                       
                       
                        /*
                        if (Type == 10) Log.WriteInfo(">---Room-1142 >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>  BUSTER ");
                        if (Type == 14) Log.WriteInfo(">---Room-1143 >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>  BOMBER ");
                        if (Type == 15) Log.WriteInfo(">---Room-1144 >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>  DEFENDER ");
                        if (Type == 16) Log.WriteInfo(">---Room-1145 >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>  BREAKER ");
                        */
                        // Log.WriteInfo(">---Room-1147 switch (Type): " + Type);
                        switch (Type)
                        {
                            case 0: spawnedMadmans++; break;
                            case 1: spawnedManiacs++; break;
                            case 2: spawnedGrinders++; break;
                            case 3: spawnedGrounders++; break;
                            case 4: spawnedGrowlers++; break;
                            case 5: spawnedHeavys++; break;
                            case 6: spawnedLovers++; break;
                            case 7: spawnedHandgemans++; break;
                            case 8: spawnedChariots++; break;
                            case 9: spawnedCrushers++; break;
                            case 10: spawnedBusters++; break;
                            case 11: spawnedCrashers++; break;
                            case 12: spawnedEnvys++; break;
                            case 13: spawnedClaws++; break;
                            case 14: spawnedBombers++; break;
                            case 15: spawnedDefenders++; break;
                            case 16: spawnedBreakers++; break; 
                            case 17: spawnedMadSoldiers++; break;
                            case 18: spawnedMadPrisoners++; break;
                            case 19: spawnedBreaker2s++; break;
                            case 20: spawnedSuperHeavys++; break;
                            case 21: spawnedLadys++; break;
                            case 22: spawnedMidgets++; break;

                        }
                        Log.WriteInfo(">---Room-1195 spawnedMadmans: " + spawnedMadmans + " spawnedManiacs: " +spawnedManiacs + "  spawnedLovers: " + spawnedLovers + "  spawnedHandgemans: " + spawnedHandgemans);
                        Log.WriteInfo(">---Room- spawnedGrinders: " + spawnedGrinders + " spawnedGrounders: " + spawnedGrounders + " spawnedGrowlers: " + spawnedGrowlers);
                        Log.WriteInfo(">---Room- spawnedHeavys: " + spawnedHeavys + " spawnedChariots: " + spawnedChariots + " spawnedCrushers: " + spawnedCrushers);
                    }
                }
            }
        }

        public Zombie GetZombieByID(int id)
        {
            if (Zombies.ContainsKey(id))
            {
               // Log.WriteInfo(">---Room-1187 Zombie ID: " + id);
                return (Zombie)Zombies[id];
            }
            return null;
        }
        public int RandomDrop() //>--- Drop item aleatorio
        {
            Random r = new Random();
            int Type = r.Next(1, 2);

            int Rand = r.Next(0, 400);

            if (Rand >= 300 && mode == (int)RoomMode.Defense) //>--- _
            {
                Type = 3; //>--- repair
            }
            else if (Rand >= 200)
            {
                Type = 2;//>--- ammo
            }
            else if (Rand >= 100)
            {
                Type = 1;//>--- medic
            }  //>---  Cambio Rand >= 0  x  Rand >= 200 orig.
            else if (Rand >= 200 && mode == (int)RoomMode.Survival && zombie.Wave >= 6 && !zombie.respawnThisWave)  
            {
                zombie.respawnThisWave = true; //>--- Drop Respawn si Wave >= 6 y no ha salido en esta wave
                Type = 0; //>--- respawn
            }
            Log.WriteInfo(">---Room-1216 Drop item: " + Type);
            return Type;
        }
        public List<Zombie> ZombieFollowers(int SlotID)
        {
            return Zombies.Values.Where(r => r != null && r.FollowUser == SlotID).ToList();
        }
        public int RandomTargetRoomSlot
        {
            get
            {
                for (int i = 0; i < maxusers; i++)
                {
                    if (users.ContainsKey(i) && i != master)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }

        #endregion

        #region SiegeWar & SiegeWar 2

        public int SiegeWarTime = -1;
        public bool PickuppedC4 = false;
        public HackPercentage HackPercentage = new HackPercentage();

        public User SiegeWarC4User = null;
        public string Mission1 = null;
        public string Mission2 = null;
        public string Mission3 = null;



        public void CheckForMission(User usr, int VehicleID)
        {
            //Log.WriteInfo(">---Room-1255 VehicleID: " + VehicleID);
            if (VehicleID == GetIncubatorVehicleId() && channel == 3 && mode == (int)RoomMode.Defense) //>--- _
            {
                //Log.WriteInfo(">---Room-1258 Defense-EndGame " );
                EndGame();
                return;
            }
            if (mapid == 42) // --- SiegeWar 
            {
                if (VehicleID == 8 || VehicleID == 9) //>--- Puertas grandes
                {
                    usr.rPoints += 25;
                    if (Mission1 == null)
                    {
                        Mission1 = usr.nickname;
                    }
                    flags[2] = 0;
                    flags[1] = 1;
                    send(new SP_Unknown(30000, 1, -1, id, 2, 156, 0, 1, 2, 0, 1, 1, 0, 20, 0, 0, 0, 705882, 637900, 705882, 0, 5600.8521, 287.8355, 5443.2065, 267.1544, -90.9612, -101.7575, 0, 0, "DS05")); //>--- DS_FLASH_MINE ??
                    send(new SP_Unknown(30000, 1, -1, id, 2, 156, 0, 1, 1, 1, -1, -1, 0, 20, 0, 0, 0, 705882, 637900, 705882, 0, 5600.8521, 287.8355, 5443.2065, 267.1544, -90.9612, -101.7575, 0, 0, "DS05"));
                }
                else if (VehicleID == 7) //>--- Controlador central
                {
                    usr.rPoints += 30;
                    if (Mission2 == null)
                    {
                        Mission2 = usr.nickname;
                    }
                    flags[1] = 0;
                    flags[3] = 1;
                    send(new SP_Unknown(30000, 1, -1, id, 2, 156, 0, 1, 1, 0, -1, 1, 0, 20, 0, 0, 0, 705882, 637900, 705882, 0, 5600.8521, 287.8355, 5443.2065, 267.1544, -90.9612, -101.7575, 0, 0, "DS05"));
                    send(new SP_Unknown(30000, 1, -1, id, 2, 156, 0, 1, 3, 1, -1, -1, 0, 20, 0, 0, 0, 705882, 637900, 705882, 0, 5600.8521, 287.8355, 5443.2065, 267.1544, -90.9612, -101.7575, 0, 0, "DS05"));
                }
                else if (VehicleID == 25 || VehicleID == 24 || VehicleID == 23) //>--- Controladores misil
                {
                    usr.rPoints += 50;
                    if (Mission3 == null)
                    {
                        Mission3 = usr.nickname;
                    }
                }
            }
            else if (mapid == 60) // --- SiegeWar2 
            {
                if (VehicleID == 0)//>--- Reactor central donde se pone el C4
                {
                    usr.rPoints += 50;
                    if (Mission2 == null)
                    {
                        Mission2 = usr.nickname;
                    }
                }
            }
            else if (timeattack != null)
            {
                //if (VehicleID == 5 || (VehicleID == 0 && mapid == 56)) //>--- Puerta grande final. 5 en PowCamp y 0 en HiddenCave
               if( usr.room.Destructed == true) 
                {
                    timeattack.end3 = true; //>---- +++
                    

                    Log.WriteInfo(">---Room-1316 DOOR EN17 Destructed ");
                    if (zombiedifficulty > 0) //>--- sigue para Stage 4
                    {
                        Log.WriteDebug(">--- Room-1316  -.- PrepareNewStage(3)  > end3 = true;  >  Ultima HARD ");
                       
                        timeattack.PrepareNewStage(3); //>--- Ultima sala si esta en modo dificil                     
                        timeattack.sleepBeforeEverything = 5; //>--- 10 Orig.
                        SleepTime = 5; 
                        Log.WriteDebug(">--- Room-1324  -.- PrepareNewStage(3)  > end3: " + timeattack.end3 +  " >  Ultima HARD ");
                    }
                    else
                    {
                        usr.room.Destructed = false; //>--- +++
                        Log.WriteDebug(">--- Room-1329 --- Final Stage 3 - EASY --- PACKET_TIMEATTACK_END(this) --- ");
                        send(new PACKET_TIMEATTACK_END(this)); // 1 - Choose your prize
                        timeattack.Stage = 0;
                        timeattack.final3 = true;
                        //EndGame();
                        return; //>--- Comento EndGame y añado return. Para que no finalice sin pasar por la eleccion de premio
                    }
                }
            }
        }
        public void SiegeWar2Explosion()
        {
            try
            {
                
                send(new SP_Unknown(29985, 0, 0, 1, 4, 0, 100, 0, 0)); // Call animation of explosion
                Vehicle Vehicle = GetVehicleByID(0);
                if (Vehicle != null)
                {
                  
                    int Damage = int.Parse(((Math.Truncate((double)(Vehicle.MaxHealth * 89))) / 100).ToString());

                    Vehicle.Health -= Damage;
                    send(new SP_Unknown(30000, 1, -1, id, 2, 104, 0, 1, 1, 0, 0, 92, 0, 92, -1, 0, 0, Vehicle.Health, Vehicle.Health, Vehicle.Health + Damage, 0, 2845.7510, 205.0797, 3374.0964, -70.9974, 45.4165, -287.9179, 0, 0, "DP05")); // Damage the 'vehicle'
                    send(new SP_Unknown(29985, 0, -1, 1, 5, -1, 0, -1, 0)); // Spawn the C4
                    if (Vehicle.Health <= 0)
                    {
                        if (Mission2 == null)
                            Mission2 = SiegeWarC4User.nickname;
                        SiegeWarC4User.rPoints += 50;
                        EndGame();
                        SiegeWarC4User = null;
                    }
                    SiegeWarTime = -1;
                }
            }
            catch { }
        }

        public int GetActualMission
        {
            get
            {
                int Count = 0;
                if (mapid == 42 || mapid == 60) //>--- SiegeWar 1 y 2
                {
                    if (Mission1 != null) Count++;
                    if (Mission2 != null) Count++;
                    if (Mission3 != null) Count++;
                }
                return Count;
            }
        }

        #endregion

        #region Conquest

        public int ConquestCountdown = 30;
        public int WinningTeam = -1;

        public bool runningCountdown = false;

        #endregion

        public bool Start() 
        {
            //Log.WriteInfo(">---Room-1396 bool Start()");
            //if (gameactive || status == 2 || users.Count < 2 && channel != 3 && !Configs.Server.Debug) return false; 

            foreach (User usr in users.Values)
            {
                if (!usr.AloneMode) // >--- +++ Empezar partida uno solo.
                {
                    if (gameactive || status == 2 || users.Count < 2 && channel != 3 ) 
                   
                    {
                        send(new Game.SP_Chat(usr, Game.SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> No puedes entrar!!", 999, Configs.Server.SystemName));
                        return false;
                    }

                    if (usr.isReady == false && usr.roomslot != master)
                    {
                        send(new Game.SP_Chat(usr, Game.SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> All Players must be ready for start the game!!", 999, Configs.Server.SystemName));
                        return false;
                    }
                    //else if (type == 1 && users.Count < 4 && !Configs.Server.Debug)
                    else if (type == 1 && users.Count < 4 )
                    {
                        send(new Game.SP_Chat(usr, Game.SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> Not enough Players to start a clanwar (Min: 4)!!", 999, Configs.Server.SystemName));
                        return false;
                    }
                    else if (type == 1 && rounds < 2)
                    {
                        string Need = ((mode == (int)RoomMode.Explosive || mode == (int)RoomMode.HeroMode) ? "3+ rounds (Hero Mode) / 5+ rounds (Explosive)" : "100+ kills");
                        //string Need = ((mode == (int)RoomMode.Explosive)  ? " 5+ rounds (Explosive)" : "100+ kills");
                        send(new Game.SP_Chat(usr, Game.SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> Need at least " + Need + " for start a clanwar!!", 999, Configs.Server.SystemName));
                        return false;
                    }
                    
                    //if (channel != 3 && users.Count <= 1 || channel == 3 && mode == 13 && users.Count <= 1 && !usr.AloneMode) 
                    if (channel != 3 && users.Count <= 1 || (channel == 3 && mode == 13 && users.Count <= 1))
                    {
                        usr.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Whisper, "SYSTEM >> Need more players!!", usr.sessionId, usr.nickname));
                        return false;
                    }
                }
                if (channel == 3 && mode == 13) // >--- +++ Escape
                {
                    Log.WriteInfo(">---Room-1438 Start() Si el slot es par, IsEscapeZombie ");
                    if (usr.roomslot % 2 == 0 && usr.roomslot != 0) usr.IsEscapeZombie = true;
                }
            }
            // Reset Room //
            Vehicles.Clear();
            MapData = Managers.MapDataManager.GetMapByID(mapid);
            SpawnLocation = 0;
            kills = highestkills = NIURounds = DerbRounds = KillsNIULeft = KillsDerbaranLeft = DerbExplosivePoints = NIUExplosivePoints = TotalWarDerb = TotalWarNIU = DerbaranPoints = NIUPoints = 0;
            bombPlanted = bombDefused = firstblood = isNewRound = EndGamefreeze = firstingame = firstspawn = sleep = false;
            HackPercentage.BaseA = HackPercentage.BaseB = 0;
            Mission1 = Mission2 = Mission3 = null;
            EscapeHuman = EscapeZombie = 0; //>--- +++
            SiegeWarC4User = null;
            SiegeWarTime = -1;
            timespent = 0;
            Placements.Clear();
            gameactive = cwcheck = true;

            #region Modes

            RoomMode tmode = (RoomMode)this.mode;

            this.explosive = null;
            this.ffa = null;
            this.deathmatch = null;
            this.totalwar = null;
            this.zombie = null;
            this.timeattack = null;
            this.capturemode = null;
            this.heromode = null;
            this.escape = null;
            //this.tankwar = null;
            //this.specialmode = null;
            
            //Log.WriteInfo(">---Room-1473 switch 1 (tmode): " + tmode );

            switch (tmode)
                        {
                            case RoomMode.Explosive:
                            case RoomMode.Annihilation:
                                {
                                    this.explosive = new Explosive(this);
                                    break;
                                }
                            case RoomMode.FFA:
                                {
                                    this.ffa = new FreeForAll(this);
                                    break;
                                }
                            case RoomMode.FourVersusFour:
                            case RoomMode.TDM:
                            case RoomMode.Conquest:
                            case RoomMode.BGExplosive:
                                {
                                    this.deathmatch = new DeathMatch(this);
                                    break;
                                }
                            case RoomMode.TotalWar:
                                {
                                    this.totalwar = new TotalWar(this);
                                    break;
                                }
                            case RoomMode.Survival: 
                            case RoomMode.Defense:
                                {

                                    this.zombie = new ZombieMode(this); 
                                    break;
                                }
                            case RoomMode.CaptureMode:
                                {
                                    this.capturemode = new CaptureMode(this);
                                    break;
                                 }
                            case RoomMode.TimeAttack:
                                {
                                    this.timeattack = new TimeAttack(this);
                                    break;
                                }
                            case RoomMode.Escape:
                                {
                                    this.escape = new Escape(this);
                                    break;
                                }
                            case RoomMode.HeroMode:
                                {
                                    this.heromode = new HeroMode(this);
                                    break;
                                }
            }
            #endregion
            //Log.WriteInfo(">---Room-1530 mode: " + mode);
            if (mode == (int)RoomMode.Explosive || mode == (int)RoomMode.HeroMode)
            {
                switch (rounds)
                {
                    case 0: explosiveRounds = 1; break;
                    case 1: explosiveRounds = 3; break;
                    case 2: explosiveRounds = 5; break;
                    case 3: explosiveRounds = 7; break;
                    case 4: explosiveRounds = 9; break;
                }
            }
            else if (mode == 1)
            {
                ffakillpoints = 10 + (5 * rounds);
            }
            else if (mode == 2 || mode == 3) //>--- 2=Deathmach 3=TDM
            {
                switch (rounds)
                {
                    case 0: kills = KillsDerbaranLeft = KillsNIULeft = 30; break;
                    case 1: kills = KillsDerbaranLeft = KillsNIULeft = 50; break;
                    case -1:
                    case 2: kills = KillsDerbaranLeft = KillsNIULeft = 100; break;
                    case 3: kills = KillsDerbaranLeft = KillsNIULeft = 150; break;
                    case 4: kills = KillsDerbaranLeft = KillsNIULeft = 200; break;
                    case 5: kills = KillsDerbaranLeft = KillsNIULeft = 300; break;
                    case 6: kills = KillsDerbaranLeft = KillsNIULeft = 500; break;
                    case 7: kills = KillsDerbaranLeft = KillsNIULeft = 999; break;
                }
            }
            else if (mode == 4 || mode == 5 || mode == 9) //>--- 4=Conquest	5=BGExplosive - 9=Capture. +++
            {
                switch (rounds)
                {
                    case -1:
                    case 0: kills = KillsDerbaranLeft = KillsNIULeft = 100; break;
                    case 1: kills = KillsDerbaranLeft = KillsNIULeft = 200; break;
                    case 2: kills = KillsDerbaranLeft = KillsNIULeft = 300; break;
                    case 3: kills = KillsDerbaranLeft = KillsNIULeft = 500; break;
                    case 4: kills = KillsDerbaranLeft = KillsNIULeft = 999; break;
                }
            }
            else if (mode == 8 ) //>--- 8 = TotalWar  
            {
                switch (rounds)
                {
                    case -1:
                    case 0: kills = 100; break;
                    case 1: kills = 200; break;
                    case 2: kills = 300; break;
                    case 3: kills = 500; break;
                    case 4: kills = 999; break;
                }
            }

            if (mode != (int)RoomMode.Explosive && mode != (int)RoomMode.HeroMode && mode != (int)RoomMode.Annihilation)
            {
                switch (timelimit)
                {
                    case 1:
                        timeleft = 599000;
                        break;
                    case 2:
                        timeleft = 1199000;
                        break;
                    case 3:
                        timeleft = 1799000;
                        break;
                    case 4:
                        timeleft = 2399000;
                        break;
                    case 5:
                        timeleft = 2399000;
                        break;
                    case 6:
                        timeleft = -1000;
                        break;
                }
            }
            else
            {
                if (mode != (int)RoomMode.Annihilation)
                {
                    timeleft = mode == (int)RoomMode.Explosive ? 180000 : 300000; // Round Timer
                }
                else { timeleft = 90000; }
                Log.WriteInfo(">---Room-1617 mode: " + mode);
            }

            if (mapid == 42 || mapid == 60) //>--- SiegeWar 1 y 2
            {
                if (mapid == 60) 
                {
                    KillsDerbaranLeft = KillsNIULeft = 400;
                    timeleft = 600000; // Round Timer
                }
                else
                {
                    KillsDerbaranLeft = KillsNIULeft = 300;
                    timeleft = 1800000; // Round Timer
                }
            }
            else if (timeattack != null) // >---Time attack---
            {
                timeleft = 1200000; //>--- Duracion total partida  20 minutos + 60000 = 1200000
            }

            for (int I = 0; I < 32; I++)
            {
                flags[I] = (int)Side.Neutral;
            }

            if (MapData != null)
            {
                Log.WriteInfo(">---Room-1645 SET flags & vehicles ");
                flags[MapData.derb] = (int)Side.Derbaran;
                flags[MapData.niu] = (int)Side.NIU;
                SpawnVehicles();
            }

            //if (this.mode == (int)RoomMode.FFA && ffa != null && ffa.isGunGame)
            //{
            //    ffa.InitializeGunGame();
            //}

            foreach (User usr in users.Values)
            {
                ResetUserStats(usr);
                usr.ExplosiveAlive = true;
                usr.Alive = true; //>--- +++ 
                usr.playing = true;
            }

            //Log.WriteInfo(">---Room-1664 RoomInitializeUsers-30017-PKT ");
            send(new SP_RoomInitializeUsers(this));
            firstInGameTS = Generic.timestamp + 15;

            return true;
        }

        #region Clan

        public int GetClanSide(User usr)
        {
            foreach (User tempUsr in users.Values)
            {
                if (tempUsr.clanId == usr.clanId)
                {
                    return (GetSide(tempUsr));
                }
            }
            return -1;
        }

        public bool JoinClanWar(User usr)
        {
            if (usr.clan == null) return false;

            int clanrank = usr.clan.clanRank(usr);

            if (clanrank == -1 || clanrank == 9) return false;

            int ClanSide = GetClanSide(usr);

            if (ClanSide == -1) return false;

            if (GetSideCount(ClanSide) >= (maxusers / 2)) return false;

            if (users.Count <= 0)
            {
                usr.room = this;
                usr.roomslot = 0;
                users.TryAdd(0, usr);
                master = 0;
                return true;
            }
            else
            {
                if (users.Count < maxusers)
                {
                    usr.roomslot = FreeRoomSlotBySide(ClanSide);
                    if (usr.roomslot != -1)
                    {
                        usr.room = this;
                        users.TryAdd(usr.roomslot, usr);
                        usr.send(new SP_JoinRoom(usr, this));
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        public bool JoinUser(User usr, int side = 2)
        {
            var roomMaster = UserManager.ServerUsers.Values.Where(r => r.room.id == id && r.roomslot == master);

            if (roomMaster == null) remove();

            ResetUserStats(usr);

            if (voteKick.lockuser.IsLockedUser(usr))
            {
                usr.send(new Game.SP_Chat("GM", Game.SP_Chat.ChatType.Whisper, "GM >> You have been kicked from the room, you must wait 5 minutes!", 999, "GM"));
                return true;
            }

            if (type == 1)
            {
                if (JoinClanWar(usr))
                {
                    return true;
                }
            }

            if (users.Count <= 0)
            {
                usr.room = this;
                usr.roomslot = 0;
                users.TryAdd(0, usr);
                master = usr.roomslot;
                return true;
            }
            else
            {
                if (users.Count < maxusers) // >--- Join Room-
                {
                    usr.Health = -1;
                    if (usr.channel != 3)
                    {
                        if (side == 0 || side == 1)
                        {
                            int EnemySide = side == 0 ? 1 : 0;
                            if (GetSideCount(side) <= GetSideCount(EnemySide))
                            {
                                usr.roomslot = FreeRoomSlotBySide(side);
                                if (usr.roomslot != -1)
                                {
                                    if (gameactive) { usr.playing = true; }
                                    usr.room = this;
                                    users.TryAdd(usr.roomslot, usr);
                                    usr.send(new SP_JoinRoom(usr, this));
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            int randomSide = (GetSideCount(1) >= GetSideCount(0) ? 0 : 1);

                            int rs = FreeRoomSlotBySide(randomSide);
                            if (rs != -1)
                            {
                                if (gameactive) { usr.playing = true; }
                                usr.roomslot = rs;
                                usr.room = this;
                                users.TryAdd(rs, usr);
                                usr.send(new SP_JoinRoom(usr, this));
                                return true;
                            }
                        }
                    }
                    else
                    {
                        for (int I = 0; I < 4; I++) //>---Room- Join Room channel 3
                        {
                            if (users.ContainsKey(I) == false)
                            {
                                if (gameactive)
                                {
                                    usr.playing = true;
                                }
                                usr.roomslot = I;
                                usr.room = this;
                                users.TryAdd(I, usr);

                                usr.send(new Game.SP_JoinRoom(usr, this));
                                break;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public User GetUser(int SlotID)
        {
            if (users.ContainsKey(SlotID))
            {
                return users[SlotID];
            }
            return null;
        }

        #region Placement

        public ConcurrentDictionary<int, Placement> Placements = new ConcurrentDictionary<int, Placement>();

        public int AddPlacement(User usr, string itemcode)
        {
            //Log.WriteInfo(">---Room-1835 Placement itemcode: " + itemcode);
            int id = Placements.Count + 1;
            Placement placement = new Placement(id, usr, itemcode);
            Placements.TryAdd(id, placement);
            return id;
        }

        public void RemovePlacement(int placementId)
        {
            if (Placements.ContainsKey(placementId))
            {
                Placement p;
                Placements.TryRemove(placementId, out p);
            }
        }

        public Placement getPlacement(int placementId)
        {
            if (Placements.ContainsKey(placementId))
            {
                return Placements[placementId];
            }
            return null;
        }

        public User getPlacementOwner(int placementId)
        {
            if (Placements.ContainsKey(placementId))
            {
                return Placements[placementId].Planter;
            }
            return null;
        }

        #endregion

        public int Lasttick = 0;

        public void updateTime()
        {
            try
            {
                if (firstingame)
                {
                    int type = 0;
                    
                      if (mode == 9) //>--- Capture añadido...
                    {
                        switch (WinningTeam)
                        {
                            case 0: DerbaranPoints += 5; break;
                            case 1: NIUPoints += 5; break;
                        }
                    }
                    
                    if (mode == 4 || mode == 8 )// 4=Conquest - 8=TotalWar
                    {
                        WinningTeam = -1;

                        if (mode == 4)
                        {
                            int DerbaranFlags = flags.Where(f => f == 0).Count();

                            int NIUFlags = flags.Where(f => f == 1).Count();
                            if (DerbaranFlags > NIUFlags)
                                WinningTeam = 0;
                            else if (NIUFlags > DerbaranFlags)
                                WinningTeam = 1;
                        }
                        else if (mode == 8) //>--- 8=TotalWar
                        {
                            WinningTeam = flags[8];
                        }

                        if (SiegeWarTime >= 0)
                        {
                            SiegeWarTime--;
                            if (SiegeWarTime <= 0)
                            {
                                SiegeWar2Explosion();
                                SiegeWarTime = -1;
                            }
                        }

                        if (WinningTeam != -1)
                        {
                            type = 1;
                            if (!runningCountdown)
                            {
                                runningCountdown = true;
                                ConquestCountdown = (mode == 4 ? 30 : 20);
                            }
                        }

                        if (ConquestCountdown > 0)
                        {
                            ConquestCountdown--;
                            if (ConquestCountdown <= 0)
                            {
                                runningCountdown = false;
                                if (mode == 4) //>--- Conquest
                                {
                                    switch (WinningTeam)
                                    {
                                        case 0: KillsNIULeft -= 10; break;
                                        case 1: KillsDerbaranLeft -= 10; break;
                                    }
                                }
                                else if (mode == 8) //>--- 8 = TotalWar
                                {
                                    switch (WinningTeam)
                                    {
                                        case 0: TotalWarDerb += 5; break;
                                        case 1: TotalWarNIU += 5; break;
                                    }
                                }

                                type = 2;
                            }
                        }
                    }

                    if (channel == 3) //>--- Modo Zombie
                    {
                        
                        if (zombieRunning && !sleep)
                        {
                            timespent += 1000;
                            if (timeleft > 0 && firstingame) timeleft -= 1000;
                        }
                    }
                    else
                    {
                        if (timeleft > 0 && !sleep && firstingame) timeleft -= 1000;
                        timespent += 1000;
                    }

                    if (sleep) return;
                    send(new SP_RoomThread(this, type));
                }
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.Message + " " + ex.StackTrace);
            }
        }

        public void DestroyVehicle(Vehicle Vehicle)
        {
            if (Vehicle != null)
            {
                //Log.WriteInfo(">---Room-1986 RoomVehicleExplode-PKT ");
                send(new SP_RoomVehicleExplode(id, Vehicle.ID, -1));
                Vehicle.Health = 0;
                Vehicle.ChangedCode = string.Empty;
                Vehicle.TimeWithoutOwner = 0;
            }
        }



        public int SideCountDerb { get { return users.Values.Where(u => u != null && GetSide(u) == (int)Side.Derbaran).Count(); } }
        public int SideCountNIU { get { return users.Values.Where(u => u != null && GetSide(u) == (int)Side.NIU).Count(); } }

        public int AliveDerb { get { return users.Values.Where(u => u.IsAlive() && GetSide(u) == (int)Side.Derbaran && u != null).Count(); } }
        public int AliveNIU { get { return users.Values.Where(u => u.IsAlive() && GetSide(u) == (int)Side.NIU && u != null).Count(); } }

        public Explosive explosive = null;
        public FreeForAll ffa = null;
        public DeathMatch deathmatch = null;
        public TotalWar totalwar = null;
        public ZombieMode zombie = null;
        public TimeAttack timeattack = null;
        public Escape escape = null;
        public CaptureMode capturemode = null;
        public HeroMode heromode = null;
        //public TankWar tankwar = null;
        //public SpecialMode specialmode = null;

        public bool disposed = false;

        public void update()
        {
            while (!disposed)
            {
                try
                {
                    if (!EndGamefreeze && gameactive)
                    {
                        if (Lasttick != DateTime.Now.Second)
                        {
                            Lasttick = DateTime.Now.Second;

                            updateTime();  //>--- Actualiza la cuenta atras
                            
                            users.Values.Where(usr => usr.spawnprotection > 0).ToList().ForEach(u => { u.spawnprotection--; });

                            #region Vehicle
                            
                            foreach (Vehicle vehicle in Vehicles.Values)
                            {
                                if (vehicle.SpawnProtection > 0)
                                {
                                    vehicle.SpawnProtection--;
                                }

                                if (vehicle.Players.Count == 0 && vehicle.ChangedCode != string.Empty)
                                {
                                    vehicle.TimeWithoutOwner++;
                                }

                                if (vehicle.ChangedCode != string.Empty && vehicle.TimeWithoutOwner >= 120)
                                {
                                    DestroyVehicle(vehicle);
                                }

                                if (vehicle.Health <= 0 && vehicle.RespawnTime != -1)
                                {
                                    vehicle.RespawnTick++;

                                    int respawnTime = vehicle.RespawnTime;

                                    if (mapid == 71 || mapid == 72) //>--- Crater_Dogfight y Pargona_Dogfight
                                        respawnTime = 20;

                                    if (vehicle.RespawnTick >= respawnTime) //>--- Respawn Vehicle
                                    {
                                        RespawnVehicle(vehicle.ID);
                                    }
                                }
                            }

                            #endregion

                        }
                        foreach (User usr in users.Values) //>--- Pongo esto para que no de error en usr.AloneMode
                        {

                            if (this.mode != 1 && channel != 3 && !usr.AloneMode) //>--- 

                            {
                                if (SideCountDerb == 0 || SideCountNIU == 0) //>--- Si uno de los dos equipos se queda sin jugadores...
                                {
                                    Log.WriteInfo(">---Room-2078 Sin players en un lado -EndGame ");
                                    EndGame();
                                    return;
                                }
                            }

                            if (voteKick.running)
                            {
                                bool kick = voteKick.GetPositiveVotes().Count >= (GetSideCount(voteKick.voteSide) / 2 + 1);
                                if (Generic.timestamp >= voteKick.timestamp)
                                {
                                    voteKick.StopVote(kick);
                                }
                                else if (kick)
                                {
                                    voteKick.StopVote(true);
                                }
                            }

                            foreach (User u in users.Values)
                            {
                                if (u.Health < 300 && u.Health > 0 && u.ExplosiveAlive && u.isSpawned) //>--- Empieza a desangrarse
                                {
                                    u.HPLossTick++;
                                    if (u.HPLossTick >= 10)
                                    {
                                        u.send(new SP_RoomInitializeUsers(this));
                                        if (!u.HasItem("CH01")) //>--- Si no llevas Compressing Bandage
                                        {
                                            u.Health -= (channel == 2 ? 25 : 5);
                                            if (u.Health <= 0)
                                            {
                                                u.OnDie();
                                                send(new SP_EntitySuicide(u.roomslot, SP_EntitySuicide.SuicideType.KilledByNotHavinHealTreatment));
                                            }
                                        }
                                        u.HPLossTick = 0;
                                    }
                                }

                                /*if (u.lastP2SUpdate + 120 < Generic.timestamp && u.ExplosiveAlive && u.isSpawned && u.Health > 0)
                                {
                                    Log.WriteError("No received game data for " + u.nickname + " for more than 120 sec");
                                    u.disconnect();
                                }*/
                            }
                            #region Mode

                            RoomMode mode = (RoomMode)this.mode;
                            lock (this)
                            {
                                // Log.WriteInfo(">---Room-1977 switch 2 - Mode: " + mode);
                                switch (mode)
                                {

                                    case RoomMode.Explosive:
                                    case RoomMode.Annihilation:
                                        {
                                            explosive.Update();
                                            break;
                                        }
                                    case RoomMode.HeroMode:
                                        {
                                            heromode.Update();
                                            break;
                                        }
                                    case RoomMode.FFA:
                                        {
                                            ffa.Update();
                                            break;
                                        }
                                    case RoomMode.FourVersusFour:
                                    case RoomMode.TDM:
                                    case RoomMode.Conquest:
                                    case RoomMode.BGExplosive:
                                        {
                                            deathmatch.Update();
                                            break;
                                        }
                                    case RoomMode.TotalWar:
                                        {
                                            totalwar.Update();
                                            break;
                                        }
                                    case RoomMode.CaptureMode: //>--- +++ 
                                        {
                                            capturemode.Update();
                                            break;
                                        }

                                    case RoomMode.Survival:
                                    case RoomMode.Defense:
                                        {
                                            //if (AliveDerb == 0) { EndGame(); return; } //>--- Comentado para posibilitar el spawn en los modos
                                            zombie.Update();
                                            break;
                                        }
                                    case RoomMode.TimeAttack:
                                        {
                                            
                                            timeattack.Update();
                                            break;
                                        }
                                    case RoomMode.Escape:
                                        {
                                            //if (AliveEscape(false) == 0 && EscapeHuman == 0) EndGame(); //>--- Esto dara error casi seguro, cambiado a Update
                                            //Log.WriteInfo(">---Room-2184 Llamada a Escape Update desde Switch mode");
                                            escape.Update();
                                            break;
                                        }
                                }
                            }
                            #endregion
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteError("Error at update:" + ex.Message + " " + ex.StackTrace);
                }

                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
            remove(); //>-- Todo el mundo fuera ...
        }

        public bool isPremMap(int Map) //>--- Mapas solo premium
        {
            switch (Map)
            {
                //case 36: return true;
                //case 38: return true;
                default: return false;
            }
        }
        public void remove()
        {
            if (users.Count > 0)
            {
                if (gameactive)
                {
                    Log.WriteInfo(">---Room-2220 - users.Count > 0: EndGame ");
                    EndGame();
                }

                foreach (User usr in users.Values)
                {
                    usr.send(new Game.SP_RoomKick(usr.roomslot));
                }
            }

            byte[] buffer = (new Game.SP_RoomListUpdate(this, 2)).GetBytes();

            foreach (User usr in UserManager.GetUsersInChannel(channel, false))
            {
                if (usr.lobbypage == Math.Floor((decimal)(id / 15)))
                {
                    usr.sendBuffer(buffer);
                }
            }
            ch.RemoveRoom(id);
        }

        public Clan GetClan(int Side)
        {
            var usr = users.Values.Where(r => GetSide(r) == Side && r.clan != null).FirstOrDefault();
            if (usr is User)
            {
                return usr.clan;
            }
            return null;
        }

        public bool isMyClan(User usr)
        {
            if (users.Values.Where(u => u.clanId == usr.clanId).Count() > 0)
            {
                return true;
            }
            return false;
        }

        public bool isGameEnding //>--- El juego se esta acabando...
        {
            get
            {
                if (gameactive && (mode == 2 || mode == 3 || mode == 4 || mode == 8 || mode == 5) && (KillsNIULeft <= 30 || KillsDerbaranLeft <= 30) || (channel == 1 && (mode == (int)RoomMode.Explosive || mode == (int)RoomMode.HeroMode) && (DerbRounds >= explosiveRounds - 2 || NIURounds >= explosiveRounds - 2)))
                {
                    // Less then 30 Kills Left
                    return true;
                }

                if (gameactive && mode == 1 && ffakillpoints <= 10)
                {
                    // Less then 10 Kills Left - FFA
                    return true;
                }

                return false;
            }
        }

        public bool isJoinable
        {
            get
            {
                if (this != null)
                {
                    if (channel == 3 && gameactive || type == 1 && gameactive) //>--- If Zombies or ClanWar, no join
                    {
                        Log.WriteLine("> -- Room-2289 - If Zombies or ClanWar, no join ");
                        return false;
                    }

                    if (users.Count >= maxusers || userlimit && !gameactive || EndGamefreeze) // Room is full, no join
                    {
                        Log.WriteLine("> -- Room-2295 - Room is full, no join");
                        return false;
                    }

                    if (isGameEnding)
                    {
                        Log.WriteLine("> -- Room-2301 - isGameEnding, no join");
                        return false;
                    }
                }

                return true;
            }
        }

        #region Spectate

        public bool AddSpectator(User usr)
        {
            if (spectators.Count >= Configs.Server.MaxSpectators) return false;
            if (!spectators.ContainsKey(usr.userId))
            {
                int id = spectators.Count;
                usr.spectating = true;
                usr.room = this;
                usr.roomslot = 32 + id; // Spectator ID
                usr.spectatorId = id;
                spectators.TryAdd(usr.userId, usr);
                return true;
            }
            return false;
        }

        public void RemoveSpectator(User usr)
        {
            if (spectators.ContainsKey(usr.userId))
            {
                send(new SP_PlayerInfoSpectate(usr, this));
                usr.send(new SP_Spectate());
                usr.lobbypage = 0;
                usr.send(new SP_RoomList(usr, 0, false));
                usr.room = null;
                usr.roomslot = 0;
                usr.spectating = false;
                User u = null;
                spectators.TryRemove(usr.userId, out u);
            }
        }

        #endregion


        #region Disposable
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            disposed = true;
        }

        #endregion

        public void InitializeUDP(User usr)
        {
            try
            {
                List<User> tempList = new List<User>();
                Log.WriteLine("> -- Room-2369 InitializeUDP(User usr): " + usr);
                tempList.AddRange(users.Values);
                tempList.AddRange(spectators.Values);

                if (tempList.Contains(usr))
                {
                    tempList.Remove(usr);
                }

                List<User> userToList = new List<User>();
                userToList.Add(usr);

                byte[] data = (new SP_PlayerInfo(userToList)).GetBytes();

                tempList.ForEach(u => { u.sendBuffer(data); });

                usr.send(new SP_PlayerInfo(tempList));
            }
            catch (Exception ex)
            {
                Log.WriteDebug("Error at Initialize UDP:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void InitializeSpectatorUDP(User usr)
        {
            List<User> players = new List<User>(users.Values.Where(r => r.IsConnectionAlive && r != null));

            usr.send(new SP_PlayerInfo(players));
            send(new SP_PlayerInfoSpectate(usr));
        }

        /// <summary>
        /// Lock the game to users
        /// </summary>
        internal class LockUser
        {

            /// <summary>
            /// Any locked user cannot join into the room
            /// </summary>
            internal class LockedUser
            {
                public User usr;
                public int timestamp;
            }
            public List<LockedUser> LockedUsers = new List<LockedUser>();

            public void Lock(User usr)
            {
                var u = LockedUsers.Where(r => r.usr.userId == usr.userId).FirstOrDefault();
                if (u == null)
                {
                    LockedUser lockusr = new LockedUser();
                    lockusr.timestamp = Generic.timestamp + 300;
                    lockusr.usr = usr;
                    LockedUsers.Add(lockusr);
                }
            }

            public bool IsLockedUser(User usr)
            {
                var u = LockedUsers.Where(r => r.usr.userId == usr.userId).FirstOrDefault();
                if (u != null)
                {
                    if (u.timestamp <= Generic.timestamp)
                    {
                        LockedUsers.Remove(u);
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Internal vote kick class for each room
        /// </summary>
        internal class VoteKick
        {
            /// <summary>
            /// Each vote for kick a user
            /// </summary>
            internal struct VoteKickVote
            {
                public User usr;
                public bool kick;
            }
            public List<VoteKickVote> votes = new List<VoteKickVote>();
            public int voteSide = -1;
            public int castedUser = -1;
            public bool running = false;
            public int timestamp = 0;
            public int lastKickTimestamp = 0;
            public Room r;
            public LockUser lockuser;

            public VoteKick(Room r)
            {
                this.r = r;
                lockuser = new LockUser();
            }

            public void StartVote(int tarGetUser, int side)
            {
                running = true;
                castedUser = tarGetUser;
                voteSide = side;
                timestamp = Generic.timestamp + 30;
            }

            public void StopVote(bool kicked)
            {
                KickUser(kicked);
                running = false;
                timestamp = 0;
                castedUser = -1;
                voteSide = -1;
                lastKickTimestamp = Generic.timestamp + 60;
                votes.Clear();
            }

            public void AddUserVotekick(User usr, bool kick)
            {
                VoteKickVote vote = new VoteKickVote();
                vote.usr = usr;
                vote.kick = kick;
                votes.Add(vote);
            }

            public List<VoteKickVote> GetPositiveVotes()
            {
                return votes.Where(r => r.kick).ToList();
            }

            public List<VoteKickVote> GetNegativeVotes()
            {
                return votes.Where(r => !r.kick).ToList();
            }

            internal void KickUser(bool kicked)
            {
                User u = this.r.GetPlayer(castedUser);

                byte[] buffer = (new SP_RoomData_VoteKick(castedUser, kicked, this.r.id)).GetBytes();
                foreach (User usr in r.users.Values)
                {
                    if (r.GetSide(usr) == voteSide)
                    {
                        usr.sendBuffer(buffer);
                    }
                }

                if (u != null)
                {
                    if (kicked)
                    {
                        r.voteKick.lockuser.Lock(u);
                        r.RemoveUser(this.castedUser);
                    }
                }
            }
        }

        internal User GetPlayer(int TargetSlot)
        {
            if (users.ContainsKey(TargetSlot))
            {
                return users[TargetSlot];
            }
            return null;
        }

        public int derbHeroUsr = -1, niuHeroUsr = -1;
        public int derbHeroKill = 0, niuHeroKill = 0;
    }
}
