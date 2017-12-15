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

        public int Wave = 1;

        public int ZombiePoints = 0;
                
        public int ZombieToWave = 0;

        public void PrepareNewWave()
        {
            room.send(new Game.SP_ZombieNewWave(0));
            room.spawnedMadmans = room.spawnedManiacs = room.spawnedGrinders = room.spawnedGrounders = room.spawnedHeavys = room.spawnedGrowlers = room.spawnedLovers = room.spawnedHandgemans = room.spawnedChariots = room.spawnedCrushers = 0;
            PreparingWave = true;
            room.KilledZombies = room.ZombieSpawnPlace = room.KillsBeforeDrop = room.SpawnedZombies = 0;
            room.SleepTime = 15; // 15 Seconds
            room.RespawnAllVehicles();
            respawnThisWave = false;
            room.send(new Game.SP_ZombieNewWave(Wave, LastWave));
            Wave++;
        }                

        public int LastTick = 0;

        public void Zombie()
        {
            try
            {
                if (LastTick != DateTime.Now.Second)
                {
                    LastTick = DateTime.Now.Second;
                    if (room.SendFirstWave)
                    {
                        room.FirstWaveSent = true;
                        room.send(new Game.SP_ZombieNewWave(0));
                        room.SendFirstWave = false;
                    }
                    if (room.zombieRunning)
                    {
                        /*foreach (User usr in Players)
                        {
                           usr.Health = 500000;
                        }*/
                        int DifficultyPer = (room.zombiedifficulty + 1);
                        switch (Wave)
                        {
                            case 1: ZombieToWave = 30; break; // Wave 1
                            case 2: ZombieToWave = 30; break; // Wave 2
                            case 3: ZombieToWave = 30; break; // Wave 3
                            case 4: ZombieToWave = 40; break; // Wave 4
                            case 5: ZombieToWave = 40; break; // Wave 5
                            case 6: ZombieToWave = 40; break; // Wave 6
                            case 7: ZombieToWave = 50; break; // Wave 7
                            case 8: ZombieToWave = 50; break; // Wave 8
                            case 9: ZombieToWave = 50; break; // Wave 9
                            case 10: ZombieToWave = 50; break; // Wave 10
                            case 11: ZombieToWave = 50; break; // Wave 11
                            case 12: ZombieToWave = 50; break; // Wave 12
                            case 13: ZombieToWave = 50; break; // Wave 13
                            case 14: ZombieToWave = 50; break; // Wave 14
                            case 15: ZombieToWave = 50; break; // Wave 15
                            case 16: ZombieToWave = 50; break; // Wave 16
                            case 17: ZombieToWave = 50; break; // Wave 17
                            case 18: ZombieToWave = 50; break; // Wave 18
                            case 19: ZombieToWave = 50; break; // Wave 19
                            case 20: ZombieToWave = 50; break; // Wave 20
                            case 21: ZombieToWave = 60; break; // Wave 21
                        }

                        LastWave = Wave >= (room.zombiedifficulty > 0 ? 18 : 21);

                        if (room.zombiedifficulty > 0) ZombieToWave += 15;
                        if (room.mode == (int)RoomMode.Defence) ZombieToWave += 5;
                        if (room.AliveDerb == 0) { room.EndGame(); }


                        //Log.WriteDebug(room.KilledZombies + " " + ZombieToWave);
                        if (room.KilledZombies >= ZombieToWave)
                        {
                            if (LastWave)
                            {
                                room.EndGame();
                            }
                            else
                            {
                                PrepareNewWave();
                            }
                            return;
                        }

                        if (room.SleepTime >= 0)
                        {
                            room.SleepTime--;
                            return;
                        }

                        if (PreparingWave) PreparingWave = false;

                        // Max 26 concurrent zombies.

                        if (room.Zombies.Where(r => r.Value.Health > 0).Count() >= 26 || room.SpawnedZombies >= ZombieToWave) return;
                        
                        if (!PreparingWave)
                        {
                            if (room.mode == (int)RoomMode.Defence)
                            {
                                if (room.spawnedHandgemans < 5) room.SpawnZombie(7);
                            }
                            switch (Wave)
                            {
                                case 1:
                                    {
                                        if (room.spawnedMadmans < 20) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedGrinders < 15) room.SpawnZombie(2);
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        if (room.spawnedMadmans < 15) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 5) room.SpawnZombie(2);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedGrounders < 15) room.SpawnZombie(3);
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        if (room.spawnedMadmans < 10) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 10) room.SpawnZombie(2);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedGrounders < 15) room.SpawnZombie(3);
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        if (room.spawnedMadmans < 20) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 10) room.SpawnZombie(2);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedMadmans < 25) room.SpawnZombie(0);
                                            if (room.spawnedManiacs < 15) room.SpawnZombie(1);
                                            if (room.spawnedGrounders < 5) room.SpawnZombie(3);
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (room.spawnedMadmans < 15) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 10) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 5) room.SpawnZombie(3);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedGrounders < 20) room.SpawnZombie(3);
                                        }
                                        break;
                                    }
                                case 6:
                                    {
                                        if (room.spawnedMadmans < 10) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 10) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 10) room.SpawnZombie(3);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedHeavys < 5) room.SpawnZombie(4);
                                            if (room.spawnedGrounders < 20) room.SpawnZombie(3);
                                        }
                                        break;
                                    }
                                case 7:
                                    {
                                        if (room.spawnedMadmans < 13) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 13) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 14) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 10) room.SpawnZombie(3);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedMadmans < 22) room.SpawnZombie(0);
                                            if (room.spawnedHeavys < 6) room.SpawnZombie(4);
                                        }
                                        break;
                                    }
                                case 8:
                                    {
                                        if (room.spawnedMadmans < 10) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 10) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 10) room.SpawnZombie(3);
                                        if (room.spawnedGrowlers < 10) room.SpawnZombie(5);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedMadmans < 15) room.SpawnZombie(0);
                                            if (room.spawnedChariots < 3) room.SpawnZombie(8);
                                            if (room.spawnedCrushers < 2) room.SpawnZombie(9);
                                            if (room.spawnedHeavys < 5) room.SpawnZombie(4);
                                        }
                                        break;
                                    }
                                case 9:
                                    {
                                        if (room.spawnedMadmans < 10) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 10) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 5) room.SpawnZombie(3);
                                        if (room.spawnedGrowlers < 10) room.SpawnZombie(5);
                                        if (room.spawnedLovers < 5) room.SpawnZombie(6);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedMadmans < 15) room.SpawnZombie(0);
                                            if (room.spawnedChariots < 3) room.SpawnZombie(8);
                                            if (room.spawnedCrushers < 2) room.SpawnZombie(9);
                                            if (room.spawnedHeavys < 5) room.SpawnZombie(4);
                                        }
                                        break;
                                    }
                                case 10:
                                case 12:
                                    {
                                        /*
                                        0 - Madman
                                        1 - Maniac
                                        2 - Grinder
                                        3 - Grounder
                                        4 - Heavy
                                        5 - Growler
                                        6 - Lover
                                        7 - Handgeman
                                        8 - Chariot
                                        9 - Crusher
                                        */
                                        if (room.spawnedMadmans < 10) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 10) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 5) room.SpawnZombie(3);
                                        if (room.spawnedLovers < 5) room.SpawnZombie(6);
                                        if (room.spawnedChariots < 1) room.SpawnZombie(8);
                                        if (room.spawnedGrowlers < 9) room.SpawnZombie(5);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedMadmans < 20) room.SpawnZombie(0);
                                            if (room.spawnedChariots < 3) room.SpawnZombie(8);
                                            if (room.spawnedCrushers < 3) room.SpawnZombie(9);
                                        }
                                        break;
                                    }
                                case 14:
                                case 16:
                                case 18:
                                    {
                                        if (room.spawnedMadmans < 10) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 10) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 5) room.SpawnZombie(3);
                                        if (room.spawnedGrowlers < 9) room.SpawnZombie(5);
                                        if (room.spawnedLovers < 5) room.SpawnZombie(6);
                                        if (room.spawnedChariots < 1) room.SpawnZombie(8);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedChariots < 4) room.SpawnZombie(8);
                                            if (room.spawnedCrushers < 2) room.SpawnZombie(8);
                                            if (room.spawnedMadmans < 15) room.SpawnZombie(0);
                                            if (room.spawnedHeavys < 5) room.SpawnZombie(4);
                                        }
                                        break;
                                    }
                                case 11:
                                case 13:
                                case 15:
                                case 17:
                                case 19:
                                    {
                                        if (room.spawnedMadmans < 10) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 10) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 10) room.SpawnZombie(3);
                                        if (room.spawnedGrowlers < 5) room.SpawnZombie(5);
                                        if (room.spawnedLovers < 5) room.SpawnZombie(6);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedMadmans < 20) room.SpawnZombie(0);
                                            if (room.spawnedHeavys < 5) room.SpawnZombie(4);
                                        }
                                        break;
                                    }
                                case 20:
                                    {
                                        if (room.spawnedMadmans < 10) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 10) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 5) room.SpawnZombie(3);
                                        if (room.spawnedGrowlers < 7) room.SpawnZombie(5);
                                        if (room.spawnedLovers < 5) room.SpawnZombie(6);
                                        if (room.spawnedChariots < 3) room.SpawnZombie(8);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedChariots < 8) room.SpawnZombie(8);
                                            if (room.spawnedMadmans < 15) room.SpawnZombie(0);
                                            if (room.spawnedHeavys < 5) room.SpawnZombie(4);
                                        }
                                        break;
                                    }
                                case 21:
                                    {
                                        if (room.spawnedMadmans < 10) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 10) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 10) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 5) room.SpawnZombie(3);
                                        if (room.spawnedGrowlers < 10) room.SpawnZombie(5);
                                        if (room.spawnedLovers < 5) room.SpawnZombie(6);
                                        if (room.spawnedChariots < 5) room.SpawnZombie(8);
                                        if (room.spawnedCrushers < 5) room.SpawnZombie(9);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedMadmans < 15) room.SpawnZombie(0);
                                            if (room.spawnedManiacs < 15) room.SpawnZombie(1);
                                            if (room.spawnedHeavys < 5) room.SpawnZombie(4);
                                        }
                                        break;
                                    }
                                case 22:
                                    {
                                        if (room.spawnedMadmans < 10) room.SpawnZombie(0);
                                        if (room.spawnedManiacs < 5) room.SpawnZombie(1);
                                        if (room.spawnedGrinders < 10) room.SpawnZombie(2);
                                        if (room.spawnedGrounders < 5) room.SpawnZombie(3);
                                        if (room.spawnedGrowlers < 10) room.SpawnZombie(5);
                                        if (room.spawnedLovers < 5) room.SpawnZombie(6);
                                        if (room.spawnedChariots < 5) room.SpawnZombie(8);
                                        if (room.spawnedCrushers < 5) room.SpawnZombie(9);
                                        if (room.spawnedHeavys < 5) room.SpawnZombie(4);
                                        if (room.zombiedifficulty > 0)
                                        {
                                            if (room.spawnedChariots < 10) room.SpawnZombie(8);
                                            if (room.spawnedCrushers < 8) room.SpawnZombie(9);
                                            if (room.spawnedHeavys < 12) room.SpawnZombie(4);
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public void Update()
        {
            if (room.AliveDerb == 0) { room.EndGame(); return; }
            Zombie();
        }

        public void reset()
        {
            PreparingWave = respawnThisWave = room.zombieRunning = room.SendFirstWave = room.FirstWaveSent = false;
            room.SleepTime = 15;
            ZombiePoints = room.SpawnedZombieplayers = room.KilledZombies = room.KillsBeforeDrop = room.ZombieSpawnPlace = room.spawnedMadmans = room.spawnedManiacs = room.spawnedGrinders = room.spawnedGrounders = room.spawnedHeavys = room.spawnedGrowlers = room.spawnedLovers = room.spawnedHandgemans = room.spawnedChariots = room.spawnedCrushers = 0;
        }

        public ZombieMode(Room room)
        {
            this.room = room;
            reset();
        }
    }
}
