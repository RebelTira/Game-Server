using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Configs
{
    class Main
    {
        public static void setup()
        {
            try
            {
                Server.Load();
                Web.Load();
                RetailSystem.LoadRetails();
                Log.WriteLine("Configs successfully loaded");
            }
            catch (Exception ex)
            {
                Log.WriteError("Couldn't setup configs (" + ex.Message + ") @ " + ex.StackTrace);
            }
        }
    }
}
