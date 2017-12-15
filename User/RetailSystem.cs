using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Game_Server
{
    class RetailSystem
    {
        static ConcurrentDictionary<int, string> retails = new ConcurrentDictionary<int, string>();
        public static bool Enabled = false;

        public static string GetRetailByClass(int Class)
        {
            if (Enabled)
            {
                if (retails.ContainsKey(Class))
                {
                    string str = (string)retails[Class];
                    if (str != "^")
                    {
                        return str;
                    }
                }
            }
            return null;
        }

        public static bool IsRetail(string weapon)
        {
            if (retails.Values.Where(w => w == weapon).Count() > 0)
            {
                return true;
            }
            return false;
        }

        public static void LoadRetails()
        {
            try
            {
                retails.Clear();
                Enabled = bool.Parse(IO.ReadValue("RetailSystem", "Enabled"));
                if (Enabled)
                {
                    retails.TryAdd(0, IO.ReadValue("RetailSystem", "Engineer"));
                    retails.TryAdd(1, IO.ReadValue("RetailSystem", "Medic"));
                    retails.TryAdd(2, IO.ReadValue("RetailSystem", "Sniper"));
                    retails.TryAdd(3, IO.ReadValue("RetailSystem", "Assault"));
                    retails.TryAdd(4, IO.ReadValue("RetailSystem", "Heavy"));
                }
            }
            catch
            {
                Log.WriteError("Couldn't Load RetaiLSystem");
            }
        }
    }
}
