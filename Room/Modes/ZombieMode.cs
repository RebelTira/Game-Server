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

using Game_Server.Game;
using Game_Server.Room_Data;
using System;
using Game_Server.GameModes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.GameModes
{
    class ZombieMode
    {
        ~ZombieMode()
        {
            GC.Collect();
        }
        private Room room = null;
        public bool PreparingWave = false;
        public bool LastWave = false;
        public bool respawnThisWave = false;
        public int Wave = 0;
        public int ZombiePoints = 0;
        public int ZombieToWave = 0;

        public void PrepareNewWave() 
        {
            PreparingWave = true; //>--- Comienzan los preparativos...

            room.spawnedMadmans = room.spawnedManiacs = room.spawnedGrinders = room.spawnedGrounders = room.spawnedHeavys = room.spawnedGrowlers = 0;
            room.spawnedLovers = room.spawnedHandgemans = room.spawnedEnvys = room.spawnedClaws = room.spawnedMadSoldiers = room.spawnedMadPrisoners = room.spawnedLadys = room.spawnedMidgets = 0;
            room.spawnedChariots = room.spawnedCrushers = room.spawnedBusters = room.spawnedCrashers = room.spawnedBombers = room.spawnedDefenders = room.spawnedBreaker2s = room.spawnedSuperHeavys = 0;
            room.spawnedztipo0 = room.spawnedztipo1 = room.spawnedztipo2 = room.spawnedztipo3 = 0;
            room.KilledZombies = room.ZombieSpawnPlace = room.KillsBeforeDrop = room.SpawnedZombies = 0;

            room.SleepTime = 15;  //>--- *** 15


            room.RespawnAllVehicles();
            respawnThisWave = false;
            Log.WriteInfo(">---Z.Mode-53 _ suicidio Zs. para evitar que quede alguno vivo entre rondas");
            
            for (int i = 4; i < 32; i++)
            {
                Zombie zombie = room.GetZombieByID(i);
                room.send(new SP_EntitySuicide(i));
                zombie.Health = 0;
                zombie.respawn = Generic.timestamp + 4;
            }
            
            room.send(new Game.SP_ZombieNewWave(0));
            Log.WriteInfo(">---Z.Mode-65 _ Wave: " + Wave );
           
            
            room.send(new Game.SP_ZombieNewWave(Wave, LastWave));
            Wave++;
            Log.WriteInfo(">---Z.Mode-67 _ SP_ZombieNewWave: " + Wave + " LastWave: " + LastWave);
        }
        public int LastTick = 0;
        
        public void Zombie() //>--- De Update . Desde Room Switch Mode
        {

            try
            {
                if (LastTick != DateTime.Now.Second)
                {
                    LastTick = DateTime.Now.Second;
                    Log.WriteInfo(">---Z.Mode-81 _ SendFirstWave:  " + room.SendFirstWave);
                    if (room.SendFirstWave) //>--- Solo una vez
                    {
                      
                        room.FirstWaveSent = true;
                        room.send(new Game.SP_ZombieNewWave(0));
                        
                        Log.WriteInfo(">---Z.Mode-85 _ PrepareNewWave --- SOLO UNA VEZ " );
                        PrepareNewWave(); //>--- +++ para que salgan los Zs en la segunda partida sin salir del mapa
                        room.SendFirstWave = false; //>-- Para que no se repita
                        
                        switch (room.mapid) //>--- Para que salgan diferentes Zs segun el mapa
                        {
                            case 48: //>--- DayOne
                                room.ztipo3 = 13; break; //>--- Claw
                            case 49: //>--- OutPost
                                {
                                    room.ztipo0 = 18; //>--- MadPrisoner
                                    room.ztipo1 = 17; //>--- MadSoldier
                                    break;
                                }
                            case 51: //>--- Barrio
                                room.ztipo2 = 12; break; //>--- Envy

                            default:
                                {
                                    room.ztipo0 = 0;  //>---  Madman
                                    room.ztipo1 = 1;  //>---  Maniac
                                    room.ztipo2 = 6;  //>---  Lover
                                    room.ztipo3 = 7;  //>---  Handgeman
                                    break;
                                }
                        }
                    }
                    
                    if (room.zombieRunning)
                    {
                        //ZombieToWave = 5;
                        //if (Wave == 22) LastWave = true;
                        //Log.WriteInfo(">---Z.Mode-115 _ zombieRunning - Wave: " + Wave);
                        
                        switch (Wave)
                        {
                            case 1: ZombieToWave = 20; break; // Wave 1
                            case 2: ZombieToWave = 27; break; // Wave 2
                            case 3: ZombieToWave = 35; break; // Wave 3
                            case 4: ZombieToWave = 43; break; // Wave 4
                            case 5: ZombieToWave = 52; break; // Wave 5
                            case 6: ZombieToWave = 65; break; // Wave 6
                            case 7: ZombieToWave = 72; break; // Wave 7
                            case 8: ZombieToWave = 83; break; // Wave 8
                            case 9: ZombieToWave = 92; break; // Wave 9
                            case 10: ZombieToWave = 100; break; // Wave 10
                            case 11: ZombieToWave = 101; break; // Wave 11
                            case 12: ZombieToWave = 107; break; // Wave 12
                            case 13: ZombieToWave = 115; break; // Wave 13
                            case 14: ZombieToWave = 117; break; // Wave 14
                            case 15: ZombieToWave = 125; break; // Wave 15
                            case 16: ZombieToWave = 128; break; // Wave 16
                            case 17: ZombieToWave = 139; break; // Wave 17
                            case 18: ZombieToWave = 144;
                                if (room.zombiedifficulty == 0)
                                { LastWave = true; } break; // Wave 18
                               
                            case 19: ZombieToWave = 153; break; // Wave 19
                            case 20: ZombieToWave = 169; break; // Wave 20
                            case 21: ZombieToWave = 192;
                                if (room.zombiedifficulty == 1) 
                                { LastWave = true; } break; // Wave 21

                        }
                        
                        if ((Wave > 0) && (room.zombiedifficulty > 0)) ZombieToWave += 5; //>--- Estaba a 15

                        if (room.AliveDerb == 0) { room.EndGame(); }
                        Log.WriteInfo(">---ZMode-150 Wave: " + Wave + "*************** ZombieToWave: " + ZombieToWave + "------------------ SpawnedZombies: " + room.SpawnedZombies + "---------------- KilledZombies: " + room.KilledZombies );
                        if (room.KilledZombies >= ZombieToWave) 
                        {
                            if (LastWave)
                            {
                                room.EndGame();
                            }
                            else
                            {
                                //Log.WriteInfo(">---Z.Mode-160 PrepareNewWave()");
                                PrepareNewWave();
                            }
                            return;
                        }
                        
                        if (room.SleepTime >= 0)
                        {
                            room.SleepTime--;
                            Log.WriteInfo(">---Z.Mode-172 _ SleepTime: " + room.SleepTime);
                            return;
                        }
                        Log.WriteInfo(">---Z.Mode-175 _ PreparingWave " + PreparingWave + " SpawnedZombies: " + room.SpawnedZombies + " ZombieToWave: " + ZombieToWave);
                        if (PreparingWave) PreparingWave = false;

                        
                        Log.WriteInfo(">---Z.Mode-183 _ Zs vivos " + room.Zombies.Where(r => r.Value.Health > 0).Count());
                        Log.WriteInfo(">---Z.Mode-184 _ room.SpawnedZombies " + room.SpawnedZombies + " ZombieToWave: " + ZombieToWave);
                        if (room.Zombies.Where(r => r.Value.Health > 0).Count() >= 20 || room.SpawnedZombies >= ZombieToWave) 
                        {
                            return; 
                        }

                        if (!PreparingWave) //>--- Si se han acabado los prerparativos... 
                        {
                            Log.WriteInfo(">---Z.Mode-192 _ switch Wave " + Wave);

                            
                            switch (Wave)
                            {

                                case 1:
                                    {
                                       
                                        //>--- ZombieToWave = 20
                                        if (room.spawnedztipo0 < 20) room.SpawnZombie(room.ztipo0);
                                        
                                        break;
                                    }
                                case 2:
                                    {
                                       
                                        //>--- ZombieToWave = 27 
                                        if (room.spawnedztipo0 < 20) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 7) room.SpawnZombie(room.ztipo1);
                                        

                                        break;
                                    }
                                case 3:
                                    {

                                        //>--- ZombieToWave = 35
                                        if (room.spawnedztipo2 < 1 && room.KilledZombies >= 20) room.SpawnZombie(room.ztipo2);//>--- Lover
                                        if (room.spawnedztipo3 < 1 && room.KilledZombies >= 25) room.SpawnZombie(room.ztipo3); //>--- Handgeman
                                        if (room.spawnedztipo0 < 20) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 7) room.SpawnZombie(room.ztipo1);
                                        if (room.spawnedGrinders < 6 && room.KilledZombies >= 20) room.SpawnZombie(2);
                                        
                                        break;
                                    }
                                case 4:
                                    {
                                        
                                        //>--- ZombieToWave = 43
                                        if (room.spawnedztipo0 < 10) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 20) room.SpawnZombie(room.ztipo1);
                                        if (room.spawnedGrinders < 8 && room.KilledZombies >= 20) room.SpawnZombie(2);
                                        if (room.spawnedztipo2 < 2 && room.KilledZombies >= 25) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 1 && room.KilledZombies >= 30) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 2 && room.KilledZombies >= 35) room.SpawnZombie(5);
                                       
                                        break;
                                    }
                                case 5:
                                    {
                                        
                                        //>--- ZombieToWave = 52
                                        if (room.spawnedztipo0 < 21) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 10) room.SpawnZombie(room.ztipo1);
                                        if (room.spawnedGrinders < 10 && room.KilledZombies >= 20) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 4 && room.KilledZombies >= 30) room.SpawnZombie(4);
                                        if (room.spawnedztipo2 < 3 && room.KilledZombies >= 30) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 2 && room.KilledZombies >= 50) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 2 && room.KilledZombies >= 30) room.SpawnZombie(5);
                                        break;
                                    }
                                case 6:
                                    {
                                        //>--- ZombieToWave = 65 
                                        if (room.spawnedztipo0 < 22) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 12) room.SpawnZombie(room.ztipo1);
                                        if (room.spawnedGrounders < 3) room.SpawnZombie(3);
                                        if (room.spawnedGrinders < 15 && room.KilledZombies >= 20) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 5 && room.KilledZombies >= 30) room.SpawnZombie(4);
                                        if (room.spawnedztipo2 < 3 && room.KilledZombies >= 35) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 2 && room.KilledZombies >= 50) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 3 && room.KilledZombies >= 40) room.SpawnZombie(5);
                                        break;
                                    }
                                case 7:
                                    {
                                        //>--- ZombieToWave = 72
                                        if (room.spawnedztipo0 < 22) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 12) room.SpawnZombie(room.ztipo1);
                                        if (room.spawnedGrounders < 4) room.SpawnZombie(3);
                                        if (room.spawnedGrinders < 18 && room.KilledZombies >= 20) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 6 && room.KilledZombies >= 30) room.SpawnZombie(4);
                                        if (room.spawnedztipo2 < 3 && room.KilledZombies >= 50) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 2 && room.KilledZombies >= 60) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 4 && room.KilledZombies >= 40) room.SpawnZombie(5);
                                        if (room.spawnedChariots < 1 && room.KilledZombies >= 60) room.SpawnZombie(8); 
                                        break;
                                    }
                                    
                                case 8:
                                    {
                                        //>--- ZombieToWave = 83
                                        if (room.spawnedztipo0 < 25) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 15) room.SpawnZombie(room.ztipo1);
                                        if (room.spawnedGrinders < 20) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 5) room.SpawnZombie(3);
                                        if (room.spawnedGrowlers < 8 && room.KilledZombies >= 40) room.SpawnZombie(4);
                                        if (room.spawnedztipo2 < 3 && room.KilledZombies >= 40) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 3 && room.KilledZombies >= 60) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 4 && room.KilledZombies >= 70) room.SpawnZombie(5);
                                        break;
                                    }
                                case 9:
                                    {
                                        //>--- ZombieToWave = 92
                                        if (room.spawnedztipo0 < 25) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 15) room.SpawnZombie(room.ztipo1);

                                        if (room.spawnedGrinders < 23) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 6) room.SpawnZombie(3);
                                        if (room.spawnedGrowlers < 8 && room.KilledZombies >= 70) room.SpawnZombie(4);
                                        if (room.spawnedztipo2 < 4 && room.KilledZombies >= 30) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 4 && room.KilledZombies >= 60) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 7 && room.KilledZombies >= 50) room.SpawnZombie(5);
                                        break;
                                    }
                                case 10:
                                    {
                                        //>--- ZombieToWave = 100
                                        if (room.spawnedztipo0 < 25) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 20) room.SpawnZombie(room.ztipo1);

                                        if (room.spawnedGrinders < 12) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 4) room.SpawnZombie(4);

                                        if (room.spawnedGrounders < 6 && room.KilledZombies >= 50) room.SpawnZombie(3);
                                        if (room.spawnedztipo2 < 4 && room.KilledZombies >= 60) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 4 && room.KilledZombies >= 70) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 7 && room.KilledZombies >= 50) room.SpawnZombie(5);
                                        if (room.spawnedChariots < 2 && room.KilledZombies >= 60) room.SpawnZombie(8);  //>---  > 40 
                                        if (room.spawnedGrinders < 24 && room.KilledZombies > 80) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 8 && room.KilledZombies > 90) room.SpawnZombie(4);
                                        break;
                                    }
                                case 11:
                                    {
                                        //>--- ZombieToWave = 101
                                        if (room.spawnedztipo0 < 25) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 20) room.SpawnZombie(room.ztipo1);

                                        if (room.spawnedGrinders < 12) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 4) room.SpawnZombie(4);
                                        if (room.spawnedGrounders < 4) room.SpawnZombie(3);

                                        if (room.spawnedGrounders < 7 && room.KilledZombies >= 50) room.SpawnZombie(3);
                                        if (room.spawnedztipo2 < 5 && room.KilledZombies >= 60) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 4 && room.KilledZombies >= 70) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 8 && room.KilledZombies >= 50) room.SpawnZombie(5);
                                        if (room.spawnedGrinders < 24 && room.KilledZombies > 80) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 8 && room.KilledZombies > 90) room.SpawnZombie(4);
                                        break;
                                    }
                                case 12:
                                    {
                                        //>--- ZombieToWave = 107
                                        if (room.spawnedztipo0 < 25) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 20) room.SpawnZombie(room.ztipo1);

                                        if (room.spawnedGrinders < 13) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 5) room.SpawnZombie(4);
                                        if (room.spawnedGrounders < 3) room.SpawnZombie(3);

                                        if (room.spawnedGrounders < 7 && room.KilledZombies >= 60) room.SpawnZombie(3);
                                        if (room.spawnedztipo2 < 5 && room.KilledZombies >= 75) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 5 && room.KilledZombies >= 80) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 8 && room.KilledZombies >= 60) room.SpawnZombie(5);
                                        if (room.spawnedChariots < 1 && room.KilledZombies >= 70) room.SpawnZombie(8); 
                                        if (room.spawnedCrushers < 1 && room.KilledZombies >= 85) room.SpawnZombie(9);
                                        if (room.spawnedGrinders < 25 && room.KilledZombies > 60) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 10 && room.KilledZombies > 70) room.SpawnZombie(4);
                                        break;
                                    }
                                
                                case 13:
                                    {
                                        //>--- ZombieToWave = 115
                                        if (room.spawnedztipo0 < 25) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 25) room.SpawnZombie(room.ztipo1);

                                        if (room.spawnedGrinders < 13) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 6) room.SpawnZombie(4);
                                        if (room.spawnedGrounders < 4) room.SpawnZombie(3);

                                        if (room.spawnedGrounders < 8 && room.KilledZombies >= 60) room.SpawnZombie(3);
                                        if (room.spawnedztipo2 < 5 && room.KilledZombies >= 60) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 5 && room.KilledZombies >= 40) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 8 && room.KilledZombies >= 60) room.SpawnZombie(5);
                                        if (room.spawnedChariots < 1 && room.KilledZombies >= 70) room.SpawnZombie(8);
                                        if (room.spawnedChariots < 2 && room.KilledZombies >= 80) room.SpawnZombie(8);
                                        if (room.spawnedGrinders < 26 && room.KilledZombies > 80) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 10 && room.KilledZombies > 70) room.SpawnZombie(4);  //>---  -1
                                        break;
                                    }  
                                case 14:
                                    {
                                        //>--- ZombieToWave = 117
                                        if (room.spawnedztipo0 < 25) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 25) room.SpawnZombie(room.ztipo1);

                                        if (room.spawnedGrinders < 13) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 7) room.SpawnZombie(4);
                                        if (room.spawnedGrounders < 4) room.SpawnZombie(3);

                                        if (room.spawnedGrounders < 9 && room.KilledZombies >= 50) room.SpawnZombie(3);
                                        if (room.spawnedztipo2 < 5 && room.KilledZombies >= 60) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 5 && room.KilledZombies >= 80) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 8 && room.KilledZombies >= 70) room.SpawnZombie(5);
                                        if (room.spawnedCrushers < 1 && room.KilledZombies >= 80) room.SpawnZombie(9);
                                        if (room.spawnedGrinders < 26 && room.KilledZombies > 90) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 13 && room.KilledZombies > 70) room.SpawnZombie(4);
                                        break;
                                    }
                                case 15:
                                    {
                                        //>--- ZombieToWave = 125 += 5
                                        if (room.spawnedztipo0 < 30) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 25) room.SpawnZombie(room.ztipo1);

                                        if (room.spawnedztipo2 < 3) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 3) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedGrinders < 14) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 6) room.SpawnZombie(4);
                                        if (room.spawnedGrounders < 5) room.SpawnZombie(3);

                                        if (room.spawnedGrounders < 10 && room.KilledZombies >= 80) room.SpawnZombie(3);
                                        if (room.spawnedztipo2 < 6 && room.KilledZombies >= 50) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 6 && room.KilledZombies >= 60) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 9 && room.KilledZombies >= 80) room.SpawnZombie(5);
                                        if (room.spawnedGrinders < 27 && room.KilledZombies > 75) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 12 && room.KilledZombies > 80) room.SpawnZombie(4);
                                        break;
                                    }
                                case 16:
                                    {
                                        //>--- ZombieToWave = 128 += 5
                                        if (room.spawnedztipo0 < 25) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 30) room.SpawnZombie(room.ztipo1);

                                        if (room.spawnedztipo2 < 4) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 3) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedGrinders < 14) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 6) room.SpawnZombie(4);
                                        if (room.spawnedGrounders < 5) room.SpawnZombie(3);

                                        if (room.spawnedGrounders < 10 && room.KilledZombies >= 80) room.SpawnZombie(3);
                                        if (room.spawnedztipo2 < 7 && room.KilledZombies >= 50) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 6 && room.KilledZombies >= 60) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 10 && room.KilledZombies >= 80) room.SpawnZombie(5);
                                        if (room.spawnedChariots < 1 && room.KilledZombies >= 75) room.SpawnZombie(8);
                                        if (room.spawnedGrinders < 27 && room.KilledZombies > 90) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 12 && room.KilledZombies > 100) room.SpawnZombie(4);

                                        foreach (User Player in room.users.Values)
                                        {
                                            if (room.WeaponsGot == false)
                                            {
                                                room.send(new SP_Chat("EVENT", SP_Chat.ChatType.Room_ToAll, "EVENT >> You got CLAW_KNIFE (30 DAYS) because you survived wave 16!!", 999, "NULL"));
                                                Inventory.AddOutBoxItem(Player, "DA13", 7, 1); //>--- DA13 = DA_Claw_knife
                                                room.WeaponsGot = true;
                                            }
                                        }
                                        break;
                                    }
                                case 17:

                                    {
                                        //>--- ZombieToWave = 139 += 5
                                        if (room.spawnedztipo0 < 30) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 30) room.SpawnZombie(room.ztipo1);

                                        if (room.spawnedztipo2 < 5) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 3) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedGrinders < 14) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 7) room.SpawnZombie(4);
                                        if (room.spawnedGrounders < 6) room.SpawnZombie(3);
                                        if (room.spawnedChariots < 1) room.SpawnZombie(8);

                                        if (room.spawnedGrounders < 11 && room.KilledZombies >= 50) room.SpawnZombie(3);
                                        if (room.spawnedztipo2 < 8 && room.KilledZombies >= 60) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 6 && room.KilledZombies >= 70) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 10 && room.KilledZombies >= 80) room.SpawnZombie(5);
                                        if (room.spawnedChariots < 2 && room.KilledZombies >= 90) room.SpawnZombie(8);
                                        if (room.spawnedCrushers < 1 && room.KilledZombies >= 100) room.SpawnZombie(9);
                                        if (room.spawnedGrinders < 28 && room.KilledZombies > 90) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 13 && room.KilledZombies > 100) room.SpawnZombie(4);
                                            break;
                                    }
                                case 18:
                                    {
                                        //>--- ZombieToWave = 144 += 5
                                        if (room.spawnedztipo0 < 30) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 30) room.SpawnZombie(room.ztipo1);

                                        if (room.spawnedGrinders < 15) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 8) room.SpawnZombie(4);
                                        if (room.spawnedztipo2 < 4) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 4) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 6) room.SpawnZombie(5);
                                        if (room.spawnedGrounders < 7) room.SpawnZombie(3);

                                        if (room.spawnedGrounders < 13 && room.KilledZombies >= 50) room.SpawnZombie(3);
                                        if (room.spawnedztipo2 < 8 && room.KilledZombies >= 80) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 7 && room.KilledZombies >= 90) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 11 && room.KilledZombies >= 95) room.SpawnZombie(5);
                                        if (room.spawnedGrinders < 30 && room.KilledZombies > 100) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 15 && room.KilledZombies > 100) room.SpawnZombie(4);
                                        break;
                                    }
                                case 19:
                                    {
                                        //>--- ZombieToWave = 153 += 5
                                        if (room.spawnedztipo0 < 30) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 35) room.SpawnZombie(room.ztipo1);
                                        if (room.spawnedChariots < 1) room.SpawnZombie(8);
                                        if (room.spawnedCrushers < 1) room.SpawnZombie(9);

                                        if (room.spawnedGrinders < 15) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 8) room.SpawnZombie(4);
                                        if (room.spawnedztipo2 < 5) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 4) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 6) room.SpawnZombie(5);    
                                        if (room.spawnedGrounders < 7) room.SpawnZombie(3);
                                        
                                        if (room.spawnedGrounders < 13 && room.KilledZombies >= 70) room.SpawnZombie(3);
                                        if (room.spawnedztipo2 < 9 && room.KilledZombies >= 80) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 7 && room.KilledZombies >= 90) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 12 && room.KilledZombies >= 100) room.SpawnZombie(5);
                                        if (room.spawnedGrinders < 30 && room.KilledZombies > 90) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 15 && room.KilledZombies > 80) room.SpawnZombie(4);
                                        break;
                                    }
                                case 20:
                                    {
                                        //>--- ZombieToWave = 169 += 5
                                        if (room.spawnedztipo0 < 35) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedChariots < 2) room.SpawnZombie(8);
                                        if (room.spawnedztipo1 < 35) room.SpawnZombie(room.ztipo1);

                                        if (room.spawnedGrinders < 16) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 8) room.SpawnZombie(4);
                                        if (room.spawnedztipo2 < 6) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 4) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 6) room.SpawnZombie(5);
                                        if (room.spawnedGrounders < 7) room.SpawnZombie(3);

                                        if (room.spawnedGrounders < 14 && room.KilledZombies >= 80) room.SpawnZombie(3);
                                        if (room.spawnedztipo2 < 12 && room.KilledZombies >= 90) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 9 && room.KilledZombies >= 100) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 12 && room.KilledZombies >= 90) room.SpawnZombie(5);
                                        if (room.spawnedChariots < 4 && room.KilledZombies >= 100) room.SpawnZombie(8);
                                        if (room.spawnedCrushers < 1 && room.KilledZombies >= 110) room.SpawnZombie(9);
                                        if (room.spawnedGrinders < 32 && room.KilledZombies > 120) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 16 && room.KilledZombies > 130) room.SpawnZombie(4);
                                        break;
                                    }
                                case 21:
                                    {
                                        //>--- ZombieToWave = 192 += 5
                                        if (room.spawnedztipo0 < 40) room.SpawnZombie(room.ztipo0);
                                        if (room.spawnedztipo1 < 40) room.SpawnZombie(room.ztipo1);
                                        if (room.spawnedChariots < 2) room.SpawnZombie(8);

                                        if (room.spawnedGrinders < 17) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 9) room.SpawnZombie(4);
                                        if (room.spawnedztipo2 < 7) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 5) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 8) room.SpawnZombie(5);
                                        if (room.spawnedGrounders < 8) room.SpawnZombie(3);
                                        if (room.spawnedCrushers < 1) room.SpawnZombie(9);

                                        if (room.spawnedGrounders < 15 && room.KilledZombies >= 90) room.SpawnZombie(3);
                                        if (room.spawnedztipo2 < 14 && room.KilledZombies >= 100) room.SpawnZombie(room.ztipo2);
                                        if (room.spawnedztipo3 < 10 && room.KilledZombies >= 120) room.SpawnZombie(room.ztipo3);
                                        if (room.spawnedHeavys < 15 && room.KilledZombies >= 90) room.SpawnZombie(5);
                                        if (room.spawnedChariots < 4 && room.KilledZombies >= 120) room.SpawnZombie(8);
                                        if (room.spawnedCrushers < 2 && room.KilledZombies >= 140) room.SpawnZombie(9);
                                        if (room.spawnedGrinders < 34 && room.KilledZombies > 120) room.SpawnZombie(2);
                                        if (room.spawnedGrowlers < 18 && room.KilledZombies > 75) room.SpawnZombie(4);

                                        foreach (User Player in room.users.Values)
                                        {
                                            if (room.WeaponsGot == false)
                                            {
                                                room.send(new SP_Chat("EVENT", SP_Chat.ChatType.Room_ToAll, "EVENT >> You got CLAW_KNIFE (30 DAYS) because you survived wave 16!!", 999, "NULL"));
                                                Inventory.AddOutBoxItem(Player, "D901", 7, 1);  //>--- D901 = D9_M249_8TH
                                                room.WeaponsGot = true;
                                            }
                                        }
                                        break;
                                    }
                                    
                            }
                            Log.WriteInfo(">---Z.Mode-586 _ spawnedztipo0 " + room.spawnedztipo0 + " spawnedztipo1: " + room.spawnedztipo1 + " spawnedztipo2: " + room.spawnedztipo2 + " spawnedztipo3: " + room.spawnedztipo3);
                            Log.WriteInfo(">---Z.Mode-587 _ spawnedHeavys " + room.spawnedHeavys + " spawnedChariots: " + room.spawnedChariots + " spawnedLadys: " + room.spawnedLadys + " spawnedMidgets: " + room.spawnedMidgets + " spawnedCrushers: " + room.spawnedCrushers);

                            //>--- +++
                            if (room.zombiedifficulty > 0)
                            {
                                //>--- Por ejemplo... 
                                if (Wave < 5)
                                {
                                    Log.WriteInfo(">---Z.Mode-571 _ (Wave <= 4) spawnedLadys: " + room.spawnedLadys + " KilledZombies: " + room.KilledZombies);
                                    if (room.spawnedLadys <= 4 && (room.KilledZombies > (10 + Wave)))
                                        room.SpawnZombie(21); //>--- Lady
                                }
                                else
                                {
                                    Log.WriteInfo(">---Z.Mode-577 _ (Wave >= 5 ) spawnedLadys: " + room.spawnedLadys + " spawnedMidgets: " + room.spawnedMidgets + " KilledZombies: " + room.KilledZombies);
                                    Random rnd = new Random();
                                    int rndz = rnd.Next(0, 2);
                                    if (room.KilledZombies > (35 + Wave))
                                        if ((room.spawnedLadys + room.spawnedMidgets) < 5)
                                        {
                                            int znew = rndz > 0 ? 22 : 21; //>--- Midget : Lady 
                                            room.SpawnZombie(znew);
                                            Log.WriteInfo(">---Z.Mode-584 znew: " + znew);
                                        }
                                }
                            }
                            //>--- +++
                        }
                    }
                }
            }
            catch { }
        }

        public void Update()
        {
            Zombie();
        }

        public void reset()
        {
            PreparingWave = respawnThisWave = room.zombieRunning = room.SendFirstWave = room.FirstWaveSent = false;
            room.SleepTime = 15; //>--- *** 15

            ZombiePoints = room.SpawnedZombieplayers = room.KilledZombies = room.KillsBeforeDrop = room.ZombieSpawnPlace = 0;
            room.spawnedMadmans = room.spawnedManiacs = room.spawnedGrinders = room.spawnedGrounders = room.spawnedHeavys = room.spawnedGrowlers = 0;
            room.spawnedLovers = room.spawnedHandgemans = room.spawnedEnvys = room.spawnedClaws = room.spawnedMadSoldiers = room.spawnedMadPrisoners = room.spawnedLadys = room.spawnedMidgets = 0;
            room.spawnedChariots = room.spawnedCrushers = room.spawnedBusters = room.spawnedCrashers = room.spawnedBombers = room.spawnedDefenders = room.spawnedBreaker2s = room.spawnedSuperHeavys = 0;
            room.spawnedztipo0 = room.spawnedztipo1 = room.spawnedztipo2 = room.spawnedztipo3 = 0;

        }

        public ZombieMode(Room room)
        {
            this.room = room;
            reset();
        }

    }

}
