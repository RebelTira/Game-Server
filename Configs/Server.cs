using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Configs
{
    class Server
    {
        public static int MaxSessions = 1000; // Max Session IDs
        public static int PingRequestTick = 5000; // In milliseconds
        public static int serverId = 1; // Server ID
        public static int ServerPort = 10375; // Login Server port
        public static int GameplayPort = 10376; // TCP Server 1 port
        public static string SystemName = "WRCHOVAR"; // Server chat (like SYSTEM >> You are muted 

        public static bool Debug = false;
        public static int ClientVersion = 0;
        public static int MaxSpectators = 5; // Max spectators for each room
        public static int MaxPing = 600; 

        /* Player */

        internal class Player
        {
            public static int MaxInventorySlot = 50; // Max Inventory Slots
            public static int MaxCostumeSlot = 50; // Max Costume Inventory Slots
            public static int LevelupDinar = 2500; // LevelupDinar * Level 
            public static int LevelupCash = 2500; // Cash when someone level ups
            public static bool CouponEvent = true; // Yes = Client open is allowed / No = Isn't allowed
            public static bool CarePackage = true; // Yes = Client open is allowed / No = Isn't allowed

            public static void Load()
            {
                Player.MaxInventorySlot = int.Parse(IO.ReadValue("Player", "MaxInventorySlots"));
                Player.MaxCostumeSlot = int.Parse(IO.ReadValue("Player", "MaxCostumesSlots"));
                Player.LevelupDinar = int.Parse(IO.ReadValue("Player", "LevelupDinar"));
                Player.LevelupCash = int.Parse(IO.ReadValue("Player", "LevelupCash"));
                Player.CouponEvent = bool.Parse(IO.ReadValue("Player", "CouponEvent"));
                Player.CarePackage = bool.Parse(IO.ReadValue("Player", "CarePackage"));
            }
        }

        /* Anti Cheat */

        internal class AntiCheat
        {
            public static string name = "ToXiiC";
            public static bool enabled = false;
            public static int routinetick = 15;
            public static int serverport = 5040;

            public static void Load()
            {
                AntiCheat.name = IO.ReadValue("AntiCheat", "AntiCheatName");
                AntiCheat.enabled = bool.Parse(IO.ReadValue("AntiCheat", "AntiEnabled"));
                AntiCheat.routinetick = int.Parse(IO.ReadValue("AntiCheat", "AntiRoutineTick"));
                AntiCheat.serverport = int.Parse(IO.ReadValue("AntiCheat", "ServerPort"));
            }
        }

        /* Clans */

        internal class Clan
        {
            public static int MaxClanSlot = 100; // Max users that can be in a clan
            public static int ClanDefaultSlot = 20; // Slots increases of 20 players
            public static int CreationCost = 10000;
            public static string DefaultAnnouncment = ""; // Default Announcment Message on create clan
            public static string DefaultDescription = ""; // Default Description Message on create clan
            
            public static void Load()
            {
                Clan.CreationCost = int.Parse(IO.ReadValue("Clan", "CreationCost"));
                Clan.MaxClanSlot = int.Parse(IO.ReadValue("Clan", "MaxSlots"));
                Clan.ClanDefaultSlot = int.Parse(IO.ReadValue("Clan", "DefaultSlot"));
                Clan.DefaultAnnouncment = IO.ReadValue("Clan", "DefaultAnnouncment");
                Clan.DefaultDescription = IO.ReadValue("Clan", "DefaultDescription");
            }
        }

        /* Channels */

        internal class Channels
        {
            public static bool Infantry = true; // True = Joinable - False = Not joinable
            public static bool Vehicular = true; // True = Joinable - False = Not joinable
            public static bool Zombie = true; // True = Joinable - False = Not joinable

            public static void Load()
            {
                Channels.Infantry = bool.Parse(IO.ReadAttribute("Server", "Channels", "Infantry"));
                Channels.Vehicular = bool.Parse(IO.ReadAttribute("Server", "Channels", "Vehicular"));
                Channels.Zombie = bool.Parse(IO.ReadAttribute("Server", "Channels", "Zombie"));
            }
        }

        /* Experience */

        internal class Experience
        {
            public static double ExpRate = 1; // Exp Rate
            public static double DinarRate = 1; // Dinar Rate 
            public static int OnKillPoints = 5; // Points gave for each kill
            public static int OnHSKillPoints = 3; // Points (+) gaved for each kill
            public static int OnDeathPoints = 1; // Points gave for each death
            public static int OnTakeFlag = 3; // Points gave for each flag
            public static int OnVehicleKillAdditional = 5; // (+) Points when someone kill you from vehicle
            public static int OnVehicleKill = 10; // (+) Points when you kill user in vehicle (each)
            public static int OnFriendHeal = 5; // Points given for each heal to a user with (< 300) HP
            public static int OnBombPlant = 10; // Points gave to the team (hidden from score) on bomb plant
            public static int OnBombDefuse = 20; // Points gave to the team (hidden from score) on bomb defuse
            public static int OnNormalPlaceUse = 5; // PX Medic Box, Flash, ...
            public static int OnLandPlaceUse = 5; // PX Ammo Box, M14, ...
            public static int OnMissionHack = 15; // Points gave on Mission Hack (SiegeWar 2)
            public static int MaxFlags = 30; // Numbers of flags that give points, like if i took 34 flags, i get only points from 30
            public static int MaxExperience = 6069; // Max experience that can be earned in a play
            public static int MaxDinars = 6069; // Max dinars that can be earned in a play

            public static void Load()
            {
                Experience.ExpRate = double.Parse(IO.ReadValue("Experience", "EXPRate").Replace(".", ","));
                Experience.DinarRate = double.Parse(IO.ReadValue("Experience", "DinarRate").Replace(".", ","));
                Experience.OnKillPoints = int.Parse(IO.ReadValue("Experience", "OnKillPoints"));
                Experience.OnHSKillPoints = int.Parse(IO.ReadValue("Experience", "OnHSKillPoints"));
                Experience.OnDeathPoints = int.Parse(IO.ReadValue("Experience", "OnDeathPoints"));
                Experience.OnTakeFlag = int.Parse(IO.ReadValue("Experience", "OnTakeFlag"));
                Experience.OnVehicleKillAdditional = int.Parse(IO.ReadValue("Experience", "OnVehicleKillAdditional"));
                Experience.OnVehicleKill = int.Parse(IO.ReadValue("Experience", "OnVehicleKill"));
                Experience.OnFriendHeal = int.Parse(IO.ReadValue("Experience", "OnFriendHeal"));
                Experience.OnBombPlant = int.Parse(IO.ReadValue("Experience", "OnBombPlant"));
                Experience.OnBombDefuse = int.Parse(IO.ReadValue("Experience", "OnBombDefuse"));
                Experience.OnNormalPlaceUse = int.Parse(IO.ReadValue("Experience", "OnNormalPlaceUse"));
                Experience.OnLandPlaceUse = int.Parse(IO.ReadValue("Experience", "OnLandPlaceUse"));
                Experience.OnMissionHack = int.Parse(IO.ReadValue("Experience", "OnMissionHack"));
                Experience.MaxFlags = int.Parse(IO.ReadValue("Experience", "MaxFlags"));
                Experience.MaxExperience = int.Parse(IO.ReadValue("Experience", "MaxExperience"));
                Experience.MaxDinars = int.Parse(IO.ReadValue("Experience", "MaxDinars"));
            }
        }

        /* Login Event */

        internal class LoginEvent
        {
            public static bool enabled = false;
            public static string[] items;
            public static int MinDays = 1;
            public static int MaxDays = 3;

            public static void Load()
            {
                LoginEvent.enabled = bool.Parse(IO.ReadValue("LoginEvent", "Enabled"));
                LoginEvent.items = IO.ReadValue("LoginEvent", "Items").Split(',');
                string[] days = IO.ReadValue("LoginEvent", "DaysRange").Split('-');
                int.TryParse(days[0], out LoginEvent.MinDays);
                int.TryParse(days[1], out LoginEvent.MaxDays); 
            }
        }

        internal class ChatEvent
        {
            public static bool enabled = false;
            public static int eventId = -1;
            public static bool daily = false;
            public static string message = "This_is_message_test";
            public static string popupMessage = "This_is_message_test";
            public static string[] items;
            public static int MinDays = 1;
            public static int MaxDays = 3;

            public static void Load()
            {
                ChatEvent.enabled = bool.Parse(IO.ReadValue("ChatEvent", "Enabled"));
                ChatEvent.daily = bool.Parse(IO.ReadValue("ChatEvent", "Daily"));
                ChatEvent.items = IO.ReadValue("ChatEvent", "Items").Split(',');
                ChatEvent.message = IO.ReadValue("ChatEvent", "Message");
                ChatEvent.popupMessage = IO.ReadValue("ChatEvent", "PopupMessage");
                string[] days = IO.ReadValue("ChatEvent", "DaysRange").Split('-');
                int.TryParse(days[0], out ChatEvent.MinDays);
                int.TryParse(days[1], out ChatEvent.MaxDays);
                int.TryParse(IO.ReadValue("ChatEvent", "EventID"), out ChatEvent.eventId);
            }
        }


        internal class Christmas
        {
            public static bool enabled = true;
            public static double ExpRate = 0.50;
            public static double DinarRate = 0.25;
            public static bool IsChristmas
            {
                get
                {
                    return (DateTime.Today.Day == 25 && DateTime.Today.Month == 12);
                }
            }

            public static void Load()
            {
                Christmas.enabled = bool.Parse(IO.ReadValue("Christmas", "Enabled"));;
                double.TryParse(IO.ReadValue("Christmas", "ExpRate"), out Christmas.ExpRate);
                double.TryParse(IO.ReadValue("Christmas", "DinarRate"), out Christmas.DinarRate);
            }
        }

        internal class RandomBoxEvent
        {
            public static bool Enabled = false;
            public static int hour = 14; // At which hour the box should be given
            public static string[] items;
            public static int MinDays = 1;
            public static int MaxDays = 3;
            public static string BoxCode = "CZ99";

            public static void Load()
            {
                RandomBoxEvent.Enabled = bool.Parse(IO.ReadValue("RandomBoxEvent", "Enabled"));
                RandomBoxEvent.hour = int.Parse(IO.ReadValue("RandomBoxEvent", "Hour"));
                RandomBoxEvent.items = IO.ReadValue("RandomBoxEvent", "Items").Split(',');
                RandomBoxEvent.BoxCode = IO.ReadValue("RandomBoxEvent", "BoxCode");
                string[] days = IO.ReadValue("RandomBoxEvent", "DaysRange").Split('-');
                int.TryParse(days[0], out RandomBoxEvent.MinDays);
                int.TryParse(days[1], out RandomBoxEvent.MaxDays);
            }
        }
        internal class ChristmasBoxEvent
        {
            public static bool Enabled = false;
            public static int hour = 14; // At which hour the box should be given
            public static string[] items;
            public static int MinDays = 1;
            public static int MaxDays = 3;
            public static string BoxCode = "CZ99";

            public static void Load()
            {
                ChristmasBoxEvent.Enabled = bool.Parse(IO.ReadValue("ChristmasBoxEvent", "Enabled"));
                ChristmasBoxEvent.hour = int.Parse(IO.ReadValue("ChristmasBoxEvent", "Hour"));
                ChristmasBoxEvent.items = IO.ReadValue("ChristmasBoxEvent", "Items").Split(',');
                ChristmasBoxEvent.BoxCode = IO.ReadValue("ChristmasBoxEvent", "BoxCode");
                string[] days = IO.ReadValue("ChristmasBoxEvent", "DaysRange").Split('-');
                int.TryParse(days[0], out ChristmasBoxEvent.MinDays);
                int.TryParse(days[1], out ChristmasBoxEvent.MaxDays);
            }
        }

        internal class SupplyBoxEvent
        {
            public static bool Enabled = false;

            public static void Load()
            {
                SupplyBoxEvent.Enabled = bool.Parse(IO.ReadValue("SupplyBoxEvent", "Enabled"));
            }
        }

        internal class ItemShop
        {
            //>---                                         CHG_CLANNICK, CHG_CLANEXTEND, CHG_CLANMARK, FRM, FC, CFRP
            public static string[] hiddenItems = new string[] { "CB02", "CB53", "CB54", "CZ83", "CZ84", "CZ85" };
            public static string[] attendanceBox = new string[] { "DF50", "DJ22", "DG48", "DC85" };
            //>---                                              DF_K1_BLOOD, DJ_RPG7_BLOOD, DG_AUG_BLOOD, DC_AK47_BLOOD
        }

        public static void Load()
        {
            try
            {
                serverId = int.Parse(IO.ReadValue("Server", "ServerID"));
                Debug = IO.ReadValue("Server", "DebugMode") == "true" ? true : false;
                ClientVersion = int.Parse(IO.ReadValue("Server", "ClientVersion"));
                ServerPort = int.Parse(IO.ReadValue("Server", "ServerPort"));
                GameplayPort = int.Parse(IO.ReadValue("Server", "GameplayPort"));
                MaxSessions = int.Parse(IO.ReadValue("Server", "MaxSessions"));
                PingRequestTick = int.Parse(IO.ReadValue("Server", "PingRequestTick"));
                SystemName = IO.ReadValue("Server", "SystemName");
                MaxSpectators = int.Parse(IO.ReadValue("Server", "MaxSpectators"));
                MaxPing = int.Parse(IO.ReadValue("Server", "MaxPing"));
                
                LoadSub();
            }
            catch (Exception ex)
            {
                Log.WriteError("Couldn't Load server info " + ex.Message);
            }
        }

        public static void LoadSub()
        {
            Channels.Load();

            Clan.Load();

            AntiCheat.Load();

            Player.Load();

            LoginEvent.Load();

            Experience.Load();

            ChatEvent.Load();

            Christmas.Load();

            RandomBoxEvent.Load();

            ChristmasBoxEvent.Load();

            SupplyBoxEvent.Load();
        }
    }
}
