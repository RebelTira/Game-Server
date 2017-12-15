using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Room_Data;

namespace Game_Server.Managers
{
    class RoomPacketManager
    {
        private static Dictionary<int, RoomDataHandler> packets = new Dictionary<int, RoomDataHandler>();

        public static void setup()
        {
            addPacket((int)Subtype.Start, new RoomHandler_RoomStart());
            addPacket((int)Subtype.MapChange, new RoomHandler_MapChange());
            addPacket((int)Subtype.AutostartChange, new RoomHandler_AutostartChange());
            addPacket((int)Subtype.KillLimitDeathmatchChange, new RoomHandler_KillLimitDeathmatchChange());
            addPacket((int)Subtype.KillLimitExplosiveChange, new RoomHandler_KillLimitExplosiveChange());
            addPacket((int)Subtype.PingChange, new RoomHandler_PingChange());
            addPacket((int)Subtype.ReadyState, new RoomHandler_ReadyState());
            addPacket((int)Subtype.Suicide, new RoomHandler_Suicide());
            addPacket((int)Subtype.SwitchTeam, new RoomHandler_SwitchTeam());
            addPacket((int)Subtype.RoomReady, new RoomHandler_RoomReady());
            addPacket((int)Subtype.BackToRoom, new RoomHandler_BackToRoom());
            addPacket((int)Subtype.Spawn, new RoomHandler_Spawn());
            addPacket((int)Subtype.WorldDamage, new RoomHandler_WorldDamage());
            addPacket((int)Subtype.UserLimit, new RoomHandler_UserLimit());
            addPacket((int)Subtype.WeaponSwitch, new RoomHandler_WeaponSwitch());
            addPacket((int)Subtype.Heal, new RoomHandler_Heal());
            addPacket((int)Subtype.Place, new RoomHandler_Place());
            addPacket((int)Subtype.PlaceUse, new RoomHandler_PlacementUse());
            addPacket((int)Subtype.ZombieDropUse, new RoomHandler_ZombieDropUse());
            addPacket((int)Subtype.ArtilleryRequest, new RoomHandler_ArtillerySupport());
            addPacket((int)Subtype.JoinVehicle, new RoomHandler_JoinVehicle());
            addPacket((int)Subtype.ChangeVehicleSeat, new RoomHandler_ChangeVehicleSeat());
            addPacket((int)Subtype.LeaveVehicle, new RoomHandler_LeaveVehicle());
            addPacket((int)Subtype.RepairVehicle, new RoomHandler_RepairVehicle());
            addPacket((int)Subtype.TotalWarSpawnVehicle, new RoomHandler_TotalWarSpawnVehicle());
            addPacket((int)Subtype.VoteKick, new RoomHandler_VoteKick());
            addPacket((int)Subtype.Flag, new RoomHandler_Flag());
            addPacket((int)Subtype.TotalWarFlag, new RoomHandler_Flag());
            addPacket((int)Subtype.Damage, new RoomHandler_Damage());
            addPacket((int)Subtype.DamageVehicle, new RoomHandler_DamageVehicle());
            addPacket((int)Subtype.ZombieExplode, new RoomHandler_ZombieExplode());
            addPacket((int)Subtype.AmmoRecharge, new RoomHandler_AmmoRecharge());
            addPacket((int)Subtype.CaptureModeRequest, new RoomHandler_CaptureModeRequest());

            Log.WriteLine("Loaded " + packets.Count + " room data handlers");
        }

        public static RoomDataHandler ParsePacket(int subtype, object[] blocks)
        {
            if (packets.ContainsKey(subtype))
            {
                RoomDataHandler handler = (RoomDataHandler)packets[subtype];
                handler.FillData(subtype, blocks);
                return handler;                
            }
            return null;
        }

        private static void addPacket(int id, RoomDataHandler handler)
        {
            if (!packets.ContainsKey(id))
            {
                packets.Add(id, handler);
            }
            else
            {
                Log.WriteError("Packet Manager already contains packetID: " + id);
            }
        }
    }
}
