using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Game_Server.Game;

namespace Game_Server.Managers
{
    class CommandManager
    {
        private static Thread consoleThread = null;

        public static void Load()
        {
            consoleThread = new Thread(ConsoleCommand);
            consoleThread.Priority = ThreadPriority.Lowest;
            consoleThread.Start();
        }

        public static void ConsoleCommand()
        {
            while (true)
            {
                try
                {
                    string CommandString = Console.ReadLine();
                    string[] CommandLine = CommandString.Split(new char[] { ' ' });
                    switch (CommandLine[0])
                    {
                        case "notice":
                            {
                                UserManager.sendToServer(new SP_Chat("NOTICE", SP_Chat.ChatType.Notice1, CommandString.Substring(7), 999, "NULL"));
                                Log.WriteLine("Successfully notice: " + CommandString.Substring(7));
                                break;
                            }
                        case "stop":
                            {
                                Log.WriteLine("Server is going to be shutdown!");
                                UserManager.sendToServer(new SP_Chat("NOTICE", SP_Chat.ChatType.Notice1, "Server is going to be restarted, sorry!!!", 999, "NULL"));
                                UserManager.sendToServer(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Room_ToAll, Configs.Server.SystemName + " >> Server is going to be restarted, sorry!!!", 999, "Server"));
                                Thread.Sleep(500);
                                Program.shutDown();
                                break;
                            }
                    }
                }
                catch { }
                Thread.Sleep(2000);
            }
        }
    }
}
