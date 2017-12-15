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
    class RoomHandler_Damage : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {
            // 27 = NX / 26 = G1
            if (!room.gameactive || room.sleep && room.bombDefused || room.firstInGameTS > Generic.timestamp) return;

            int targetId = int.Parse(getBlock(7));
            uint HSCalculation = uint.Parse(getBlock(15)) - usr.sessionId;
            string shootenPart = getBlock(22);
            int slotId = int.Parse(getBlock(8));
            int weaponSlot = int.Parse(getBlock(13)); // Weapon slot (example if you shoot with knuckles the slot is 0, if K1, slot is 2, etc...)
            
            bool useRadius = getBlock(14) == "1";
            string weapon = getBlock(27).Substring(0, 4);
            
            Item Item = ItemManager.GetItem(weapon);
            if (Item == null) return;

            //22 = Nexon - 21 = Older clients
            //27 = Nexon - 26 = Older clients <- Weapon

            int points = Configs.Server.Experience.OnKillPoints;

            int Type = 1;

            switch (HSCalculation)
            {
                case 1237: Type = 0; break; //>--- Granadas
                case 1239: Type = 1; break;
                case 1241: Type = 2; break;
            }

            /* Avoid nades as hs etc*/

            if (weapon.StartsWith("DM") || weapon.StartsWith("DN") || ((weapon.StartsWith("DJ") || weapon.StartsWith("DK")) && HSCalculation >= 1237)) // Weapon hit user at 100%
                Type = 0;

            int Damage = ItemManager.GetDamage(weapon, Type);

            if (usr.currentVehicle != null)
            {
                bool MainCT = int.Parse(getBlock(13)) == 0;
                if (weapon == usr.currentVehicle.Code)
                {
                    points += Configs.Server.Experience.OnVehicleKillAdditional;
                    if (MainCT)
                    {
                        Damage = ItemManager.GetDamage(usr.currentSeat.MainCTCode);
                    }
                    else
                    {
                        Damage = ItemManager.GetDamage(usr.currentSeat.SubCTCode);
                    }
                }
            }
            else
            {
                if (room.new_mode == 5) // Kamikaze mode -> 1 hit
                {
                    Damage = 1000; // One hit
                }
            }

            bool isNade = weapon.StartsWith("DN") || weapon.StartsWith("DM"); //>--- Granadas

            int[] calculations = new int[] { 75, 100 };

            int min = calculations[isNade ? 0 : 1];

            if (HSCalculation > 0 && HSCalculation < min)
            {
                // Calculate range and remove the percentage from the damage
                Damage = (int)Math.Ceiling((double)(Damage * HSCalculation) / 100);
            }

            if ((usr.Health <= 0 || !usr.IsAlive()) && (HSCalculation < 0 || HSCalculation > 100)) return;

            if (!usr.IsWhitelistedWeapon(weapon) && (usr.currentVehicle != null && weapon != usr.currentVehicle.Code)) return;
            bool hs = false;           
            if (room.channel == 3) //>--- Zombies Damage
            {
                if (targetId >= 4 && slotId < 4) // Target > 4 (Zombie) && Shooter < 4 (Player) 
                {
                    if (Type == 0) //>--- Granadas
                    {
                        if (!weapon.StartsWith("DA") && !weapon.StartsWith("DN") && !weapon.StartsWith("DJ") && !weapon.StartsWith("DM") && !weapon.StartsWith("DK"))
                        {
                            shootenPart = "99.0000";
                            hs = true;
                        }
                    }

                    Zombie zombie = room.GetZombieByID(targetId); //>--- targetID - Slot que ocupa el blanco de nuestros disparos
                    //Log.WriteInfo(">---Damage-121 targetId: " + targetId);

                    if (zombie != null && zombie.Health > 0) //>--- disparando a Zs
                    {
                        if (zombie.timestamp > Generic.timestamp) return;
                        points = zombie.Points;

                        if (hs) //>--- Si HeadShot puntos x 2
                        {
                            points *= 2;
                        }
                        if (zombie.Name == "Breaker")
                        {
                            double RCoef = (30 / 100.0); //>--- +++ 
                            double vOut = Convert.ToDouble(Damage); //>--- +++ 
                            Damage = (int)(vOut * RCoef); //>--- +++ 
                            usr.BossDamage += Damage;
                            Log.WriteInfo(">---Damage-135 BossDamage: " + usr.BossDamage  );
                        }
                        if (usr.GodMode) { Damage = 10000; }  //>---  +++ 
                        zombie.Health -= Damage;
                        //Log.WriteInfo(">---Damage-138 zombie.Type: " + zombie.Type + " - Damage: " + Damage + " - zombie.Health: " + zombie.Health);
                        sendBlocks[12] = Convert.ToString(getBlock(12));
                        sendBlocks[15] = Convert.ToString(zombie.Health);
                        sendBlocks[16] = Convert.ToString(zombie.Health - Damage);
                        sendBlocks[22] = Convert.ToString(getBlock(22));

                        if (zombie.Health > 0 && zombie.Name != "Breaker")
                        {
                            sendBlocks[15] = zombie.Health.ToString();
                            sendBlocks[16] = (zombie.Health + Damage).ToString();
                            sendBlocks[27] = weapon.ToString();
                        }




                        //>-----------------------------------------------------------------------
                        if (zombie.Health <= 0) //>--- Z muerto
                        {
                            room.KillsBeforeDrop++;
     
                            //Log.WriteInfo(">---Damage-160 // >--- ----------- Zombie Killed: " + zombie.Type);

                            int b = (room.zombie != null ? room.zombie.Wave : room.timeattack.Stage); //>--- Si survival-defense b=wave : T.A. b=stage
                            int c = (room.zombie != null ? 3 : 0);  //>--- Si survival-defense c=3 : T.A. c=0
                    
                            if (zombie.Type == 16 )
                            {
                                room.BossKilled = true;
                                //Log.WriteInfo(">---Damage-168  %%%%%%%---%%%%% BossKilled %%%%%%---%%%%%%" );
                            }
                            if (b >= c && room.DropID < 90 && room.KillsBeforeDrop >= 10) //>--- Si wave > 3 -y- Drops hasta ahora < 90 -y- 10 0 mas kills desde el ultimo drop
                            {
                                int DropType = room.RandomDrop(); //>--- RandomDrop en Room-1092

                                room.send(new SP_ZombieDrop(usr, targetId, room.DropID, DropType));
                                room.DropID++;
                                room.KillsBeforeDrop = 0;
                            }

                            if (room.zombie != null)
                            {
                                //Log.WriteInfo(">---Damage-182  zombie.Type: " + zombie.Type);
                                if (zombie.Type == 7 && usr.skillPoints < 20) //>--- Si zombie es Handgeman, skillpoint
                                {
                                    usr.skillPoints++;
                                    if (usr.skillPoints == 5 || usr.skillPoints == 10 || usr.skillPoints == 20)
                                        room.send(new SP_ZombieSkillpointUpdate(usr));
                                }
                            }

                            usr.rPoints += points;
                            usr.rKills++;
  
                            zombie.Health = 0;
                            zombie.respawn = Generic.timestamp + 4;

                            room.KilledZombies++;
                            room.ZombiePoints += points;

                            sendBlocks[3] = (int)Subtype.ServerKill;
                            sendBlocks[8] = slotId;
                            sendBlocks[22] = shootenPart;
                        }
                    }
                }
                else if (targetId <= 3 && slotId >= 4) //>--- Target player
                {
                    Zombie Zombie = room.GetZombieByID(slotId); // SlotID -> From who getting shotted

                    if (slotId == usr.roomslot) return; // Cant shoot yourself

                    if (Zombie == null || Zombie.Health <= 0 || usr.Health <= 0 || Zombie.ID == -1) return;

                    if (!usr.GodMode) { usr.Health -= Zombie.Damage; } //>--- No damage in AI. 

                    //Log.WriteInfo(">---Damage-216 Health: " + usr.Health + " Zombie.Damage: " + Zombie.Damage);

                    sendBlocks[8] = slotId; 
                    sendBlocks[16] = usr.Health;
                    sendBlocks[17] = (usr.Health + Zombie.Damage); // 
                    sendBlocks[27] = weapon; // Weapon (NX removes the other 4 chars at the send ( Splitted full is ) - DF01299 = Weapon[4]|Class[1]

                    if (usr.Health <= 0) //>--- Player muerto
                    {
                        //Log.WriteInfo(">---Damage-228 Health 0?? : " + usr.Health );


                        if (room.timeattack != null)
                        {
                            room.timeAttackSpawns--;
                            room.timeattack.vidas--;
                            //Log.WriteInfo(">---Damage-232 timeAttackSpawns: " + room.timeAttackSpawns);

                            sendBlocks[3] = (int)Subtype.ServerKill; //>--- +++  

                        }
                        else
                        {
                            sendBlocks[3] = (int)Subtype.ServerKill; 
                            usr.OnDie(); //>--- +++ No se usa para TA
                            //Log.WriteInfo(">---Damage-241 usr.OnDie(); ");
                        }
                    }
                }
            }
            else
            {
                User target = room.users[targetId];
                if (target == null || target.spawnprotection > 0 || room.GetSide(usr) == room.GetSide(target) && room.mode != 1) return; //>--- Fuego amigo. No aplicable en FFA

                if (HSCalculation == 1237) // HS = x2  Ganadas
                {
                    //>---                  DA=Slot 1               DN=Slot 4                   Lanzacohetes                Granadas                Lanzamisiles
                    if (!weapon.StartsWith("DA") && !weapon.StartsWith("DN") && !weapon.StartsWith("DJ") && !weapon.StartsWith("DM") && !weapon.StartsWith("DK"))
                    {
                        Damage += 200;
                        shootenPart = "99.0000"; //>--- HeadShot
                        hs = true;
                    }
                }

                if (target.Health >= 0)
                {
                    if (usr.GodMode) { Damage = 10000; }  //>---  +++  Si GodMode daño = 10000
                    target.Health -= Damage; //>--- Daño al enemigo

                    sendBlocks[8] = slotId;
                    sendBlocks[16] = target.Health; //
                    sendBlocks[17] = (target.Health + Damage); // 
                    sendBlocks[18] = points; // UPDATE 24.07.2013 - Showen points in the center of the screen when you kill
                    sendBlocks[22] = shootenPart; // Headshot or other shit handled from SessionID [ TODO: Costume calculate damage ]
                    sendBlocks[27] = weapon; // Weapon (NX removes the other 4 chars at the send ( Splitted full is ) - DF01299 = Weapon[4]|Class[1]

                    if (target.Health <= 0)
                    {
                        if (room.heromode != null && (target.roomslot == room.derbHeroUsr || target.roomslot == room.niuHeroUsr))
                        {
                            points += 20;
                        }
                        target.OnDie();
                        
                        if (room.mode == 8) //>--- Total War
                        {
                            usr.TotalWarPoint = 0;
                            target.TotalWarSupport += 2;

                            sendBlocks[19] = usr.TotalWarPoint;
                            sendBlocks[20] = usr.TotalWarSupport;

                            switch (room.GetSide(usr))
                            {
                                case 0: room.TotalWarDerb += 5; room.TotalWarNIU += 2; break;
                                case 1: room.TotalWarNIU += 5; room.TotalWarDerb += 2; break;
                            }
                        }

 // Esto estaba comentado.  Para evitar que se cuenten los HeadShots en modo Cabezón 
                         if (hs)
                        {
                            usr.rPoints += Configs.Server.Experience.OnHSKillPoints;
                            usr.rHeadShots++;
                            if (room.new_mode != 6 && room.new_mode_sub != 0) //>--- Big Head
                            {
                                usr.send(new SP_CustomSound(SP_CustomSound.Sounds.HeadShot));
                            }
                        }
// hasta aqui 
                        usr.rKillSinceSpawn++;
                                                
                        if (!room.firstblood)
                        {
                            room.firstblood = true;
                            usr.rPoints += 3;
                            usr.send(new SP_KillAnimation(SP_KillAnimation.Type.FirstKill));
                        }

                        if (usr.lastKillUser == target.roomslot)
                        {
                            usr.send(new SP_KillAnimation(SP_KillAnimation.Type.RevengeKill));
                            usr.rPoints++;
                        }

                        if(isNade)
                        {
                            usr.send(new SP_KillAnimation(SP_KillAnimation.Type.GrenadeKill));
                            usr.rPoints++;
                        }
                        else if(hs)
                        {
                            usr.send(new SP_KillAnimation(SP_KillAnimation.Type.HeadShot));
                            usr.rPoints++;
                        }
                        
                        switch (usr.rKillSinceSpawn)
                        {
                            case 1: // Do nothing
                                break;
                            case 2:
                                usr.send(new SP_KillAnimation(SP_KillAnimation.Type.DoubleKill));
                                usr.rPoints++;
                                break;
                            case 3:
                                usr.send(new SP_KillAnimation(SP_KillAnimation.Type.TripleKill));
                                usr.rPoints++;
                                break;
                            case 4:
                                usr.send(new SP_KillAnimation(SP_KillAnimation.Type.QuadraKill));
                                usr.rPoints += 2;
                                break;
                            case 5:
                                usr.send(new SP_KillAnimation(SP_KillAnimation.Type.PentaKill));
                                usr.rPoints += 2;
                                break;
                            case 6:
                                usr.send(new SP_KillAnimation(SP_KillAnimation.Type.HexaKill));
                                usr.rPoints += 3;
                                break;
                            case 7:
                                usr.send(new SP_KillAnimation(SP_KillAnimation.Type.UltraKill));
                                usr.rPoints += 3;
                                break;
                            case 8:
                                usr.send(new SP_KillAnimation(SP_KillAnimation.Type.Assasin));
                                usr.rPoints += 2;
                                break;
                        }

                        target.lastKillUser = usr.roomslot;


                        // Snow Fight 
                        /*if(room.mapid == 72 && room.new_mode == 6 && room.new_mode_sub == 2)
                        {
                            usr.KillEventCheck();
                        }
                        // GunSmith Materials Earn part 
                        */
                        int g_perc = Generic.random(0, 500);
                        if (g_perc < 20)
                        {
                            usr.RandomGunsmithResource();
                        }

                        if (usr.rKills > room.highestkills) { room.highestkills = usr.rKills; }

                        if (room.channel == 3 && room.mode == 13 && target.IsEscapeZombie) room.EscapeZombie--;
                        if (room.channel == 3 && room.mode == 13 && !target.IsEscapeZombie) 
                        {
                            room.EscapeHuman--;
                            target.IsEscapeZombie = true;
                        }

                        if (room.KillsDerbaranLeft == 30 || room.KillsNIULeft == 30 || room.NIURounds >= 6 || room.DerbRounds >= 6)
                        {
                            lobbychanges = true;
                        }

                        if (target.currentVehicle != null) points += 5;

                        //Log.WriteInfo(">---Damage-400 target.currentVehicle: " + target.currentVehicle);

                        sendBlocks[3] = (int)Subtype.ServerKill;
                        usr.rKills++;
                        usr.rPoints += points;
                        
                        if (room.explosive != null)
                        {
                            sendPacket = false;
                            room.send(new SP_RoomData(sendBlocks));
                            room.explosive.CheckForNewRound();
                            return;
                        }
                        
                            else if (room.heromode != null)
                        {
                            sendPacket = false;
                            room.send(new SP_RoomData(sendBlocks));
                            room.heromode.CheckForNewRound();
                            return;
                        }
                        
                        /*
                        if (room.mode == (int)RoomMode.FFA)
                        {
                            sendPacket = false;
                            room.send(new SP_RoomData(sendBlocks));
                            // Max 30 kills at FFA
                            if (room.ffa.gunGameScores[usr.roomslot] < 30)
                            {
                                room.ffa.UpdateGunGameScore(usr.roomslot);
                                room.ffa.GunGameUpdate(usr);
                                usr.SwitchWeapon(room.ffa.GetGunGameWeapon(usr));
                            }
                            room.ffa.UpdateGunGameScore(usr.roomslot);
                            target.SwitchWeapon("DA01"); //>--- M7
                            return;
                        }
                        */
                    }
                }
            }

            /* Important */

            sendPacket = true;
        }
    }
}
