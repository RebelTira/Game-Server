using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Game;
using Game_Server.Room_Data;
using Game_Server.GameModes;

namespace Game_Server.Managers
{
    /// <summary>
    /// This class handles and contains all used packets
    /// </summary>
    class Packet_Manager
    {
        private static Dictionary<ushort, Handler> packets = new Dictionary<ushort, Handler>();

        public static void setup()
        {
            addPacket((ushort)PacketID.WelcomePacket, new CP_WelcomePacket());
            addPacket((ushort)PacketID.CharacterInfo, new CP_CharacterInfo());
            addPacket((ushort)PacketID.SwitchChannel, new CP_SwitchChannel());
            addPacket((ushort)PacketID.RoomList, new CP_RoomList());
            addPacket((ushort)PacketID.RoomKick, new CP_RoomKick());
            addPacket((ushort)PacketID.Itemequipment, new CP_ItemEquipment());
            addPacket((ushort)PacketID.CreateRoom, new CP_CreateRoom());
            addPacket((ushort)PacketID.Chat, new CP_Chat());
            addPacket((ushort)PacketID.DinarItemBuy, new CP_DinarItemBuy());
            addPacket((ushort)PacketID.CashItemBuy, new CP_CashItemBuy());
            addPacket((ushort)PacketID.Outbox, new CP_Outbox());
            addPacket((ushort)PacketID.RoomData, new CP_RoomData());
            addPacket((ushort)PacketID.RoomHackMission, new CP_RoomHackMission());
            addPacket((ushort)PacketID.RoomVehicleUpdate, new CP_RoomVehicleUpdate());
            addPacket((ushort)PacketID.DeleteItem, new CP_DeleteItem());
            addPacket((ushort)PacketID.DeleteCostume, new CP_DeleteCostume());
            addPacket((ushort)PacketID.UserList, new CP_UserList());
            addPacket((ushort)PacketID.JoinRoom, new CP_JoinRoom());
            addPacket((ushort)PacketID.ScoreBoard, new CP_ScoreBoard());
            addPacket((ushort)PacketID.LeaveRoom, new CP_LeaveRoom());
            addPacket((ushort)PacketID.RoomPlantData, new CP_RoomPlantData());
            addPacket((ushort)PacketID.ZombieMultiplayerUpdate, new CP_ZombieMultiPlayer());
            addPacket((ushort)PacketID.ZombieSkillPointRequest, new CP_ZombieSkillPointRequest());
            addPacket((ushort)PacketID.QuickJoinRoom, new CP_QuickJoinRoom());
            addPacket((ushort)PacketID.RoomInfoUpdate, new CP_RoomInfoUpdate());
            addPacket((ushort)PacketID.RoomInvite, new CP_RoomInvite());
            addPacket((ushort)PacketID.ClanRanking, new CP_ClanRanking());
            addPacket((ushort)PacketID.CouponOpen, new CP_CouponEvent());
            addPacket((ushort)PacketID.CouponBuy, new CP_CouponBuy());
            addPacket((ushort)PacketID.CostumeBuy, new CP_CostumeBuy());
            addPacket((ushort)PacketID.CostumeEquip, new CP_CostumeEquip());
            addPacket((ushort)PacketID.Messenger, new CP_Messenger());
            addPacket((ushort)PacketID.SpectateRoom, new CP_Spectate());
            addPacket((ushort)PacketID.PingInformation, new CP_PingInformation());
            addPacket((ushort)PacketID.Clan, new CP_Clan());
            addPacket((ushort)PacketID.CarePackageOpen, new CP_CarePackage());
            addPacket((ushort)PacketID.CarePackageSendItem, new CP_CarePackageSendItem());
            addPacket((ushort)PacketID.AntiCheat, new CP_AntiCheat());
            addPacket((ushort)PacketID.ShopCoupon, new CP_ShopCoupon());
            addPacket((ushort)PacketID.NewZombieStage, new CP_ZombieNewStage());
            addPacket((ushort)PacketID.EscapeMode, new CP_EscapeMode()); 
            addPacket((ushort)PacketID.GunSmith, new CP_GunSmith());
            addPacket((ushort)PacketID.Disconnect, new CP_Disconnect());
            addPacket((ushort)PacketID.DailyLoginEvent, new CP_LoginEvent());
            addPacket((ushort)PacketID.RoomInviteOrQuickJoin, new CP_RoomInviteOrQuickJoin());
            addPacket((ushort)PacketID.AchievementSystem, new CP_AchievementSystem());
            addPacket((ushort)PacketID.RankingList, new CP_RankingList());
                
            Log.WriteLine(">--- PcktMang- Loaded " + packets.Count + " packet handlers");
        }

        public static Handler ParsePacket(string packetStr)
        {
            string[] packetBlocks = packetStr.Split(' ');
            uint timeGetTime;
            uint.TryParse(packetBlocks[0], out timeGetTime);
            ushort packetId;
            ushort.TryParse(packetBlocks[1], out packetId);
            if ((packetBlocks[1]) != ("25600") && (packetBlocks[1]) != ("31490"))
            { 
            Log.WriteLine(">--- PcktMang-79 packetId: " + (packetBlocks[1]));
            }
            if (timeGetTime > 0 && packetId > 0)
            {
                if (packets.ContainsKey(packetId))
                {
                    string[] resizedBlocks = new string[packetBlocks.Length - 2];
                    Array.Copy(packetBlocks, 2, resizedBlocks, 0, packetBlocks.Length - 2);
                    Handler handler = (Handler)packets[packetId];
                    handler.FillData(timeGetTime, packetId, resizedBlocks);
                    //>--- PARA ABREVIAR El LOG  :) ++++++++++++++++++
                    string m = "";
                    m = handler.ToString();
                    if (!m.Contains("Ping") && !m.Contains("CP_ZombieMultiPlayer"))
                    {
                        Log.WriteInfo(">--- PacketManager-90  handler: " + handler  + " " + packetId );
                    }
                    //>---      +++++++++++++++++++++++++++
                    return handler;
                }
                else if (Configs.Server.Debug)
                {
                    Log.WriteError("Unhandled Packet ID " + packetId);
                }
            }
            return null;
        }

        private static void addPacket(ushort id, Handler handler)
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
