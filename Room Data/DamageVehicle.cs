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

using Game_Server.Managers;
using Game_Server.Game;

namespace Game_Server.Room_Data
{
    class RoomHandler_DamageVehicle : RoomDataHandler
    {
        public bool isZombieWeapon(string Weapon)
        {
            if (Weapon == "DA50" || Weapon == "DA51" || Weapon == "DA52" || Weapon == "DA53" || Weapon == "DA54" || Weapon == "DN51" || Weapon == "DN52")
                return true;
            return false;
        }

        public override void Handle(User usr, Room room)
        {
            // 27 = NX / 26 = G1
            if (!room.gameactive || room.sleep && room.bombDefused || sendBlocks.Length < 27) return;
            if (room.firstInGameTS > Generic.timestamp) { usr.disconnect(); return; }
            if (!room.gameactive || usr.Health <= 0) return;

            if (room.Vehicles.Count == 0)
            {
                Log.WriteError("No vehicles for map " + room.mapid);
                return;
            }

            int shoot_type = int.Parse(getBlock(6)); // 0 = Veh / 1 = Player
            int vehicleId = int.Parse(getBlock(7));
            int slotId = int.Parse(getBlock(8));
            int Damage = 0;
            int points = Configs.Server.Experience.OnVehicleKill;
            string weapon = getBlock(27).Substring(0, 4);
            int HSCalculation = int.Parse(getBlock(15));

            bool useRadius = getBlock(14) == "1";

            if (vehicleId < 0 || vehicleId > room.Vehicles.Count) return;

            Vehicle vehicle = room.GetVehicleByID(vehicleId);

            if (vehicle.Side == room.GetSide(usr) || vehicle.SpawnProtection > 0 || vehicle.Health <= 0 || vehicle == null) return;

            int damageType = shoot_type == 1 ? 0 : 1; //>--- 0=Vehicle 1=player ???
            
            // TO DO
            //if (usr.currentVehicle == null && shoot_type == 0 || usr.currentVehicle != null && shoot_type == 1) return;

            if (usr.currentVehicle != null)
            {
                if (!usr.currentVehicle.IsRightVehicle(weapon) && usr.channel != 3) return;
                bool MainCT = int.Parse(getBlock(13)) == 0;
                if (MainCT)
                {
                    Damage = ItemManager.GetVehicleDamage(usr.currentSeat.MainCTCode, shoot_type);
                }
                else
                {
                    Damage = ItemManager.GetVehicleDamage(usr.currentSeat.SubCTCode, shoot_type);
                }

                if (room.channel == 3 && room.mode == (int)RoomMode.Defense && room.GetIncubatorVehicleId() == vehicleId)
                {
                    if (!isZombieWeapon(weapon)) return; // >--- Solo los zombies dañan al incubator

                    Damage = 150 * (room.zombiedifficulty + 1);
                }
            }
            else //>--- No entiendo esto. Ver diferencia entre NULL y no NULL
            {
                Damage = ItemManager.GetVehicleDamage(weapon, shoot_type);
                if (room.channel == 3 && room.mode == (int)RoomMode.Defense && room.GetIncubatorVehicleId() == vehicleId)
                {
                    if (!isZombieWeapon(weapon)) return; // >--- Solo los zombies dañan al incubator

                    Damage = 100 * (room.zombiedifficulty + 1);
                }
            }
            Log.WriteInfo("DamageVehicles-95   Code: " + vehicle.Code + " - vehicleId: " + vehicleId + " - shoot_type: " + shoot_type + " - vehicle.Health: " + vehicle.Health);
            if (damageType == 1) //>--- Si dispara un vehiculo...
            {
                if (HSCalculation > 0 && HSCalculation < 100 && useRadius)
                {
                    // Calculate range and remove the percentage from the damage
                    Damage = (int)Math.Ceiling((double)(Damage * HSCalculation) / 100);
                }
            }
            
            if (vehicle.Code == "EN01")  //>---  FIXED_BRK_DRUM - Bidones
            {
                Damage = 250;
            }
            else if (vehicle.Code == "EJ05" && weapon.StartsWith("DK"))  //>---  EJ05: HEL_500MD 
            {
                Damage = 800;
            }
            
            if (usr.GodMode && slotId < 4) { Damage = 10000; }  //>---  Para testear mas rapido +++ (Los zombies de Defense, NO :)
            vehicle.Health -= Damage;

            if (room.mode == 12 && vehicle.Code == "EN17")  //>---  FIXED_BRK_PRISON_GATE
            {
                    usr.timeattackDamagedDoor++;
                }

            sendBlocks[3] = (int)Subtype.DamageVehicle;
            sendBlocks[8] = usr.roomslot;
            sendBlocks[16] = vehicle.Health;
            sendBlocks[17] = (vehicle.Health + Damage);
            sendBlocks[27] = weapon;

            //27 = Nexon - 26 = Older clients <- Weapon

            if (vehicle.Health <= 0)
            {
                if (room.mode == 12 && vehicle.Code == "EN17")  //>---  FIXED_BRK_PRISON_GATE
                {
                    usr.room.Destructed = true;
                }
                room.CheckForMission(usr, vehicleId);

                if (vehicle.Players.Count > 0)
                {
                    object[] tempPacket = sendBlocks;
                    tempPacket[3] = (int)Subtype.ServerKill; // Server Kill Subtype

                    foreach (VehicleSeat seat in vehicle.Seats.Values)
                    {
                        if (seat.seatOwner != null && seat.seatOwner.currentVehicle == vehicle)
                        {
                            if (!room.firstblood)
                            {
                                room.firstblood = true;
                                byte[] buffer = (new SP_CustomSound(SP_CustomSound.Sounds.FirstBlood)).GetBytes();
                                foreach (User u in room.users.Values.Where(r => r.mapLoaded))
                                {
                                    u.sendBuffer(buffer);
                                }
                            }
                            if (seat.seatOwner.Health > 0)
                            {
                                int roomslot = seat.seatOwner.roomslot;

                                seat.seatOwner.OnDie();
                                usr.rPoints += points;
                                usr.rKills++;

                                if (room.mode == 8)
                                {
                                    usr.TotalWarPoint += 5;
                                    seat.seatOwner.TotalWarSupport += 2;
                                }

                                tempPacket[7] = roomslot;
                                tempPacket[19] = (room.mode == 8 ? usr.TotalWarPoint : tempPacket[19]);
                                tempPacket[20] = (room.mode == 8 ? usr.TotalWarPoint : tempPacket[20]);
                                tempPacket[21] = points + ".00";
                                tempPacket[22] = "2.000";
                                tempPacket[27] = weapon;
                                room.send(new SP_RoomData(tempPacket));
                            }
                        }
                    }
                }

                if (vehicle.Players.Count > 0)
                {
                    if (room.mode == 8)
                    {
                        switch (room.GetSide(usr))
                        {
                            case 0: room.TotalWarDerb += 5; room.TotalWarNIU += 2; break;
                            case 1: room.TotalWarNIU += 5; room.TotalWarDerb += 2; break;
                        }
                    }
                }

                room.updateTime();
                room.send(new SP_RoomVehicleExplode(room.id, vehicle.ID, usr.roomslot));

                if ((vehicleId == 25 || vehicleId == 24 || vehicleId == 23) && room.mapid == 42 && room.mode == 5)
                {
                    room.EndGame();
                }
                else if (vehicleId == 0 && room.mapid == 60 && room.mode == 5) //>--- cambio 56 x 60 SiegeWar2
                {
                    room.EndGame();
                }
                if (room.explosive != null)
                {
                    room.explosive.CheckForNewRound();
                }
                else if (room.heromode != null)
                {
                    room.heromode.CheckForNewRound();
                }
                return;
            }

            /* Important */
            sendPacket = true;
        }
    }
}