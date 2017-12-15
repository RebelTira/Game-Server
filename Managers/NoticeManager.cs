using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Game_Server.Managers
{
    class NoticeManager
    {
        private static Thread NoticeThread = null;
        private static string[] Messages;
        private static int rn = 0;

        public static bool Load()
        {
            try
            {
                LoadMessages();
                if (NoticeThread == null)
                {
                    NoticeThread = new Thread(noticeLoop);
                    NoticeThread.Priority = ThreadPriority.Lowest;
                    NoticeThread.Start();
                }
                else
                {
                    NoticeThread.Start();
                }
                Log.WriteLine("Successfully loaded [" + Messages.Length + "] notices");
            }
            catch { }
            return true;
        }

        private static void LoadMessages()
        {
            DataTable dt = DB.RunReader("SELECT * FROM notices WHERE deleted='0'");
            Messages = new string[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                Messages[i] = row["message"].ToString();
            }
        }

        private static void noticeLoop()
        {
            while (true)
            {
                try
                {
                    rn++;
                    if (rn >= 3)
                    {
                        rn = 0;
                        UserManager.sendToServer(new Game.SP_Chat("NOTICE", Game.SP_Chat.ChatType.Notice1, UserManager.ServerUsers.Count + " online players. Server " + Generic.runningSince.ToLower() + " Server Time: " + DateTime.Now.ToString("HH:mm") + " - " + DateTime.Now.ToString("dd/MM/yy"), 999, "NULL"));
                    }
                    else
                    {
                        LoadMessages();
                        if (Messages.Length > 0)
                        {
                            int iMessage = (new System.Random()).Next(0, Messages.Length - 1);
                            UserManager.sendToServer(new Game.SP_Chat("NOTICE", Game.SP_Chat.ChatType.Notice1, Messages[iMessage], 999, "NULL"));
                        }
                    }
                }
                catch { }
                Thread.Sleep(300000);
            }
        }
    }
}
