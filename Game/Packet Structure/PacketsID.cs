using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server
{
    enum PacketID : ushort
    {           
        WelcomePacket = 24832,
        CharacterInfo = 25088,
        SwitchChannel = 28673,
        RoomList = 29184,
        RoomKick = 29505,
        Itemequipment = 29970,
        CreateRoom = 29440,
        Chat = 29696,
        DinarItemBuy = 30208,
        CashItemBuy = 30720,
        Outbox = 30752,
        RoomData = 30000,
        RoomHackMission = 29985,
        RoomVehicleUpdate = 29969,
        DeleteItem = 30224,
        DeleteCostume = 30225,
        UserList = 28928, // Old 28960
        JoinRoom = 29456,
        ScoreBoard = 30032,
        LeaveRoom = 29504,
        RoomPlantData = 29984,
        ZombieMultiplayerUpdate = 31490,
        ZombieSkillPointRequest = 31492,
        QuickJoinRoom = 29472,
        RoomInfoUpdate = 29201,
        RoomInvite = 29520,
        ClanRanking = 26464,
        CouponOpen = 25605,
        CouponBuy = 25606,
        CostumeEquip = 29971,
        CostumeBuy = 30209,
        Messenger = 32256,
        Clan = 26384,
        SpectateRoom = 29488,
        PingInformation = 25600,
        CarePackageOpen = 30272,
        CarePackageSendItem = 30273,
        AntiCheat = 46723,
        ShopCoupon = 30992,
        NewZombieStage = 30053,
        EscapeMode = 31507,
        GunSmith = 30995,
        Disconnect = 24576,
        DailyLoginEvent = 30993,
        RoomInviteOrQuickJoin = 29457,
        AchievementSystem = 32257,
        RankingList = 30816
    }
}
