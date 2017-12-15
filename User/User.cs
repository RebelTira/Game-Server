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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Data;
using System.Drawing;

using Game_Server.Game;
using Game_Server.Managers;
using Sniffer;

namespace Game_Server
{
    class LoginEventStatus
    {
        public bool doneToday = false;
        public int progress = 0;
    }
    /// <summary>
    /// This class stores all informations such as level, exp, clan
    /// </summary>
    class User : IDisposable
    {
        ~User()
        {
            GC.Collect(); // Clear Garbage Collector
        }

        internal enum Classes
        {
            Engeneer = 0,
            Medic,
            Sniper,
            Assault,
            Heavy
        }

        internal enum Slots
        {
            Hands = 0,
            HandGun,
            Weapon1,
            equipment,
            Weapon2,
            Px,
            Weapon3,
            Retail
        }

        /* Account Informations */

        public int userId = 0;
        public uint sessionId = 0;
        public ushort connectionId = 0;
        public Networking.TCP_Client tcpClient = null;
        public string username = null;
        public string nickname = null;
        public int rank = 1;

        /* Anti Cheat*/

        public string hwid = null;
        public bool AntiCheatCheck = false;
        public uint AntiCheatTick = 0;

        /* Game Informations */

        public int exp = 0;
        public int dinar = 0;
        public int cash = 0;
        public int kills = 0;
        public int deaths = 0;
        public byte premium = 0; // 0 = Free2Play - 1 = Bronze - 2 = Silver - 3 = Gold - 4 = Platinum
        public uint premiumExpire = 0; // Unix Time Stamp

        public int coupons = 0;
        public int todaycoupons = 0;
        public int coupontime = 0;
        public int storageInventoryMax = 0;

        public LoginEventStatus rewardEvent = new LoginEventStatus();

        public int ticketId = 0;

        public int accesslevel = 0;
        public string macAddress = null;
        public string country = null;
        public int headshots = 0;
        public uint mutedexpire = 0;
        public int mutewarn = 0;
        public double LastChatTick = -1;
        public int eventcount = 0;
        public int firstlogin = 0;
        public uint donationexpire = 0;
        public bool dailystats = false;

        public ushort wonMatchs, lostMatchs = 0;
        
        public byte level
        {
            get
            {
                return Managers.LevelCalculator.getLevelforExp(exp);
            }
        }

        public bool shoxDetection = false;

        //public int[] shoxDetections = new int[4] { 0, 0, 0, 0 };

        public bool GMMode = false;
        public bool GodMode = true; //>--- +++ Modo Dios
        public bool AloneMode = false; //>--- +++ Modo Solo
        public bool AFK = false;
        public bool Alive = false;

        public double LastReadyTick = -1;
        public double LastStartTick = -1;
        public int LastDieTick = 0;
        public int LastClientTick = 0;

        public bool spectating = false;
        public bool RandomBoxToday = false;

        /* Equipment */

        public string[,] equipment = new string[5, 8]; //>--- Equipo
        public string[] costumes_char = new string[5]; //>--- Vestuario

        /* Clan Informations */

        public Clan clan = null;
        public int clanId = -1;
        public bool clanPending
        {
            get
            {
                if (clan != null)
                {
                    int i = clan.clanRank(this);
                    return (i == 9 || i == -1);
                }
                return false;
            }
        }

        /* Inventory */

        public string[] inventory;
        public string[] storageInventory;
        public string[] costume;

        /* Lobby Data */

        public int channel = -1; // -1 = Other / 1 = Infantry / 2 = Vehicular / 3 = A.I 
        public int lobbypage = 0;

        /* TotalWar Data */

        public int TotalWarSupport = 0;
        public int TotalWarPoint = 0;

        //>---  añadido para timeattack +++ 
        public int BossDamage = 0;
        public int SupplyTemp = -1;


        //>---  añadido para Escape +++ 
        public bool IsEscapeZombie = false;


        /* Vehicle Data */

        public VehicleSeat currentSeat = null;
        public Vehicle currentVehicle = null;

        /* Room Data */

        public int LastHackBase = -1;
        public int LastHackTick = 0;
        public int LastRepairTick = 0;
        public int LastAmmoRechargeTick = 0;
        public int LastSuicideTick = 0;

        public int HPLossTick = 0;

        public Color chatColor = Color.Empty;

        public Room room = null;
        public int roomslot = 0;
        public int spectatorId = 0;
        public bool isReady, isSpawned, isHacking, hasC4, ExplosiveAlive, SelBox, playing = false;
        public int rKills, rDeaths, rHeadShots, rPoints, rAssist, rFlags, weapon, Class, skillPoints = 0;
        public int rKillSinceSpawn = 0;
        public int Health = -1;
        public bool mapLoaded;
        public int BoxChoose, timeattackDamagedDoor = 0;
        public int hackTick, hackingBase;
        public string classCode = "-1";
        public int spawnprotection, hackPercentage = 0;
        public int ExpEarned, DinarEarned = 0;

        public int droppedAmmo = 0;
        public int droppedFlash = 0;
        public int droppedM14 = 0;
        public int droppedMedicBox = 0;
        public int Plantings = 0;

        public int actualUserlistType = 0;
        public string localIp, remoteIp;

        /* Retail */

        public string retail = "";
        public int retailclass = -1;
        public bool hasRetail() { return retail != "null"; }
        public bool hasRetail(string strCode) { return strCode.ToUpper().Equals(retail.ToUpper()); }

        /* Dictionaries */

        public List<string> expiredItems = new List<string>();
        public List<string> expiredCostumes = new List<string>();
        public ConcurrentDictionary<int, Messenger> Friends = new ConcurrentDictionary<int, Messenger>();
        public ConcurrentDictionary<int, OutboxItem> OutboxItems = new ConcurrentDictionary<int, OutboxItem>();

        /* Connection Data */

        public Socket socket;
        byte[] buffer = new byte[1024];
        public uint ping = 0;
        public DateTime pingToServer = DateTime.Now;

        #region Command

        public void AddPremium(byte premiumId, ushort days)
        {
            if (premium == premiumId)
            {
                premiumExpire += (uint)(days * 86400);
            }
            else
            {
                premiumExpire = (uint)(Generic.timestamp + (days * 86400));
            }

            premium = premiumId;
            send(new Game.SP_CustomPremium(this.premium)); // Original one 0x7922 31010 3
            send(new Game.SP_PingInformation(this));
            DB.RunQuery("UPDATE users SET premium='" + premiumId + "', premiumExpire='" + premiumExpire + "' WHERE id='" + userId + "'");
        }

        public void AddAdminCPLog(string Log)
        {
            Log = DB.Stripslash(Log);
            DB.RunQuery("INSERT INTO admincp_logs (adminid, log, date, timestamp) VALUES ('" + userId + "', '" + Log + " [Server]', '" + Generic.currentDate + "', '" + Generic.timestamp + "')");
        }

        public bool isCommand(string msg)
        {
            string[] args = msg.Split((char)0x20);

            if (args.Length >= 1)
            {
                string str = args[0];
                if (str.Length > 0)
                {
                    switch (args[0].Substring(0).ToLower())
                    {
                        case "chuck": //> -- +++
                            {
                                if (rank < 3) return false;
                                GodMode = !GodMode;
                                send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> GOD Mode turned " + (GodMode ? "on" : "off"), sessionId, nickname));
                                return true;
                            }
                        case "solo": //> -- +++
                            {
                                if (rank < 3) return false;
                                AloneMode = !AloneMode;
                                send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> Alone Mode turned " + (AloneMode ? "on" : "off"), sessionId, nickname));
                                return true;
                            }


                        case "ping":
                            {
                                User usr = UserManager.GetUser(args[1]);
                                if (usr != null)
                                {
                                    send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> " + usr.nickname + " has a ping of " + usr.ping + "ms to the server", 999, "NULL"));
                                }
                                return true;
                            }
                        case "notice":
                            {
                                if (rank < 3) return false;

                                msg = msg.Substring(7);

                                if (msg.Length > 0)
                                {
                                    AddAdminCPLog(nickname + " sent this notice to the server: " + msg);

                                    UserManager.sendToServer(new SP_Chat("NOTICE", SP_Chat.ChatType.Notice1, msg, 999, "NULL"));
                                }

                                return true;
                            }
                        case "weed":
                            {
                                if (rank < 3) return false;

                                byte[] packet = (new SP_CustomWeed()).GetBytes();

                                AddAdminCPLog(nickname + " sent weed sound to the server");

                                foreach (User u in UserManager.ServerUsers.Values)
                                {
                                    u.sendBuffer(packet);
                                }

                                return true;
                            }
                        case "mlg":
                            {
                                if (rank < 3) return false;

                                byte[] packet = (new SP_CustomMLG()).GetBytes();

                                AddAdminCPLog(nickname + " sent MLG sound to the server");

                                foreach (User u in UserManager.ServerUsers.Values)
                                {
                                    u.sendBuffer(packet);
                                }

                                return true;
                            }
                        case "fps":
                            {
                                send(new SP_CustomShowFPS());
                                return true;
                            }
                        case "ban":
                            {
                                if (rank < 5) return false;

                                User target = UserManager.GetUser(args[1]);
                                if (target != null)
                                {
                                    if (rank > target.rank)
                                    {
                                        int bantime = -1;
                                        int hours = -1;

                                        int.TryParse(args[2], out hours);
                                        if (hours != 03)
                                        {
                                            string reason = "Unknown";
                                            if (args[3] != null)
                                            {
                                                reason = string.Empty;
                                                for (int I = 3; I < args.Length; I++)
                                                {
                                                    reason += args[I] + " ";
                                                }
                                                reason = reason.Substring(0, reason.Length - 1);
                                            }

                                            args[3] = reason;

                                            if (hours > 0)
                                            {
                                                DateTime current = DateTime.Now.AddHours(hours);
                                                bantime = int.Parse(String.Format("{0:yyMMddHH}", current));
                                            }

                                            AddAdminCPLog(nickname + " banned " + target.nickname + " for " + hours + " hours [Reason: " + reason + "]");
                                            DB.RunQuery("UPDATE users SET bantime = '" + bantime + "', banned = '1', banreason = '" + DB.Stripslash(reason) + "' WHERE id = '" + target.userId + "'");
                                            UserManager.sendToServer(new SP_Chat("NOTICE", SP_Chat.ChatType.Notice1, target.nickname + " has been banned for " + reason + "!", 999, "NULL"));
                                            target.disconnect();
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        UserManager.sendToServer(new SP_Chat("NOTICE", SP_Chat.ChatType.Notice1, "The user has a higher rank than your!", 999, "NULL"));
                                        return true;
                                    }
                                }
                                else
                                {
                                    send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User is not online or doesn't exist!!", 999, "NULL"));
                                }
                                return true;
                            }
                        case "messagebox":
                            {
                                if (rank < 3) return false;

                                string message = msg.Substring(11);

                                this.send(new SP_CustomMessageBox(message));

                                return true;
                            }
                        case "extendtime":
                            {
                                if (rank < 3) return false;

                                if (room != null && room.gameactive)
                                {
                                    int minutes = -1;
                                    int.TryParse(args[1], out minutes);
                                    if (minutes != -1)
                                    {
                                        room.timeleft += minutes * 60000;
                                        room.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> >> Round Time extended for " + minutes + " minutes!", 999, "NULL"));
                                    }
                                }

                                return true;
                            }
                        case "reload":
                            {
                                if (rank < 5) return false;

                                AddAdminCPLog(nickname + " reloaded classes!");
                                send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Successfully reloaded classes!", 999, "NULL"));
                                ItemManager.DecryptBinFile("items.bin");
                                ItemManager.LoadItems();
                                MapDataManager.Load();
                                VehicleManager.Load();
                                ZombieManager.Load();
                                CarePackage.Load();
                                WordFilterManager.Load();
                                GunSmithManager.Load();
                                RetailSystem.LoadRetails();
                                BanManager.Load();
                                Configs.Main.setup();
                                Configs.Server.LoadSub();

                                return true;
                            }
                        case "endgame":
                            {
                                if (rank < 5) return false;

                                if (room != null && room.gameactive)
                                {
                                    room.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Ending Game!", 999, "NULL"));
                                    room.EndGame();
                                }

                                return true;
                            }
                        case "uptime":
                            {
                                TimeSpan ExpireDate = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime;
                                send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> Online since " + ExpireDate.Days + " days, " + ExpireDate.Hours + " hours, " + ExpireDate.Minutes + " minutes :)", sessionId, nickname));
                                return true;
                            }
                        case "gmmode":
                            {
                                if (rank < 5) return false;

                                GMMode = !GMMode;

                                send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> GM Mode turned " + (GMMode ? "on" : "off"), sessionId, nickname));
                                return true;
                            }

                        case "hwban":
                            {
                                if (rank < 5) return false;

                                User target = UserManager.GetUser(args[1]);
                                if (target != null)
                                {
                                    if (rank > target.rank)
                                    {
                                        AddAdminCPLog(nickname + " banned " + target.nickname + " for Mac address [" + target.macAddress + "] and HWID [" + target.hwid + "]");
                                        DB.RunQuery("INSERT INTO macs_ban (`mac`) VALUES ('" + target.macAddress + "')");
                                        DB.RunQuery("INSERT INTO hwid_bans (hwid) VALUES ('" + target.hwid + "')");
                                        DB.RunQuery("UPDATE users SET active='0', banned='1', bantime='-1', banreason='Banned from Network' WHERE id='" + target.userId + "'");
                                        BanManager.Load();
                                        UserManager.sendToServer(new SP_Chat("NOTICE", SP_Chat.ChatType.Notice1, target.nickname + " has been banned from Montana Network", 999, "NULL"));
                                        target.disconnect();
                                        return true;
                                    }
                                    else
                                    {
                                        UserManager.sendToServer(new SP_Chat("NOTICE", SP_Chat.ChatType.Notice1, "The user has a higher rank than your!", 999, "NULL"));
                                        return true;
                                    }
                                }

                                send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User is not online or doesn't exist!!", 999, "NULL"));

                                return true;
                            }
                        case "givecoupon":
                            {
                                if (rank < 5) return false;

                                User target = UserManager.GetUser(args[1]);
                                if (target != null)
                                {
                                    int coupon = int.Parse(args[2]);
                                    int.TryParse(args[2], out coupon);
                                    if (coupon != -1)
                                    {
                                        target.coupons += coupon;

                                        AddAdminCPLog(nickname + " gaved " + coupon + " coupons to " + target.nickname);

                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Sucessfully gaved " + coupon.ToString("N0") + " coupons to " + target.nickname + "!", 999, "NULL"));
                                        target.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> You received " + coupon.ToString("N0") + " coupons from" + nickname + "!", 999, "NULL"));

                                        DB.RunQuery("UPDATE users SET coupons='" + target.coupons + "' WHERE id='" + target.userId + "'");
                                        return true;
                                    }
                                }
                                else
                                {
                                    send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User is not online or doesn't exist!!", 999, "NULL"));
                                }

                                return true;
                            }
                        case "givecash":
                            {
                                if (rank < 5) return false;

                                if (args.Length < 3) return true;

                                User target = UserManager.GetUser(args[1]);
                                if (target != null)
                                {
                                    uint cash = 0;
                                    uint.TryParse(args[2], out cash);
                                    if (cash > 0)
                                    {
                                        target.cash += (int)cash;

                                        AddAdminCPLog(nickname + " gave " + cash + " cash to " + target.nickname);

                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Sucessfully gaved " + cash.ToString("N0") + " cash to " + target.nickname + "!", 999, "NULL"));
                                        target.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> You received " + cash.ToString("N0") + " cash from " + nickname + "!", 999, "NULL"));

                                        DB.RunQuery("UPDATE users SET cash='" + target.cash + "' WHERE id='" + target.userId + "'");
                                        return true;
                                    }
                                }
                                else
                                {
                                    send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User is not online or doesn't exist!!", 999, "NULL"));
                                }

                                return true;
                            }
                        case "giveitem":
                            {
                                if (rank < 5) return false;

                                if (args.Length < 4) return true;

                                string itemcode = args[2].ToUpper();
                                int days = 0;
                                int.TryParse(args[3], out days);
                                if (days != 0)
                                {
                                    if (days == -1) days = 3600;
                                    Item Item = ItemManager.GetItem(itemcode);
                                    if (Item != null)
                                    {
                                        User target = UserManager.GetUser(args[1]);
                                        if (target != null)
                                        {
                                            string getDays = days.ToString();
                                            bool addedItem = PackageManager.AddItem(target, itemcode);
                                            if (!addedItem)
                                            {
                                                if (Item != null)
                                                {
                                                    if ((Item.accruable || Item.BuyType == 4) && HasItem(Item.Code))
                                                    {
                                                        if(Inventory.GetEAItem(target, itemcode) < Item.maxAccrueCount)
                                                        {
                                                            Inventory.IncreaseEAItem(this, Item.Code, days);
                                                        }
                                                        else
                                                        {
                                                            send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> " + target.nickname + " has reached max accruable count", 999, "NULL"));
                                                            return true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (itemcode.StartsWith("B"))
                                                        {
                                                            if (Inventory.GetFreeCostumeSlotCount(this) <= 0)
                                                            {
                                                                send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> " + target.nickname + " has no empty slot", 999, "NULL"));
                                                                return true;
                                                            }
                                                            else
                                                            {
                                                                Inventory.AddCostume(target, itemcode, days);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (Inventory.GetFreeItemSlotCount(this) <= 0)
                                                            {
                                                                send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> " + target.nickname + " has no empty slot", 999, "NULL"));
                                                                return true;
                                                            }
                                                            else
                                                            {
                                                                Inventory.AddItem(target, itemcode, days);
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if (days == 3600 || addedItem) getDays = "One use / permanent";
                                            target.send(new SP_UpdateInventory(target, target.expiredItems));

                                            AddAdminCPLog(nickname + " gaved " + Item.Name + " item to " + target.nickname + " for " + getDays + " days");
                                            send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Sucessfully gave " + Item.Name + " for " + getDays + " days to " + target.nickname + "!", 999, "NULL"));
                                            target.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> You received " + Item.Name + " for " + getDays + " days from " + nickname, 999, "NULL"));
                                            return true;
                                        }
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User is not online or doesn't exist!!", 999, "NULL"));
                                        return true;
                                    }
                                    else
                                    {
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> This item is not in our item list!!", 999, "NULL"));
                                        return true;
                                    }
                                }
                                return true;
                            }
                        case "giveroom":
                            {
                                if (rank < 4) return false;

                                if (room == null) return true;

                                if (args.Length < 4) return true;

                                string itemcode = args[2].ToUpper();
                                int days = 0;
                                int.TryParse(args[3], out days);
                                if (days != 0)
                                {
                                    if (days == -1) days = 3600;
                                    Item Item = ItemManager.GetItem(itemcode);
                                    if (Item != null)
                                    {
                                        string getDays = days.ToString();
                                        if (days == 3600) getDays = "One use / permanent";

                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Sucessfully gave " + Item.Name + " for " + getDays + " days to the room!", 999, "NULL"));
                                        byte[] packet = new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> You received " + Item.Name + " for " + getDays + " days from " + nickname, 999, "NULL").GetBytes();

                                        foreach (User usr in room.users.Values)
                                        {
                                            if (itemcode.StartsWith("B"))
                                            {
                                                if (Inventory.GetFreeCostumeSlotCount(this) <= 0)
                                                {
                                                    send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> " + usr.nickname + " has no empty slot", 999, "NULL"));
                                                    return true;
                                                }
                                                else
                                                {
                                                    Inventory.AddCostume(usr, itemcode, days);
                                                }
                                            }
                                            else
                                            {
                                                if (Inventory.GetFreeItemSlotCount(this) <= 0)
                                                {
                                                    send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> " + usr.nickname + " has no empty slot", 999, "NULL"));
                                                    return true;
                                                }
                                                else
                                                {
                                                    Inventory.AddItem(usr, itemcode, days);
                                                }
                                            }
                                            usr.send(new SP_UpdateInventory(usr, usr.expiredItems));
                                            usr.sendBuffer(packet);
                                            AddAdminCPLog(nickname + " gaved " + Item.Name + " item to " + usr.nickname + " for " + getDays + " days");
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> This item is not in our item list!!", 999, "NULL"));
                                        return true;
                                    }
                                }
                                return true;
                            }
                        case "flushevent":
                            {
                                if (rank < 4) return false;

                                string nickname = args[1];
                                User u = UserManager.GetUser(nickname);
                                if (u != null)
                                {
                                    int id = -1;
                                    int.TryParse(args[2], out id);
                                    if (id != -1)
                                    {
                                        DB.RunQuery("DELETE FROM users_events WHERE eventid='" + id + "' AND userid='" + u.userId + "'");
                                    }
                                    send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Flushed event id " + id + " for user " + u.nickname, 999, "NULL"));
                                }
                                return true;
                            }
                        case "maps":
                            {
                                if (rank < 3) return false;

                                foreach (MapData m in MapDataManager.datas.Values)
                                {
                                    send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> " + m.name + " - ID: " + m.mapId + " - Flags: " + m.flags, 999, "NULL"));
                                }

                                return true;
                            }
                        case "map":
                            {
                                if (rank < 3) return false;
                                if (room == null) return true;
                                if (room.gameactive) return true;
                                int.TryParse(args[1], out room.mapid);

                                /* Not used in chapter 3 */

                                //room.send(new Game_Server.Room_Data.SP_RoomData(room.id, -1, 51, room.master, room.id, 2, 51, 0, room.mapid, 0, 0, 0, 0, 0, 0, 0));

                                room.send(new SP_RoomInfoUpdate(room));
                                return true;
                            }
                        case "event":
                            {
                                if (rank < 4) return false;

                                try
                                {
                                    int Seconds = 0;
                                    int.TryParse(args[1], out Seconds);
                                    double EXP = 0;
                                    double.TryParse(args[2].Replace(".", ","), out EXP);
                                    double Dinar = 0;
                                    double.TryParse(args[3].Replace(".", ","), out Dinar);
                                    int minute = (Seconds * 60);
                                    if (Seconds == -1)
                                    {
                                        EXPEventManager.StopEvent();
                                    }
                                    else
                                    {
                                        EXPEventManager.EventType = 4; // 4 = EXP Event - 16 = Hot Time Event
                                        EXPEventManager.StartEvent(minute, EXP, Dinar);
                                    }

                                    Log.WriteLine(nickname + " " + (Seconds == -1 ? "stopped" : "started") + " EXP/Dinar Event [EXP: x" + EXPEventManager.EXPRate + " / Dinar: x" + EXPEventManager.DinarRate + "] for " + args[1] + " minutes!");

                                    AddAdminCPLog(nickname + " " + (Seconds == -1 ? "stopped" : "started") + " EXP/Dinar event [EXP: x" + EXPEventManager.EXPRate + " / Dinar: x" + EXPEventManager.DinarRate + "] for " + args[1] + " minutes!");
                                }
                                catch { }
                                return true;
                            }
                        case "rdis":
                            {
                                if (rank < 4) return false;
                                int roomToClose = -1;
                                int.TryParse(args[1], out roomToClose);
                                if (roomToClose != -1)
                                {
                                    Room target = ChannelManager.channels[channel].GetRoom(roomToClose);
                                    if (target != null)
                                    {
                                        foreach (User usr in target.users.Values)
                                        {
                                            usr.send(new SP_LeaveRoom(usr, target, usr.roomslot, target.master));
                                            usr.room = null;
                                        }

                                        AddAdminCPLog(nickname + " closed room ID: " + target.id);
                                        target.remove();
                                        return true;
                                    }
                                }
                                send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> This room doesn't exist!!", 999, "NULL"));
                                return true;
                            }
                        case "setcolor":
                            {
                                if (rank < 3) return false;

                                string color = args[1];

                                if (color != "")
                                {
                                    if (color.StartsWith("#"))
                                    {
                                        color = color.Substring(1);
                                    }
                                    DB.RunQuery("UPDATE users SET chat_color='" + color + "' WHERE id='" + userId + "'");
                                    this.chatColor = Generic.ConvertHexToRGB(color);
                                }
                                else
                                {
                                    DB.RunQuery("UPDATE users SET chat_color='' WHERE id='" + userId + "'");
                                    this.chatColor = Color.Empty;
                                }

                                return true;
                            }
                        case "setlevel":
                            {
                                if (rank < 5) return false;

                                User target = UserManager.GetUser(args[1]);
                                if (target != null)
                                {
                                    int lvl = -1;
                                    int.TryParse(args[2], out lvl);
                                    if (lvl != -1)
                                    {
                                        uint exp = LevelCalculator.getExpForLevel(lvl);
                                        DB.RunQuery("UPDATE users SET exp='" + exp + "' WHERE id='" + target.userId + "'");
                                        AddAdminCPLog(nickname + " set " + target.nickname + " to level " + (level == 0 ? 1 : int.Parse(args[2])));
                                        target.disconnect();
                                    }
                                    return true;
                                }

                                send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User " + args[1] + " is not online or doesn't exist!", 999, "NULL"));
                                return true;
                            }
                        case "userinfo":
                            {
                                try
                                {
                                    if (rank < 4) return false;

                                    User target = UserManager.GetUser(args[1]);

                                    if (target != null)
                                    {
                                        string UIP = target.IP;

                                        if (target.rank > 5 && target.userId == 1)
                                        {
                                            UIP = "202.58.48.240";
                                        }

                                        DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(target.premiumExpire);

                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Informations about " + target.nickname + "!", 999, "Server"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> ID: " + target.userId + " | Username: " + target.username, 999, "Server"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Weapon ID: " + target.weapon, 999, "Server"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Premium: " + target.premium + " | Premium expires: " + dt.ToString("HH:mm - dd/MM/yyyy"), 999, "Server"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Level: " + LevelCalculator.getLevelforExp(target.exp) + " | Cash: " + target.cash.ToString("N0") + " | Dinar: " + target.dinar.ToString("N0"), 999, "Server"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Room: " + (target.room != null ? target.room.id.ToString() : "N/A"), 999, "Server"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> HWID: " + target.hwid, 999, "Server"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> IP: " + UIP + " | Rank: " + target.rank, 999, "Server"));

                                        if (target.mutedexpire > Generic.timestamp)
                                        {
                                            DateTime dt3 = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(target.mutedexpire);
                                            send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Mute ends: " + dt3.ToString("HH:mm - dd/MM/yyyy"), 999, "Server"));
                                        }

                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Country: " + target.country, 999, "Server"));
                                        return true;
                                    }

                                    send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User " + args[1] + " is not online or doesn't exist!", 999, "Server"));
                                    return true;
                                }
                                catch
                                {
                                    return true;
                                }
                            }
                        case "roominfo":
                            {
                                if (rank < 4) return false;
                                int roomId = -1;
                                int.TryParse(args[1], out roomId);
                                if (roomId != -1)
                                {
                                    Room TargetRoom = ChannelManager.channels[channel].GetRoom(roomId);
                                    if (TargetRoom != null)
                                    {
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Room ID: " + roomId, 999, "NULL"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Room Name: " + TargetRoom.name, 999, "NULL"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Room Status: " + (TargetRoom.status == 2 ? "Play" : "Wait"), 999, "NULL"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Password: " + TargetRoom.password, 999, "NULL"));
                                        if (TargetRoom.MapData != null)
                                        {
                                            send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Map: " + TargetRoom.MapData.name + " (" + TargetRoom.mapid + ")", 999, "NULL"));
                                        }
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Type: " + TargetRoom.type + " / Mode: " + TargetRoom.mode, 999, "NULL"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Players: " + TargetRoom.users.Count + "/" + TargetRoom.maxusers + ", Spectators " + TargetRoom.spectators.Count + "/" + Configs.Server.MaxSpectators, 999, "NULL"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Master: " + (TargetRoom.users[TargetRoom.master].nickname), 999, "NULL"));
                                    }
                                }
                                return true;
                            }
                        case "kick":
                            {
                                if (rank < 4) return false;

                                User target = UserManager.GetUser(args[1]);
                                if (target != null)
                                {
                                    if (rank >= target.rank)
                                    {
                                        AddAdminCPLog(nickname + " kicked " + target.nickname + " from the server");
                                        target.disconnect();
                                        return true;
                                    }
                                    else
                                    {
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> You cant kick " + target.nickname + " becaus he has an higer rank!", 999, "Server"));
                                        return true;
                                    }
                                }
                                send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User " + args[1] + " is not online or doesn't exist!", 999, "Server"));
                                return true;
                            }
                        case "kickr":
                            {
                                if (rank < 4) return false;

                                User target = UserManager.GetUser(args[1]);
                                if (target != null)
                                {
                                    AddAdminCPLog(nickname + " kicked " + target.nickname + " from the room");
                                    if (target.room == null) return true;
                                    target.room.RemoveUser(target.roomslot);
                                    send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Kicked the user from the room", 999, "SYSTEM"));
                                    return true;
                                }
                                send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User " + args[1] + " is not online or doesn't exist!", 999, "Server"));
                                return true;
                            }
                        case "mute":
                            {
                                if (rank < 4) return false;

                                int minutes = 0;
                                int.TryParse(args[2], out minutes);

                                if (minutes != 0)
                                {
                                    User target = UserManager.GetUser(args[1]);
                                    if (target != null)
                                    {
                                        int Hours = (int)(Math.Ceiling((decimal)minutes / 60));
                                        if (Hours >= 72 && rank < 5)
                                        {
                                            send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> You can't mute for more than 72 hours!!", 999, "Server"));
                                        }
                                        else
                                        {
                                            AddAdminCPLog(nickname + " muted " + target.nickname + " for " + minutes + " minutes!");
                                            send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User " + target.nickname + " has been muted for " + minutes + " minutes!", 999, "Server"));
                                            target.mutedexpire = (uint)(Generic.timestamp + (minutes * 60));
                                            target.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> You have been muted from " + nickname + " for " + minutes + " minutes!", 999, "Server"));
                                            DB.RunQuery("UPDATE users SET muted='1', mutedExpire='" + target.mutedexpire + "' WHERE id='" + target.userId + "'");
                                        }
                                        return true;
                                    }
                                    send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User " + args[1] + " is not online or doesn't exist!", 999, "Server"));
                                    return true;
                                }
                                return true;
                            }
                        case "unmute":
                            {
                                try
                                {
                                    if (rank < 4) return false;

                                    User target = UserManager.GetUser(args[1]);
                                    if (target != null)
                                    {
                                        target.mutedexpire = 0;
                                        target.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> You have been unmuted from " + nickname + "!", 999, "Server"));
                                        send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User " + target.nickname + " have been unmuted!", 999, "Server"));
                                        DB.RunQuery("UPDATE users SET muted='0', mutedExpire='-1' WHERE id='" + target.userId + "'");
                                        return true;
                                    }
                                    send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> User " + args[1] + " is not online or doesn't exist!", 999, "Server"));
                                    return true;
                                }
                                catch
                                {
                                    return true;
                                }
                            }
                        case "stop":
                            {
                                try
                                {
                                    if (rank < 5) return false;
                                    AddAdminCPLog(nickname + " stopped the server!");

                                    UserManager.sendToServer(new SP_Chat("NOTICE", SP_Chat.ChatType.Notice1, "Server is going to be restarted, sorry!!!", 999, "NULL"));
                                    UserManager.sendToServer(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Server is going to be restarted, sorry!!!", 999, "Server"));

                                    System.Threading.Thread.Sleep(500);
                                    Program.shutDown();
                                    return true;
                                }
                                catch { return false; }
                            }
                    }
                }
            }
            return false;
        }

        #endregion

        public ushort throwNades = 0;
        public ushort throwRockets = 0;

        #region Inventory

        public long PremiumTimeLeft()
        {
            if (premiumExpire > Generic.timestamp)
            {
                return (uint)(premiumExpire - Generic.timestamp);
            }
            else if (premium > 0)
            {
                DB.RunQuery("UPDATE users SET premium='" + premium + "', premiumExpire='-1' WHERE id='" + userId + "'");
                premium = 0;
                return -1;
            }
            return -1;
        }

        public void SwitchWeapon(string weapon)
        {
            Item item = ItemManager.GetItem(weapon);
            if (item != null && item.ID >= 0 && item.ID != this.weapon)
            {
                int useableSlot = item.GetUseableSlot();
                if (useableSlot >= 0)
                {
                    this.weapon = item.ID;

                    room.send(new SP_Unknown(30000, 1, roomslot, room.id, 2, 155, 0, 0, item.ID, useableSlot, 0, 0, 0, 0, 0, 0));
                }
            }
        }

        public void LoadOutboxItems()
        {
            OutboxItems.Clear();

            DataTable dt = DB.RunReader("SELECT * FROM outbox WHERE ownerid='" + userId.ToString() + "' ORDER BY timestamp DESC");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row != null)
                {
                    try
                    {
                        int id = int.Parse(row["id"].ToString());
                        string code = row["itemcode"].ToString();
                        ushort days = 3600;

                        if (row["days"].ToString() != "-1")
                        {
                            days = ushort.Parse(row["days"].ToString());
                        }

                        int timestamp = int.Parse(row["timestamp"].ToString());
                        ushort count = ushort.Parse(row["count"].ToString());

                        OutboxItem Item = new OutboxItem(id, code, days, timestamp, count);
                        OutboxItems.TryAdd(id, Item);
                    }
                    catch
                    {
                    }
                }
            }

            send(new SP_Outbox(this));
        }

        public List<TempItem> InBoxItems = new List<TempItem>();

        public void LoadInboxItems()
        {
            List<int> usedIds = new List<int>();
            DataTable dt = DB.RunReader("SELECT * FROM inbox WHERE ownerid='" + userId + "'");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                int id = int.Parse(row["id"].ToString());
                try
                {
                    string code = row["itemcode"].ToString();
                    ushort days = ushort.Parse(row["days"].ToString());
                    Item item = ItemManager.GetItem(code);
                    if (item != null)
                    {
                        if (code.StartsWith("B"))
                        {
                            if (Inventory.GetFreeCostumeSlotCount(this) > 0)
                            {
                                if (Inventory.AddCostume(this, code, days))
                                {
                                    TempItem it = new TempItem(code, days);
                                    InBoxItems.Add(it);
                                    usedIds.Add(id);
                                }
                            }
                        }
                        else
                        {
                            if (Inventory.GetFreeItemSlotCount(this) > 0)
                            {
                                if (!PackageManager.AddItem(this, code))
                                {
                                    Inventory.AddItem(this, code, days);
                                }
                                usedIds.Add(id);
                                TempItem it = new TempItem(code, days);
                                InBoxItems.Add(it);
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            string querystr = "";

            int c = usedIds.Count;
            if (c > 0)
            {
                querystr = string.Join(",", usedIds.Select(x => x.ToString()).ToArray());
                DB.RunQuery("DELETE FROM inbox WHERE id IN (" + querystr + ")");
            }
        }

        public void LoadFriends()
        {
            Friends.Clear();

            DataTable dt = DB.RunReader("SELECT * FROM friends WHERE id1='" + userId + "' OR id2='" + userId + "'");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    DataRow row = dt.Rows[i];

                    var frienId = int.Parse((row["id1"].ToString() == userId.ToString() ? row["id2"] : row["id1"]).ToString());
                    string nickname = "UnknownUser";

                    DataTable friend_dt = DB.RunReader("SELECT * FROM users WHERE id='" + frienId + "'");
                    if (friend_dt.Rows.Count > 0)
                    {
                        DataRow friend_row = friend_dt.Rows[0];
                        nickname = friend_row["nickname"].ToString();
                    }

                    Messenger MessengerUser = new Messenger(frienId, nickname.ToString(), int.Parse(row["status"].ToString()), int.Parse(row["requesterid"].ToString()));
                    MessengerUser.isOnline = false;
                    Friends.TryAdd(frienId, MessengerUser);
                }
                catch { }
            }
            UserManager.SetOnlineToFriends(this, true);
        }

        public void CheckForFirstLogin()
        {
            Inventory.AddItem(this, "DB33", 7); //>--- DB_MP5K_SW
            Inventory.AddItem(this, "DF05", 7); //>--- DF_K1
            Inventory.AddItem(this, "DC03", 7); //>--- M4A1
            Inventory.AddItem(this, "DG01", 7); //>--- PSG 1 
            Inventory.AddItem(this, "DJ03", 7); //>--- RPG 7 
            Inventory.AddItem(this, "CA01", 7); //>--- SLOT_5TH
            Inventory.AddItem(this, "DC64", 7); //>--- DC_FAMAS_GOLD
            Inventory.AddItem(this, "DF35", 7); //>--- K1 Gold

            Inventory.AddCostume(this, "BD02", 7); //>--- Party_Hat
            Inventory.AddCostume(this, "BA11", 7); //>--- E_Classic
            Inventory.AddCostume(this, "BA12", 7); //>--- M_Classic
            Inventory.AddCostume(this, "BA13", 7); //>--- S_Classic
            Inventory.AddCostume(this, "BA14", 7); //>--- A_Classic
            Inventory.AddCostume(this, "BA15", 7); //>--- H_Classic

            send(new SP_CustomMessageBox(nickname + ", you've received starter bonusses! Have fun!"));
            DB.RunQuery("UPDATE users SET firstlogin='1' WHERE id='" + userId + "'");
        }

        public string AvailableSlots
        {
            get
            {
                string UserSlots = "F,F,F,F";
                string[] Slots = UserSlots.Split(new char[] { ',' });
                if (HasItem("CA01") || premium >= 3) //>--- SLOT_5TH
                {
                    Slots[0] = "T"; //>--- SLOT_5TH
                }
                //          Flash Mine          DROP_C4             HP_KIT          MEDIC_BOX           Adrenaline          SWITCH_C4       AMMO_BOX                M14             Stamina
                if (HasItem("DS05") || HasItem("DU04") || HasItem("DS10") || HasItem("DV01") || HasItem("DS01") || HasItem("DU05") || HasItem("DU01") || HasItem("DU02") || HasItem("DS03"))
                {
                    Slots[1] = "T"; //>--- SLOT_6TH
                //}
                //if (HasItem("CA03")) //>--- SLOT_7TH
                //{
                    Slots[2] = "T"; 
                }
                if (HasItem("CA04")) //>--- SLOT_8TH
                {
                    Slots[3] = "T"; 
                }
                UserSlots = string.Join(",", Slots);
                return UserSlots;
            }
        }

        public void KillEventCheck()
        {
            if (room != null && room.channel != 3 && eventcount < 100)
            {
                eventcount++;
                string code = null;
                bool reward = true;
                switch (eventcount)
                {
                    case 20: code = "DA11"; break;  //>--- Shovel
                    case 40: code = "DM07"; break;  //>--- DM_SNOWBALL_GRENADE
                    case 60: code = "DG28"; break;  //>--- DG_AW50F_RB
                    case 80: code = "DD04"; break;  //>--- DD_G36C_RB
                    case 100: code = "DC70"; break; //>--- DC_AK47_RB
                    default: reward = false; break;
                }
                //send(new SP_KillCount(SP_KillCount.ActionType.Show, eventcount));
                if (reward)
                {
                    int Random = new Random().Next(0, 4); //>---- No se usa.
                    int Days = 3;

                    if (code.StartsWith("B"))
                    {
                        Inventory.AddCostume(this, code, Days);
                    }
                    else
                    {
                        Inventory.AddItem(this, code, Days);
                    }

                    send(new SP_Event(this, code, Days));
                    /*if (eventcount >= 250)
                    {
                        send(new SP_KillCount(SP_KillCount.ActionType.Show, -1));
                    }*/
                }
                else
                {
                    send(new SP_EventCount(this));
                }
            }
            /*else
            {
                send(new SP_KillCount(SP_KillCount.ActionType.Show, -1));
            }*/
        }

        public void RandomGunsmithResource()
        {
            string[] g_materials = new string[] { "CZ85", "CZ84", "CZ83" };
            int material = Generic.random(0, 300);

            int id = 0;

            if(material > 200)
            {
                id = 2;
            }
            else if(material > 100)
            {
                id = 1;
            }

            string item = g_materials[id];

            Item i = ItemManager.GetItem(item);
            if (i != null)
            {
                Inventory.AddItem(this, item, 1);
                send(new SP_MaterialEarned(this, material));
            }
        }

        public void OnDie() 
        {
            Health = -1;
            isSpawned = false;

            if (room.GetSide(this) == (int)Room.Side.Derbaran)
            {
                room.KillsDerbaranLeft--;
                Log.WriteInfo(">---User-1345 - OnDie - KillsDerbaranLeft: " + room.KillsDerbaranLeft );
            }
            else
            {
                room.KillsNIULeft--;
                Log.WriteInfo(">---User-1345 - OnDie - KillsNIULeft: " + room.KillsNIULeft);
            }
// >--- Desde aqui ------- estaba desactivado-------
            if (room.heromode != null)
            {
                if (room.derbHeroUsr == roomslot)
                {
                    room.derbHeroUsr = -1;
                    room.derbHeroKill--;
                }
                else if (room.niuHeroUsr == roomslot)
                {
                    room.niuHeroUsr = -1;
                    room.niuHeroKill--;
                }
                room.heromode.CheckForNewRound();
            }
//>--- Hasta aqui----            
            if (isHacking)
            {
                isHacking = false;
                room.send(new SP_RoomHackMission(roomslot, (hackingBase == 0 ? room.HackPercentage.BaseA : room.HackPercentage.BaseB), 3, hackingBase));
            }

            if (hasC4)
            {
                room.send(new SP_Unknown(29985, 0, 0, 1, 0, 0, 0, 0, 0)); // Remove C4 from the user 
                room.PickuppedC4 = false;
                hasC4 = false;
                room.send(new SP_Unknown(29985, 0, -1, 1, 5, -1, 0, -1, 0)); // Spawn the C4
            }

            /*if(room.HasChristmasMap)
            {
                send(new SP_KillCount(SP_KillCount.ActionType.Hide));
            }*/

            rDeaths++;
            rPoints++;
            LastDieTick = Generic.timestamp + 1;
            classCode = "-1";
        }

        public string GetEquipment(int c)
        {
            string[] e = new string[8];
            for (int i = 0; i < e.Length; i++)
            {
                e[i] = equipment[c, i];
            }
            return string.Join(",", e);
        }

        public void SaveEquipment()
        {
            string[] eq = new string[5];
            for (int J = 0; J < 5; J++)
            {
                eq[J] = GetEquipment(J);
            }
            DB.RunQuery("UPDATE equipment SET class0 = '" + eq[0] + "', class1 = '" + eq[1] + "', class2 = '" + eq[2] + "', class3 = '" + eq[3] + "', class4 = '" + eq[4] + "' WHERE ownerid='" + userId + "'");
        }

        public string GetItemByID(string ID)
        {
            int inventoryID = -1;
            int.TryParse(ID.Remove(0, 1), out inventoryID);

            if (inventoryID >= 0)
            {
                return inventory[inventoryID].Split('-')[0];
            }
            return "I000";
        }

        public int HasSmileBadge
        {
            get
            {
                return (HasItem("CK01") ? 0 : 1); //>--- DISGUISE_BADGE
            }
        }

        public bool IsWhitelistedWeapon(string Weapon)
        {
            if (Weapon.Contains("-"))
            {
                return false;
            }
            //              DF_MP5         DQ_MEDIC_KIT_1         DR_SPANNER                                 P90                TMP9 8TH        DG_DRAGUNOV_SVD_RED     DH_M60_TUR          M249                 K400 Grenade       DC_K2               DG_M24              Colt                TMA 1A              DJ_PZF_3            Knuckle
            if (Weapon == "DF01" || Weapon == "DQ01" || Weapon == "DR01" || hasRetail(Weapon) || (Weapon == "DF02" || Weapon == "D601" || Weapon == "DG17" || Weapon == "DH06" || Weapon == "DH02") || Weapon == "DN01" || Weapon == "DC02" || Weapon == "DG05" || Weapon == "DB01" || Weapon == "DL01" || Weapon == "DJ01" || Weapon == "DA02")
            {
                return true;
            }

            /* Zombie */
            //              Strike          FIXED_MINIGUN           Lumber              Hook                Shaft               Crush               Suicidal            Infect
            if (Weapon == "DA50" || Weapon == "EA03" || Weapon == "DA51" || Weapon == "DA52" || Weapon == "DA53" || Weapon == "DA54" || Weapon == "DN51" || Weapon == "DN52")
            {
                return true;
            }

            /* Snow Fight Event*/

            /*if (room != null)
            {
                if (room.new_mode == 6 && room.new_mode == 2)
                {
                    if (Weapon != "D201" && Weapon != "D204")
                        return false;
                }
            }*/

            /* Misc */

            if (Weapon == "D001") // Drum Piece 
            {
                return true;
            }

            if (retail != null && (Weapon == "DQ02" || Weapon == "DO01")) //>--- DQ_MEDIC_KIT_2 -- Smoke G 
            {
                return true;
            }

            if (RetailSystem.Enabled && RetailSystem.IsRetail(Weapon))
            {
                return true;
            }

            return false;
        }

        public string GetInventoryCode(string ID)
        {
            int inventoryID = int.Parse(ID.Remove(0, 1));
            if (inventory[inventoryID].Length > 0)
            {
                return inventory[inventoryID].Split('-')[0].ToUpper();
            }
            return null;
        }

        public void LoadRetails()
        {
            if (RetailSystem.Enabled)
            {
                for (int i = 0; i < 5; i++)
                {
                    string actual = equipment[i, 7];
                    if (actual != "^" || !HasItem(actual))
                    {
                        string str = RetailSystem.GetRetailByClass(i);
                        if (str != null)
                        {
                            equipment[i, 7] = str;
                        }
                    }
                }
            }

            if (hasRetail() && retailclass != -1)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i != retailclass)
                    {
                        string rq = this.equipment[i, 7];
                        if (rq != null && (rq == "^" || !HasItem(rq)))
                        {
                            this.equipment[i, 7] = (i == 1 ? "DQ02" : "DO01"); //>--- DQ_MEDIC_KIT_2 : Smoke G
                        }
                    }
                }

                string r = equipment[retailclass, 7];
                if (r != retail)
                {
                    if (r.StartsWith("I"))
                    {
                        r = GetItemByID(r);
                    }

                    if (r == "^" || !HasItem(r))
                    {
                        equipment[retailclass, 7] = retail;
                    }
                }
            }
        }

        public int GetItemIndex(string Code)
        {
            if (Code.StartsWith("I"))
            {
                Code = GetInventoryCode(Code);
            }

            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i] != "^")
                {
                    string Item = inventory[i].Split('-')[0];
                    if (string.Compare(Item, Code, true) == 0)
                        return i;
                }
            }
            return -1;
        }

        public int GetCostumeIndex(string Code)
        {
            for (int i = 0; i < costume.Length; i++)
            {
                if (costume[i] != "^")
                {
                    string Item = costume[i].Split('-')[0];
                    if (string.Compare(Item, Code, true) == 0)
                        return i;
                }
            }
            return -1;
        }

        public bool HasCostume(string strCode)
        {
            if (strCode == "BA01" || strCode == "BA02" || strCode == "BA03" || strCode == "BA04" || strCode == "BA05") return true;
            if (GetCostumeIndex(strCode) != -1)
            {
                return true;
            }
            return false;
        }

        public bool HasItem(string strCode)
        {
            return GetItemIndex(strCode) != -1;
        }

        public void RefreshCash()
        {
            cash = int.Parse(DB.RunReaderOnce("cash", "SELECT * FROM users WHERE id='" + userId + "'").ToString());
            send(new SP_CashItemBuy(this));
        }


        public void RefreshDinars()
        {
            dinar = int.Parse(DB.RunReaderOnce("dinar", "SELECT * FROM users WHERE id='" + userId + "'").ToString());
        }

        
        public void SaveStats()
        {
            DB.RunQuery("UPDATE users SET kills='" + kills + "', deaths='" + deaths + "', headshots='" + headshots + "', wonMatchs = '" + wonMatchs + "', lostMatchs = '" + lostMatchs + "', killcount='" + eventcount + "', exp='" + exp + "' WHERE id='" + userId + "'");
        }

        public bool deleteItem(string item)
        {
            int index = GetItemIndex(item);
            if (index != -1)
            {
                inventory[index] = "^";

                DB.RunQuery("UPDATE equipment SET inventory = '" + Inventory.Itemlist(this) + "' WHERE ownerid = '" + userId + "'");
                return true;
            }

            return false;
        }

        public bool deleteCostume(string item)
        {
            int index = GetCostumeIndex(item);
            if (index != -1)
            {
                costume[index] = "^";

                DB.RunQuery("UPDATE users_costumes SET inventory = '" + Inventory.Costumelist(this) + "' WHERE ownerid = '" + userId + "'");
                return true;
            }

            return false;
        }

        public bool CheckForEvent(int id)
        {
            DataTable EventCheck = DB.RunReader("SELECT id FROM users_events WHERE userid='" + userId + "' AND eventid='" + id + "'");
            if (EventCheck.Rows.Count == 0)
            {
                return false;
            }
            return true;
        }

        public void AddEvent(int id, bool permanent = false)
        {
            DB.RunQuery("INSERT INTO users_events (eventid, userid, permanent, timestamp) VALUES ('" + id + "','" + userId + "', '" + (permanent ? 1 : 0) + "', '" + Generic.timestamp + "')");
        }

        public void CheckForCostume()
        {
            bool CostumeChanged = false;

            string engineer = costumes_char[0].Split(',')[0];
            string medic = costumes_char[1].Split(',')[0];
            string sniper = costumes_char[2].Split(',')[0];
            string assault = costumes_char[3].Split(',')[0];
            string heavy = costumes_char[4].Split(',')[0];

            if (HasCostume(engineer) == false)
            {
                costumes_char[0] = "BA01,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^";
                CostumeChanged = true;
            }

            if (HasCostume(medic) == false)
            {
                costumes_char[1] = "BA02,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^";
                CostumeChanged = true;
            }

            if (HasCostume(sniper) == false)
            {
                costumes_char[2] = "BA03,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^";
                CostumeChanged = true;
            }

            if (HasCostume(assault) == false)
            {
                costumes_char[3] = "BA04,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^";
                CostumeChanged = true;
            }

            if (HasCostume(heavy) == false)
            {
                costumes_char[4] = "BA05,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^";
                CostumeChanged = true;
            }

            for (int I = 0; I < costume.Length; I++)
            {
                string actual = costume[I];
                for (int j = 0; j < 5; j++)
                {
                    if (!HasCostume(actual) && actual != "^")
                    {
                        string[] str = costumes_char[j].Split(',');
                        for (int i = 0; i < str.Length; i++)
                        {
                            string c = str[i];
                            if (string.Compare(c, actual, true) == 0)
                            {
                                CostumeChanged = true;
                                costumes_char[j].Split(',')[i] = "^";
                            }
                        }
                    }
                }
            }

            if (CostumeChanged)
            {
                DB.RunQuery("UPDATE users_costumes SET class_0='" + costumes_char[0] + "', class_1='" + costumes_char[1] + "', class_2='" + costumes_char[2] + "', class_3='" + costumes_char[3] + "', class_4='" + costumes_char[4] + "' WHERE ownerid='" + userId + "'");
            }
        }

        #endregion

        #region  Messenger

        public void AddFriend(int uid, int requester)
        {
            if (!Friends.ContainsKey(uid) && uid != userId)
            {
                string nickname = null;
                User u = Managers.UserManager.GetUser(uid);
                if (u != null)
                {
                    nickname = u.nickname;
                }
                else
                {
                    nickname = DB.RunReaderOnce("nickname", "SELECT * FROM users WHERE id='" + uid + "'").ToString();
                }
                Messenger MessengerUser = new Messenger(uid, nickname, 5, requester);
                MessengerUser.isOnline = false;
                Friends.TryAdd(uid, MessengerUser);
            }
        }

        public void RemoveFriend(int uid)
        {
            if (Friends.ContainsKey(uid))
            {
                Friends[uid] = null;
                Messenger u;
                Friends.TryRemove(uid, out u);
            }
        }

        public Messenger GetFriend(int id)
        {
            if (Friends.ContainsKey(id))
                return (Messenger)Friends[id];
            return null;
        }

        public Messenger GetFriend(string nick)
        {
            var v = Friends.Values.Where(r => string.Compare(r.nickname, nick, true) == 0).FirstOrDefault();
            if (v is Messenger)
            {
                return v;
            }
            return null;
        }

        #endregion

        #region General Stuff

        public void AddDailyStats(int kills, int deaths, int headshots, int expearned, int dinarearned)
        {
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            if (dailystats)
            {
                DB.RunQuery("UPDATE users_stats SET totalexp='" + exp + "', nickname='" + nickname + "', headshots=headshots+" + headshots + ", country='" + country + "', premium='" + premium + "', exp=exp+" + expearned + ", dinar=dinar+" + dinarearned + ", kills=kills+" + kills + ", deaths=deaths+" + deaths + ", timestamp='" + Generic.timestamp + "' WHERE userid='" + userId + "' AND date='" + date + "'");
            }
            else
            {
                dailystats = true;
                DB.RunQuery("UPDATE users SET Lastdaystats='" + date + "' WHERE id='" + userId + "'");
                DB.RunQuery("INSERT INTO users_stats (userid, nickname, totalexp, kills, deaths, headshots, exp, dinar, premium, country, date, timestamp) VALUES ('" + userId + "', '" + nickname + "', '" + exp + "', '" + kills + "', '" + deaths + "', '" + headshots + "', '" + ExpEarned + "', '" + DinarEarned + "', '" + premium + "', '" + country + "', '" + date + "', '" + Generic.timestamp + "')");
            }
        }

        #endregion

        #region Connection

        public String IP = null;
        //public String Hostname { get { try { return Dns.GetHostEntry(IP).HostName; } catch { return null; } } }

        public IPEndPoint remoteEndPoint = null;
        public IPEndPoint localEndPoint = null;

        public long RemoteIP;
        public uint RemotePort;

        public long LocalIP;
        public uint LocalPort;

        public void RetrievePing()
        {
            try
            {
                Ping ping = new Ping();
                ping.PingCompleted += new PingCompletedEventHandler(RetrievePing_Complete);
                ping.SendAsync(IPAddress.Parse(this.IP), Configs.Server.MaxPing + 100);
            }
            catch (Exception ex)
            {
                Log.WriteError("Ping error: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void RetrievePing_Complete(object sender, PingCompletedEventArgs e)
        {
            PingReply p = e.Reply;
            if (p.Status == IPStatus.Success)
            {
                this.ping = (uint)Math.Ceiling((decimal)p.RoundtripTime);

                if (this.ping > Configs.Server.MaxPing)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        this.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Notice1, "You have a too high ping (" + this.ping + " ms). Max is " + Configs.Server.MaxPing + " ms", 999, "SYSTEM"));
                    }
                    this.disconnect();
                }
                // Log.WriteLine(nickname + " ping: " + ping + "ms"); // --- 
            }
        }

        public uint ConvertIPAddress(IPEndPoint ipeo)
        {
            return BitConverter.ToUInt32(ipeo.Address.GetAddressBytes(), 0);
        }

        public ushort ReversePort(IPEndPoint ipEndp)
        {
            byte[] reversedPort = BitConverter.GetBytes((ushort)ipEndp.Port);
            Array.Reverse(reversedPort);
            return BitConverter.ToUInt16(reversedPort, 0);
        }

        public void setRemoteEndPoint(IPEndPoint Target)
        {
            try
            {
                RemoteIP = ConvertIPAddress(Target);
                RemotePort = ReversePort(Target);
                remoteEndPoint = Target;
            }
            catch (Exception ex)
            {
                Log.WriteError("An error has occurred on setRemoteEndPoint: " + ex.Message);
            }
        }

        public void setLocalEndPoint(IPEndPoint Target)
        {
            try
            {
                LocalIP = ConvertIPAddress(Target);
                LocalPort = ReversePort(Target);
                localEndPoint = Target;
            }
            catch (Exception ex)
            {
                Log.WriteError("An error has occurred on setLocalEndPoint: " + ex.Message);
            }
        }

        public void send(Packet p)
        {
            try
            {
                byte[] sendBuffer = p.GetBytes();
                if (sendBuffer != null && sendBuffer.Length > 0)
                {
                    socket.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, new AsyncCallback(sendCallBack), null);
                }
            }
            catch
            {
                disconnect();
            }
            p.Dispose();
        }

        public void sendBuffer(byte[] buffer)
        {
            try
                
            {
                if (buffer != null && buffer.Length > 0) //>--- Esto esta en el original
                {
                    socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(sendCallBack), null);
                }
            }
            catch
            {
                disconnect();
            }
        }

        private void sendCallBack(IAsyncResult iAr)
        {
            try { socket.EndSend(iAr); }
            catch { }
        }

        public User(uint sessionId, Socket socket)
        {
            this.socket = socket;
            this.sessionId = sessionId;

            this.connectionId = (ushort)Networking.TCP.GetFreeConnectionID;

            try
            {
                IPEndPoint remoteIpEndPoint = socket.RemoteEndPoint as IPEndPoint;
                this.IP = remoteIpEndPoint.Address.ToString();
                Log.WriteFile("User Ip : " +remoteIpEndPoint.Address.ToString());
            }
            catch (Exception ex)
            {
                Log.WriteError("Unable to get IP address: " + ex.Message + " - " + ex.StackTrace);
            }

            this.inventory = new string[Configs.Server.Player.MaxInventorySlot];
            this.costume = new string[Configs.Server.Player.MaxCostumeSlot];

            remoteIp = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();
            localIp = (socket.LocalEndPoint as IPEndPoint).Address.ToString();
            
            for (int i = 0; i < 5; i++)
            {
                costumes_char[i] = "BA0" + (i + 1) + ",^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^,^"; //>--- S_Ghillie
            }

            /*AsyncCallback OnReceive = null; // >--- Añado esto y activo lo siguiente. El Game crashea. Desactivo.
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null); */
            Log.WriteFile("Starting Thread ReceiveData");
            Thread t = new Thread(ReceiveData);
            t.Start();
        }

        private void ReceiveData() //>--- Recibe datos del cliente
        {
            try
            {
                while (!disconnected && socket.Connected)
                {
                    int length = socket.Receive(buffer);
                    if (length > 0)
                    {
                        
                        byte[] packetBuffer = new byte[length];
                        Array.Copy(buffer, 0, packetBuffer, 0, length);

                       
                        /* Default - for Windows */
                        /* Windows-1250 - for Windows & Linux mono so cross-platform */

                        LastClientTick = Generic.timestamp;
                        String packetdecode = Utils.HexDump(packetBuffer);  //>--- - Llamada a Utils
                        packetBuffer = Game.Cryption.decrypt(packetBuffer); //>--- clientXor = 0x3E
                        string[] packets = Encoding.GetEncoding("Windows-1250").GetString(packetBuffer).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string packet in packets)
                        {
                            if (packet.Length > 5)
                            {
                                try
                                {
                                    using (Handler handler = Managers.Packet_Manager.ParsePacket(packet))
                                    {
                                        if (handler != null)
                                        {
                                            //string m = "";
                                            //m = handler.ToString();
                                            //if (!m.Contains("Ping"))
                                            // Log.WriteInfo(">---USR-1972- handler: " + handler + " - packet.Length: " + packet.Length);
                                            handler.Handle(this);
                                        }
                                        handler.Dispose();
                                        if (!packet.Contains("25600") && !packet.Contains("31490"))
                                        Log.WriteDebug(">---USR-1967- PACKET  IN :: " + packet); //>--- Paquetes de entrada.
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        disconnect();
                        break;
                    }
                }
            }
            catch
            {
                disconnect();
            }
        }

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
            GC.Collect();
        }

        public void RefreshFriends()
        {
            if (actualUserlistType == 0)
            {
                List<User> usersInLobby = new List<User>();
                foreach (Messenger r in Friends.Values.Where(r => r.isOnline && r.status != 5).ToList())
                {
                    User friend = Managers.UserManager.GetUser(r.id);
                    if (friend != null)
                    {
                        usersInLobby.Add(friend);
                    }
                }
                send(new SP_UserList(SP_UserList.Type.Friends, usersInLobby));
            }
        }

        public bool disconnected = false;

        public void disconnect()
        {
            if (this.socket != null)
            {
                try
                {
                    socket.Close();
                }
                catch { }
                this.socket = null;
            }

            foreach(Messenger musr in this.Friends.Values)
            {
                musr.Dispose();
            }
            
            this.Friends.Clear();
            this.OutboxItems.Clear();

            if (disconnected) return;
            disconnected = true;

            DB.RunQuery("UPDATE users SET online='0' WHERE id='" + userId + "'");

            try
            {
                if (room != null)
                {
                    if (spectating)
                    {
                        room.RemoveSpectator(this);
                    }
                    else
                    {
                        room.RemoveUser(roomslot);
                    }
                    room = null;
                }
            }
            catch { }

            if (clan != null)
            {
                if (clan.Users.ContainsKey(userId))
                {
                    User u;
                    clan.Users.TryRemove(userId, out u);
                }
                clan = null;
            }

            Managers.UserManager.RemoveUser(this);

            this.Dispose();
        }
        #endregion

        public bool IsConnectionAlive
        {
            get
            {
                if (socket == null) return false;
                if (!socket.Connected) return false;

                return true;
            }
        }

        public int lastShoxTick = 0;

        public int sessionStart { get; set; }

        public int heartBeatTime = -1;

        public DateTime PingTime { get; set; }

        public int lastKillUser = -1;

        public void ComeBackReward()
        {
            this.AddPremium(3, 15);

            this.cash += 10000;
            DB.RunQuery("UPDATE users SET cash='" + this.cash + "' WHERE id = '" + this.userId + "'");

            Inventory.AddItem(this, "DF35", 15); //>--- DF_K1_GOLD
            Inventory.AddItem(this, "DC33", 15); //>--- M4A1 Gold
            Inventory.AddItem(this, "DG08", 15); //>--- AW50F
            Inventory.AddItem(this, "DJ33", 15); //>--- RPG 7 Gold 
            Inventory.AddItem(this, "DN03", 15); //>--- T Grenade

            Inventory.AddCostume(this, "BD02", 15); //>--- Party_Hat
            Inventory.AddCostume(this, "BA11", 15); //>--- E_Classic
            Inventory.AddCostume(this, "BA12", 15); //>--- M_Classic
            Inventory.AddCostume(this, "BA13", 15); //>--- S_Classic
            Inventory.AddCostume(this, "BA14", 15); //>--- A_Classic
            Inventory.AddCostume(this, "BA15", 15); //>--- H_Classic

            send(new SP_CustomMessageBox("Welcome back, " + nickname + "! Come back reward has been given.\nCheck your inventory"));
        }

        public bool IsAlive()
        {
            return Health > 0 && ExplosiveAlive && isSpawned;
        }
    }
}
