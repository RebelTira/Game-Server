/*
 _____   ___ __  __  _____ _____   ___  _  __              ___   ___   __    __ 
/__   \ /___\\ \/ /  \_   \\_   \ / __\( )/ _\            / __\ /___\ /__\  /__\
  / /\///  // \  /    / /\/ / /\// /   |/ \ \            / /   //  /// \// /_\  
 / /  / \_//  /  \ /\/ /_/\/ /_ / /___    _\ \          / /___/ \_/// _  \//__  
 \/   \___/  /_/\_\\____/\____/ \____/    \__/          \____/\___/ \/ \_/\__/  
__________________________________________________________________________________

Created by: ToXiiC
Thanks to: CodeDragon, Kill1212, CodeDragon

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Room_Data
{
    internal enum Subtype : int
    {
        Start = 1,
        ServerStart = 4,
        ServerNewRound = 5,
        ServerPrepareNewRound = 6,
        NewRound = 7,
        BackToRoom = 9,
        VoteKickActive = 14,
        ReadyState = 50,
        MapChange = 51,
        ModeChange = 52,
        TimeChange = 54,
        KillLimitDeathmatchChange = 53,
        KillLimitExplosiveChange = 55,
        PingChange = 59,
        VoteKick = 61,
        AutostartChange = 62,
        SwitchTeam = 56,
        UserLimit = 58,

        MagicSubtype1 = 100,
        MagicSubtype2 = 154,

        Heal = 101,
        Damage = 103,
        AmmoRecharge = 105,
        Spawn = 150,
        VehicleSpawn = 151,
        ServerKill = 152,
        WeaponSwitch = 155,
        Flag = 156,
        Suicide = 157,
        TotalWarFlag = 165,
        TotalWarSpawnVehicle = 166,
        CaptureModeResponse = 167, //>--- Estaba 157. Eso es Suicide. Cambio ...
        //>--- probando 
        CaptureModeRequest = 180,
        Place = 400,
        PlaceUse = 401,
        RoomReady = 402,
        ServerRoomReady = 403,
        WorldDamage = 500,
        DeathCam = 800,
        ZombieDropUse = 902,
        ZombieExplode = 900,
        RepairVehicle = 102,
        DamageVehicle = 104,
        VehicleKill = 153,
        JoinVehicle = 200,
        ChangeVehicleSeat = 201,
        LeaveVehicle = 202,
        ArtilleryRequest = 159
    }
}
