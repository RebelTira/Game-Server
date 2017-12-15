using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_RoomThread : Packet
    {
        public SP_RoomThread(Room room, int type = 0)
        {
            newPacket(30016);
            if (room.channel == 3)
            {
                addBlock((room.timeattack != null ? room.timeleft : 0));
                addBlock(room.timespent);
                addBlock(room.ZombiePoints); // Points
            }
            else
            {
                switch (room.mode)
                {
                    case 8: // Total War
                        {
                            //30016 322000 0 0 0 0 0 0 1478000 0 0 22 36 2 1 2
                            addBlock(room.timespent);
                            Fill(0, 4);
                            addBlock(room.kills);
                            addBlock(0);
                            addBlock(room.timeleft);
                            Fill(0, 2);
                            addBlock(room.TotalWarDerb);
                            addBlock(room.TotalWarNIU);
                            break;
                        }
/*
                    case 9: //>--- Añadido para Capture mode
                        {
                            addBlock(room.timespent);
                            Fill(0, 4);
                            addBlock(room.kills);
                            addBlock(0);
                            addBlock(room.timeleft);
                            Fill(0, 2);
                            addBlock(room.DerbaranPoints);
                            addBlock(room.NIUPoints);
                            break;
                        }
*/
                    default:
                        {
                            //Log.WriteInfo(">---RoomTickPacket-55  mode default (Excepto TotalWar)");
                            //30016 303000 897000 0 0 300 299 2 0 30 1 Tishina 0 NONE 0 NONE 1
                            //30016 594000 606000 0 0 294 295 2 0 30 1 Tishina 1 Tishina 0 NONE 2
                            //30016 58000 2342000 0 0 200 200 2 1 1 
                            //30016 59000 2341000 0 0 200 190 2 2 30
                            addBlock(room.timespent); // Spawn Counter
                            addBlock(room.timeleft); // Time Left
                            if (room.mode == 2 || room.mode == 3 || room.mode == 4 || room.mode == 5 ) // DeathMatch - Conquest - Mission
                            {
                                //Log.WriteInfo(">---RoomTickPacket-63  modes 3-DeathMatch - 4-Conquest - 5-Mission");
                                addBlock(room.AliveDerb);
                                addBlock(room.AliveNIU);
                                addBlock(room.KillsDerbaranLeft);
                                addBlock(room.KillsNIULeft);
                            }
                            else if (room.mode == (int)RoomMode.Explosive || room.mode == (int)RoomMode.HeroMode || room.mode == (int)RoomMode.Annihilation)
                            {
                                //Log.WriteInfo(">---RoomTickPacket-73  modes 0-Explosive - 7-HeroMode - 15-Annihilation");
                                //30016 14000 286000 0 0 3 3 -1 -1 2 0 30
                                if (room.mode == (int)RoomMode.Annihilation)
                                {
                                    addBlock(0); // Over Time
                                }
                                addBlock(room.DerbRounds);
                                addBlock(room.NIURounds);
                                addBlock(room.mode == (int)RoomMode.HeroMode ? room.derbHeroKill : room.AliveDerb);
                                addBlock(room.mode == (int)RoomMode.HeroMode ? room.niuHeroKill : room.AliveNIU);
                                addBlock(room.mode == (int)RoomMode.HeroMode ? room.derbHeroUsr : room.DerbRounds);
                                addBlock(room.mode == (int)RoomMode.HeroMode ? room.niuHeroUsr : room.NIURounds);
                            }
                            else
                                //Log.WriteInfo(">---RoomTickPacket-91  mode default (Excepto 8-TotalWar - 3-DeathMatch - 4-Conquest - 5-Mission - 0-Explosive - 7-HeroMode - 15-Annihilation)");
                            {
                                addBlock(room.DerbRounds);
                                addBlock(room.NIURounds);
                                addBlock(room.ffakillpoints);
                                addBlock(room.highestkills);
                            }
                            break;
                        }
                }
            }
            addBlock(2);
            addBlock(type);
            addBlock(room.ConquestCountdown);
            if (room.mode == 5)
            {
                switch (room.mapid)
                {
                    case 42: // >--- Siegewar
                        {
                            addBlock(room.Mission1 == null ? 0 : 1);
                            addBlock(room.Mission1 == null ? "NONE" : room.Mission1);
                            addBlock(room.Mission2 == null ? 0 : 1);
                            addBlock(room.Mission2 == null ? "NONE" : room.Mission2);
                            addBlock(room.Mission3 == null ? 0 : 1);
                            addBlock(room.Mission3 == null ? "NONE" : room.Mission3);
                            addBlock(room.GetActualMission);
                            break;
                        }
                    case 60: // >--- Siegewar2
                        {
                            addBlock(room.Mission1 == null ? 0 : 1);
                            addBlock(room.Mission1 == null ? "NONE" : room.Mission1);
                            addBlock(room.Mission2 == null ? 0 : 1);
                            addBlock(room.Mission2 == null ? "NONE" : room.Mission2);
                            addBlock(room.Mission3 == null ? 0 : 1);
                            addBlock(room.Mission3 == null ? "NONE" : room.Mission3);
                            addBlock(room.GetActualMission);
                            if (room.Mission1 == null)
                            {
                                addBlock(-2);
                                Fill(0, 2);
                                addBlock(1);
                                addBlock((room.HackPercentage.BaseA + room.HackPercentage.BaseB) + ".0000");
                            }
                            else
                            {
                                addBlock(0);
                            }
                            break;
                        }
                }
            }
        }
    }
}
