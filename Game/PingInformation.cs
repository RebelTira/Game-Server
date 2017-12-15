using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Managers;

namespace Game_Server.Game
{
    class CP_PingInformation : Handler
    {
        public override void Handle(User usr)
        {
            int sessionId = int.Parse(getBlock(0));
            uint timeGetTime = uint.Parse(getBlock(1));

            if (usr.sessionStart + 5 < Generic.timestamp)
            {
                if (usr.tcpClient == null && !Configs.Server.Debug)
                {
                    Log.WriteDebug("[DEBUG] " + usr.nickname + " No TCP Client - kick out");
                    usr.disconnect();
                }
            }
            
            //TimeSpan ts = DateTime.Now - usr.PingTime;
            //usr.ping = (uint)(ts.Milliseconds - 10);
            //Log.WriteDebug("PING: " + usr.ping);
            /*if (usr.sessionId != sessionId)
            {
                Log.WriteError("Wrong SessionID on PingInformation (Local: " + usr.sessionId + " / Remote: " + sessionId + ")");
                Log.WriteDebug(string.Join(" ", getAllBlocks));
                usr.disconnect();
            }*/
        //    TimeSpan ts = DateTime.Now - usr.pingToServer;
        //    double mm = ts.TotalMilliseconds;
        //    usr.ping = (int)mm;
        }
    }

    class SP_PingInformation : Packet
    {
        enum EventType : int
        {
            Default = -1,
            ExpDinarEvent = 4,
            RandomHotTime = 16,
            WhiteChristmas = 64
        }

        public SP_PingInformation(User usr)
        {
            newPacket(25600);
            addBlock(5000);
            addBlock(usr.ping);
            addBlock(0);
            addBlock(EXPEventManager.EventTime);
            if (Configs.Server.RandomBoxEvent.hour == DateTime.Now.Hour)
            {
                addBlock((int)EventType.RandomHotTime);
            }
            else if(Configs.Server.Christmas.IsChristmas && Configs.Server.Christmas.enabled)
            {
                addBlock((int)EventType.WhiteChristmas);
            }
            else
            {
                addBlock(EXPEventManager.isRunning ? EXPEventManager.EventType : 0);
            }
            addBlock(EXPEventManager.EXPRate);
            addBlock(EXPEventManager.DinarRate);
            addBlock(usr.PremiumTimeLeft());
        }
    }
}
