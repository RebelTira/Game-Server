using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Game_Server.Game
{
    class CP_WelcomePacket : Handler
    {
        public override void Handle(User usr)
        {
            usr.macAddress = getBlock(2);
            if (usr.macAddress.Length > 0 && Generic.IsAlphaNumeric(usr.macAddress))
            {
                if (!Managers.BanManager.isMacBanned(usr.macAddress))
                {
                    /* If you want to insert mac ban do a check using the Ban Manager class */
                    usr.send(new SP_WelcomePacket(usr));
                }
                else
                {
                    Log.WriteError(usr.IP + " -> tried to connect with Mac Banned");
                    usr.disconnect();
                }
            }
            else
            {
                Log.WriteError("Invalid Mac Address from " + usr.username);
                usr.disconnect();
            }
        }
    }

    class SP_WelcomePacket : Packet
    {
        internal enum ErrorCodes
        {
            Success = 1,
            ClientVersionMissmatch = 90020,
        }

        public int WeekCalculation(DateTime dt)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }

        public SP_WelcomePacket(ErrorCodes ErrorCode)
        {
            newPacket(24832);
            addBlock((int)ErrorCode);
        }

        public SP_WelcomePacket(User usr)
        {
            string date = DateTime.Now.Second + "/" + DateTime.Now.Minute + "/" + (DateTime.Now.Hour + 18) + "/" + (DateTime.Now.Day - 1) + "/" + (DateTime.Now.Month - 1) + "/" + (DateTime.Now.Year - 1900) + "/" + WeekCalculation(DateTime.Now) + "/" + DateTime.Now.DayOfYear + "/0";

            //Log.WriteDebug(date);

            newPacket(24832);
            addBlock(1);
            addBlock(date);
            addBlock(usr.connectionId);
        }
    }
}
