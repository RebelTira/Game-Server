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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace Game_Server
{
    /// <summary>
    /// This is the class where everything is initialized (also known as console startup process)
    /// </summary>
    class Program
    {
        public static bool running = false;
        public static LookupService ipLookup;
        public static Server server = null;

        static void ConsoleTitle()
        {
            while (true)
            {
                try
                {
                    TimeSpan ts = DateTime.Now - Process.GetCurrentProcess().StartTime;
                    int threads = Process.GetCurrentProcess().Threads.Count;
                    Console.Title = Configs.Console.title + " | " + threads + " T | [" + Managers.UserManager.ServerUsers.Count + " online - " + ts.Days + " days, " + ts.Hours + " hours, " + ts.Minutes + " minutes]";
                }
                catch { }
                Thread.Sleep(1000);
            }
        }
                
        static void Main(string[] args)
        {
       //     Console.Title = Configs.Console.title;
            Console.WindowWidth = Configs.Console.width;  // Estaban como comentarios
            Console.WindowHeight = Configs.Console.heigth; // Los activo para coger los datos de Console.cs

            // Credits part
            Console.WriteLine(@"  _______ ______   _______ _____ _____ _  _____      _____ ____  _____  ______ ");
            Console.WriteLine(@" |__   __/ __ \ \ / |_   _|_   _/ ____( )/ ____|    / ____/ __ \|  __ \|  ____|");
            Console.WriteLine(@"    | | | |  | \ V /  | |   | || |    |/| (___     | |   | |  | | |__) | |__   ");
            Console.WriteLine(@"    | | | |  | |> <   | |   | || |       \___ \    | |   | |  | |  _  /|  __|  ");
            Console.WriteLine(@"    | | | |__| / . \ _| |_ _| || |____   ____) |   | |___| |__| | | \ \| |____ ");
            Console.WriteLine(@"    |_|  \____/_/ \_|_____|_____\_____| |_____/     \_____\____/|_|  \_|______|");
            Console.WriteLine(@"|______________________________________________________________________________|");

            Log.WriteBlank();
            Console.WriteLine(" - Wrote by ToXiiC - Version TiraChinas");
            Console.WriteLine(" - Thanks to CodeDragon, Kill1212");
            Log.WriteBlank(2);
            //>--- Le añado la Hora y minutos para que escriba en diferentes archivos cada vez
            Log.setup("X-Game " + DateTime.Now.ToString("dd-MM H-mm") + ".log");  
			//>---Log.setup("ToXiiC_Core_" + DateTime.Now.ToString("dd-MM-yyyy") + ".log");
            
            new Thread(ConsoleTitle).Start();
            //new Thread(Managers.CommandManager.ConsoleCommand).Start();

            Type t = Type.GetType("Mono.Runtime");

            if (t != null)
            {
                Log.WriteLine("This GameServer is running under Mono VM!");
            }

            running = initializeStartup();
            if (running != true)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[" + (new string('-', Configs.Console.width - 2) + "]"));
                Console.ReadKey();
                Console.ReadKey();
            }
        }
        
        static bool initializeStartup()
        {
            string settingsFile = Application.StartupPath + "/gameserver.xml";
            
            IO.path = settingsFile;
            IO.workingDirectory = Application.StartupPath;

            if (!System.IO.File.Exists(settingsFile))
            {
                Log.WriteError("Error: Cannot find gameserver.xml");
                return false;
            }

            if (!System.IO.File.Exists(IO.workingDirectory + @"/items.bin"))
            {
                Log.WriteError("Error: Cannot find items.bin");
                return false;
            }

            string GeoIP = IO.workingDirectory + @"/GeoIP.dat";
            if (System.IO.File.Exists(GeoIP) == false)
            {
                Log.WriteError("Error: Cannot find GeoIP.dat");
                return false;
            }

            ipLookup = new LookupService(GeoIP, LookupService.GEOIP_MEMORY_CACHE);

            string host = IO.ReadValue("Database", "host");
            int port = int.Parse(IO.ReadValue("Database", "port"));
            string username = IO.ReadValue("Database", "user");
            string password = IO.ReadValue("Database", "password");
            string database = IO.ReadValue("Database", "database");
            int poolsize = int.Parse(IO.ReadValue("Database", "poolsize"));

            DB.openConnection(host, port, database, username, password, poolsize);

            Configs.Main.setup();

            DB.RunQuery("UPDATE users SET online='0' WHERE serverid='" + Configs.Server.serverId + "' OR serverid='-1'");
            Log.WriteLine("All accounts have been set offline");

            Managers.ItemManager.DecryptBinFile(IO.workingDirectory + "//items.bin");
            Managers.ItemManager.LoadItems();

            Managers.ChannelManager.Setup();
            Managers.EXPEventManager.Load();
            Managers.MapDataManager.Load();
            Managers.ZombieManager.Load();
            Managers.VehicleManager.Load();
            Managers.ClanManager.Load();
            Managers.RoutineManager.Load();
            Managers.CarePackage.Load();
            Managers.NoticeManager.Load();
            Managers.BanManager.Load();
            Managers.GunSmithManager.Load();
            Managers.WordFilterManager.Load();
            Managers.UserManager.setup();
            Managers.Packet_Manager.setup();
            Managers.RoomPacketManager.setup();
            Game.RankingList.Load();
            GlobalServers.LoadServers();
            /*
            for (int i = 0; i < 100; i++)
            {
                Room r = new Room();
                r.id = i;
                r.name = "I'm the room " + i;
                r.levellimit = (i < 5 ? i : 0);
                r.maxusers = 24;
                r.channel = 1;
                r.status = 2;
                r.mode = 3;
                r.type = 0;
                r.rounds = 7;
                r.ping = 0;
                r.mapid = i;
                r.userlimit = false;
                r.ch = Managers.ChannelManager.channels[r.channel];
                r.premiumonly = (i % 2 == 0 ? 1 : 0);
                Log.WriteDebug("Added room " + i + " to the stack(overflow)");
                Managers.ChannelManager.channels[r.channel].AddRoom(r.id, r);
            }
            */
            if (!Networking.NetworkSocket.InitializeSocket(Configs.Server.ServerPort))  //>---  10375
            {
                Log.WriteError("Error: Cannot Initialize a new socket on the port " + Configs.Server.ServerPort);
                return false;
            }

            if(!Networking.TCP.Start(Configs.Server.GameplayPort))  //>---  10376
            {
                Log.WriteError("Error: Cannot Initialize a new socket on the port " + Configs.Server.GameplayPort);
                return false;
            }

            if (Configs.Server.AntiCheat.enabled)
            {
                AntiCheatServer ac = new AntiCheatServer();
                Anti_Cheat.Structure.PacketManager.Load();
                if (!ac.Initialize(Configs.Server.AntiCheat.serverport))
                {
                    Log.WriteError("Error: Cannot Initialize a new socket on the port " + Configs.Server.AntiCheat.serverport);
                    return false;
                }
            }

            if (Configs.Web.allow)
            {
                if (!Web.WebManager.openSocket(Configs.Web.port))
                {
                    Log.WriteError("Error: Cannot Initialize a new web socket on the port " + Configs.Web.port);
                }
            }

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            return true;
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Log.WriteError("Unhandled exception [2]: " + (e.Exception as Exception).Message + " " + (e.Exception as Exception).StackTrace);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.WriteError("Unhandled exception [1]: " + (e.ExceptionObject as Exception).Message + " " + (e.ExceptionObject as Exception).StackTrace);
        }

        public static void shutDown()
        {
            if (!running) return;
            running = false;

            DB.RunQuery("UPDATE users SET online='0' WHERE serverid='" + Configs.Server.serverId + "'");

            foreach (Room r in Managers.ChannelManager.GetAllRooms())
            {
                r.EndGame();
                r.remove();
            }

            foreach (User usr in Managers.UserManager.getAllUsers())
            {
                try
                {
                    usr.disconnect();
                }
                catch { }
            }

            Environment.Exit(0);
        }
    }
}
