using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;

namespace Game_Server.Managers
{
    class MapDataManager
    {
        public static ConcurrentDictionary<int, MapData> datas = new ConcurrentDictionary<int, MapData>();
        public static void Load()
        {
            datas.Clear();

            DataTable dt = DB.RunReader("SELECT * FROM maps");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                int mapId = int.Parse(row["mapid"].ToString());
                if (!datas.ContainsKey(mapId))
                {
                    try
                    {
                        //string[] EventInfo = DB.RunReader("SELECT map , maxhealth, seats, positionX, positionY, positionZ,axisX,axisY,axisZ FROM vehicle WHERE id=" + _VehicleIDs[I].ToString());
                        string name = row["name"].ToString();
                        int flags = int.Parse(row["flags"].ToString());
                        string[] splitDefault = row["defaultflags"].ToString().Split(new char[] { '|' });
                        int derb = int.Parse(splitDefault[0]);
                        int niu = int.Parse(splitDefault[1]);
                        string vehString = row["vehicles"].ToString();
                        MapData sMap = new MapData(mapId, name, flags, derb, niu, vehString);
                        datas.TryAdd(mapId, sMap);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteError("Coudln't Load map id " + mapId + "[" + ex.Message + "]");
                    }
                }
                else
                {
                    Log.WriteError("Map ID [" + mapId + "] its already in the dictionary, maybe some duplicate (?)");
                }
            }
            Log.WriteLine("Successfully loaded [" + datas.Count + "] MapDatas");
        }

        public static MapData GetMapByID(int MapID)
        {
            if(datas.ContainsKey(MapID))
            {
                return (MapData)datas[MapID];
            }
            return null;
        }
    }

    class MapData
    {
        public int mapId;
        public string name;
        public int flags;
        public int derb;
        public int niu;
        public string vehicleString = null;
        
        public MapData(int mapId, string name, int flags, int derb, int niu, string vehicleString)
        {
            this.mapId = mapId;
            this.name = name;
            this.flags = flags;
            this.derb = derb;
            this.niu = niu;
            this.vehicleString = vehicleString;
        }
    }
}
