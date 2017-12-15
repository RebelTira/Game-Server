using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Managers;

namespace Game_Server
{
    internal struct TempItem
    {
        public string name;
        public string code;
        public ushort days;

        public TempItem(string code, ushort days)
        {
            this.name = "Unknown";
            Item i = ItemManager.GetItem(code);
            
            if (i != null)
            {
                this.name = i.Name;
            }
            
            this.code = code;
            this.days = days;
        }
    }
}
