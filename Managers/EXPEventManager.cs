using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;

namespace Game_Server.Managers
{
    class EXPEventManager
    {
        public static bool isRunning = false;
        public static int EventType = -1; // -1 = Nothing, 4 = EXP Event, 45 = Hot Time
        public static int EventTime = -1;
        public static double EXPRate = -1;
        public static double DinarRate = -1;
        private static Thread EventThread = null;

        public static void Load()
        {
            try
            {
                isRunning = false;
                EventThread = new Thread(EventLoop);
                EventThread.Priority = ThreadPriority.Lowest;
                EventThread.Start();
                DB.RunQuery("UPDATE serverinfo SET value='0' WHERE name='expevent'");
                Log.WriteLine("EXP/Dinar event manager is running properly!");
            }
            catch (Exception ex)
            {
                Log.WriteError("EXP/Dinar event manager didn't Load properly: " + ex.Message);
            }
        }

        private static void EventLoop()
        {
            while (true)
            {
                if (isRunning)
                {
                    if (EventTime > 0)
                    {
                        EventTime -= 5;
                    }
                    else if (EventTime <= 0)
                    {
                        StopEvent();
                    }
                }
                Thread.Sleep(5000);
            }
        }

        public static void StartEvent(int minute, double exp, double dinar)
        {
            isRunning = true;
            EventTime = minute;
            EXPRate = exp;
            DinarRate = dinar;
            UserManager.sendToServer(new Game.SP_ExpEvent(Game.SP_ExpEvent.EventCodes.EXP_Activate));

            int min = (int)Math.Ceiling((decimal)EventTime / 60);

            DateTime dt = DateTime.Now;
            dt = dt.AddMinutes(min);

            DB.RunQuery("UPDATE serverinfo SET value='1' WHERE name='expevent'");
            DB.RunQuery("UPDATE serverinfo SET value='" + dt.ToString("HH:mm dd/MM/yy") + "' WHERE name='expexpire'");

            string rate = (int)Math.Ceiling((decimal)(exp * 100)) + "%, " + (int)Math.Ceiling((decimal)(dinar * 100)) + "%";

            DB.RunQuery("UPDATE serverinfo SET value='" + rate + "' WHERE name='exprate'");

            UserManager.ServerUsers.Values.Where(u => u != null).ToList().ForEach(usr =>
            {
                usr.send(new Game.SP_PingInformation(usr));
            });
        }

        public static void StopEvent()
        {
            isRunning = false;
            EventTime = -1;
            EXPRate = 1;
            DinarRate = 1;
            UserManager.sendToServer(new Game.SP_ExpEvent(Game.SP_ExpEvent.EventCodes.EXP_Deactivate));
            DB.RunQuery("INSERT INTO admincp_logs (adminid, log, date, timestamp) VALUES ('-1', 'EXP / Dinar event end!', '" + Generic.currentDate + "','" + Generic.timestamp + "')");
            DB.RunQuery("UPDATE serverinfo SET value='0' WHERE name='expevent'");
        }
    }
}
