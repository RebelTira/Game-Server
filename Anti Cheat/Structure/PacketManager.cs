using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Anti_Cheat.Structure
{
    enum Packets
    {
        Authentication = 10300
    }
    class PacketManager
    {
        private static Dictionary<int, Anti_Cheat.Structure.Handler> handlers = new Dictionary<int, Structure.Handler>();

        public static void Load()
        {
            AddPacket((int)Packets.Authentication, new Data.CP_Authentication());
        }

        public static Handler ParsePacket(string packetStr)
        {
            string[] packetBlocks = packetStr.Split(' ');
            uint timeGetTime;
            uint.TryParse(packetBlocks[0], out timeGetTime);
            int packetId;
            int.TryParse(packetBlocks[1], out packetId);

            if (timeGetTime > 0 && packetId > 0)
            {
                if (handlers.ContainsKey(packetId))
                {
                    string[] resizedBlocks = new string[packetBlocks.Length - 2];
                    Array.Copy(packetBlocks, 2, resizedBlocks, 0, packetBlocks.Length - 2);
                    Handler handler = (Handler)handlers[packetId];
                    handler.FillData(timeGetTime, packetId, resizedBlocks);
                    Log.WriteInfo("PacketManager-37  handler: " + handler);
                    return handler;
                }
                else if (Configs.Server.Debug)
                {
                    Log.WriteError("Unhandled AC Packet ID " + packetId);
                }
            }
            return null;
        }

        private static void AddPacket(int packetId, Structure.Handler h)
        {
            if (!handlers.ContainsKey(packetId))
            {
                handlers.Add(packetId, h);
            }
            else
            {
                Log.WriteError(packetId + " key is already used in the dictionary (AC)");
            }
        }
    }
}
