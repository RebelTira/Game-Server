using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using System.Threading;

namespace Game_Server.Managers
{
    class EventManager
    {
        public Thread OneMinuteThread = null;

        public void Load()
        {
        }

        public void OneMinuteTick ()
        {
            while (true)
            {
                Thread.Sleep(60000);
            }
        }
    }
}
