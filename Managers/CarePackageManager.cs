using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Game_Server.Managers
{
    class CarePackageItem
    {
        public int Price;
        public int Method;
        public string Item, Item1, Item2, Item3, Item4;
        public int days, days1, days2, days3, days4;
    }

    class CarePackage
    {
        public static Dictionary<int, CarePackageItem> items = new Dictionary<int, CarePackageItem>();

        public static CarePackageItem GetItem(int ID)
        {
            if (items.ContainsKey(ID))
                return (CarePackageItem)items[ID];
            return null;
        }

        public static void Load()
        {
            items.Clear();
            DataTable dt = DB.RunReader("SELECT * FROM carepackage");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                CarePackageItem c = new CarePackageItem();
                c.Item = row["itemcode"].ToString();
                c.Price = int.Parse(row["price"].ToString());
                c.Method = int.Parse(row["method"].ToString());
                c.days = int.Parse(row["itemdays"].ToString());
                c.Item1 = row["loseitem1"].ToString();
                c.days1 = int.Parse(row["loseitemdays1"].ToString());
                c.Item2 = row["loseitem2"].ToString();
                c.days2 = int.Parse(row["loseitemdays2"].ToString());
                c.Item3 = row["loseitem3"].ToString();
                c.days3 = int.Parse(row["loseitemdays3"].ToString());
                c.Item4 = row["loseitem4"].ToString();
                c.days4 = int.Parse(row["loseitemdays4"].ToString());
                items.Add(i, c);
            }
        }
    }
}
