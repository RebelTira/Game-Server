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

using Game_Server.Game;
using Game_Server.Managers;
using Game_Server.GameModes;
using System.Threading;

namespace Game_Server.Room_Data
{
    class RoomHandler_Spawn : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            if (!room.gameactive) return;
            if (usr.IsAlive() && room.mode != 1) return; //>---Si no es FFA - Return

            int mode = room.new_mode;
            int selection = room.new_mode_sub;
            Log.WriteInfo(">---Spawn-32 Switch new.mode: " + mode + " new_mode_sub: " + selection);
            #region New Modes
            switch (mode) //>--- NewModes
            {
                case 1:
                    if (selection == 0)
                    {
                        Item item = ItemManager.GetItem("DA02"); //>--- DA_KNUCKLE
                        if (item != null)
                        {
                            usr.weapon = item.ID;
                        }
                    }
                    break;
                case 3:
                    if (selection == 0)
                    {
                        Item item = ItemManager.GetItem("DB01");  //>--- Colt
                        if (item != null)
                        {
                            usr.weapon = item.ID;
                        }
                    }
                    break;
                case 4:
                    if (selection == 0)
                    {
                        Item item = ItemManager.GetItem("DN01");  //>--- DN_K400_GRENADE
                        if (item != null)
                        {
                            usr.weapon = item.ID;
                        }
                    }
                    else if (selection == 1)
                    {
                        Item item = ItemManager.GetItem("D202"); //>--- D2_S_Water_Balloon
                        if (item != null)
                        {
                            usr.weapon = item.ID;
                        }
                    }
                    break;
                case 5:
                    if (selection == 0)
                    {
                        Item item = ItemManager.GetItem("DB25"); //>--- DB_LugerP08
                        if (item != null)
                        {
                            usr.weapon = item.ID;
                        }
                    }
                    else if (selection == 1)
                    {
                        Item item = ItemManager.GetItem("DC74");//>--- DC_Springfield_M1903A3
                        if (item != null)
                        {
                            usr.weapon = item.ID;
                        }
                    }
                    else if (selection == 2)
                    {
                        Item item = ItemManager.GetItem("DG42"); //>--- DG_TOKAREV_SVT40
                        if (item != null)
                        {
                            usr.weapon = item.ID;
                        }
                    }
                    break;
                case 6:
                    if (selection == 1)
                    {
                        Item item = ItemManager.GetItem("DA06"); //>--- SQUEAKY Hammer
                        if (item != null)
                        {
                            usr.weapon = item.ID;
                        }
                    }
                    break;
            }
            #endregion

            usr.Class = int.Parse(getBlock(7)); //>--- Comprueba Class 
            if (usr.Class < 0 || usr.Class > 4)
            {
                /* Invalid spawn branch */

                Log.WriteLine(usr.nickname + " -> Invalid branch at spawn");
                return;
            }

            room.SpawnLocation++;
            Log.WriteInfo("Spawn-126 room.mode: " + room.mode);
            if (room.SpawnLocation >= 15) room.SpawnLocation = 0;
            if (room.mode == 1) //>--- FFA
            {
                sendBlocks[10] = room.SpawnLocation.ToString(); //>--- +++ 
                sendBlocks[11] = room.SpawnLocation.ToString(); //>--- +++ 
                sendBlocks[12] = room.SpawnLocation; //>--- Sitios de spawn en FFA
            }

            /* Snow fight */
            /*if(room.mapid == 72 && room.new_mode == 6 && room.new_mode_sub == 2) //>--- Pargona_Dogfight ???
            {
                usr.weapon = 122; //>--- D2_SNOWBALL D201 SoundIdx = 122
            }*/
            /* Waterland */
            //if (room.mapid == 90) { usr.weapon = 550; } //>--- Waterland. Añado esto pero no va.  DJ_WaterBazooka: DJ27 - SoundIdx = 550
            Log.WriteInfo("Spawn-141 usr.Class: " + usr.Class + " SpawnLocation: " + room.SpawnLocation);
            if (room.channel == 3) // >--- Zombies
            {
                room.SpawnedZombieplayers++;
                
                Log.WriteInfo(">---Spawn-146 SpawnedZombieplayers: " + room.SpawnedZombieplayers + " room.users.Count: " + room.users.Count + " SendFirstWave: " + room.SendFirstWave + " FirstWaveSent: " + room.FirstWaveSent); 
                if (room.SpawnedZombieplayers >= room.users.Count && !room.FirstWaveSent) //>--- Se pone true al empezar partida
                {
                    Log.WriteInfo(">---Spawn-156  --- usr.Alive = true; //>--- +++ " );
                    usr.Alive = true; //>--- +++
                    //room.SendFirstWave = true;
                    //Log.WriteInfo("Spawn-151 SendFirstWave: " + room.SendFirstWave);
                    //>--- +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape 

                    if (room.mode == 13)
                    {

                       room.send(new PACKET_1()); //>--- NO-VA 31507 0 12 1 6 0 0 -1 0
                        Thread.Sleep(1000);
                        for (int n = 0; n < 9; n++)
                        {
                            Thread.Sleep(1000);
                            room.send(new PACKET_2(n)); //>--- 31507 0 12 1 5 0 0 -1 0
                        }

                        //room.send(new PACKET_3(2, room)); //>--- NO-VA 31507 0 0 room blck3 0 0 -1 0

                        //>--- +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape +++ Escape 
                    }
                   

                    if (room.mode == 12 )
                    {
                        if (room.zombiedifficulty == 0)
                        {
                            room.send(new PACKET_TIMEATTACK_ALL(30053, 0, 0, 5, 5));
                        }
                        else
                        {
                            //285778092 30053 0 1 4 3 
                            room.send(new PACKET_TIMEATTACK_ALL(30053, 0, 1, 4, 3)); //>--- +++
                        }

                    }
                }

                //room.SendFirstWave = true;
                room.zombieRunning = true;

                if (room.zombie != null)
                {
                    room.SleepTime = 15; //>--- *** 15
                }
            }
            room.zombieRunning = true; //>+++ 
            room.SendFirstWave = true;
            usr.classCode = getBlock(27);
            usr.Health = 1000;
            usr.Plantings = usr.skillPoints = 0;
            usr.spawnprotection = 3;
            usr.currentVehicle = null;
            usr.HPLossTick = 0;
            usr.rKillSinceSpawn = 0;
            usr.throwNades = usr.throwRockets = 0;
            usr.currentSeat = null;
            if (room.mapid == 71 || room.mapid == 72) usr.spawnprotection = 10; //>---  Crater_Dogfight - Pargona_Dogfight
            room.firstspawn = true;
            usr.ExplosiveAlive = true;
            usr.isSpawned = true;
            usr.Alive = true; //>--- +++
            /* Important */

            sendPacket = true;

        }
    }
}
