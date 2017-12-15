using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    
    class SP_ScoreboardInformations : Packet  //>---  Esta bien asi
    {
        public SP_ScoreboardInformations(Room r)
        {
            newPacket(30053);
            addBlock(3);
            if(r.timeattack != null)
            {
                DateTime dt = DateTime.Now;

                dt = dt.AddMilliseconds(r.timeattack.time.ElapsedMilliseconds);

                TimeSpan span = dt - DateTime.Now;

                addBlock(span.TotalMilliseconds);
                switch (r.timeattack.Stage)
                {
                    case 1:
                        {
                            var v = r.users.Values.OrderByDescending(u => u.kills).Take(2);  //>---  Solo se toman en cuenta los dos primeros
                            
                            foreach (User usr in v)
                            {
                                Log.WriteInfo(">---ScoreBInfo-28 --- Stage 1 - Kills: " + usr.kills);
                                addBlock(usr.roomslot);  //>---  Jugador con mas kills
                                addBlock((usr.rKills > r.timeattack.stage1ZombieCount ? r.timeattack.stage1ZombieCount : usr.rKills));
                            }
                            if (v.Count() == 1)  //>--- - ... Si solo hay un jugador
                            {
                                addBlock(-1);  //>---  Segundo jugador con mas kills (Si existe)
                                addBlock(0);
                            }
                            break;
                        }
                    case 2:   //>--- Ordena los jugadores por hackPercentage en el Stage 2
                        {
                            var v = r.users.Values.OrderByDescending(u => u.hackPercentage).Take(2);  //>---  pongo hackPercentage en vez de kills
                            foreach (User usr in v)
                            {
                                Log.WriteInfo(">---ScoreBInfo-48 --- Stage 2 - hackPercentage: " + usr.hackPercentage);
                                addBlock(usr.roomslot);
                                addBlock(usr.hackPercentage);
                            }
                            if (v.Count() == 1)
                            {
                                addBlock(-1);
                                addBlock(0);
                            }
                            break;
                        }
                    case 3:   //>--- Ordena los jugadores por daño a la puerta en el Stage 3
                        {
                            var v = r.users.Values.OrderByDescending(u => u.timeattackDamagedDoor).Take(2);  //>---  pongo  timeattackDamagedDoor en vez de kills original
                            foreach (User usr in v)
                            {
                                Log.WriteInfo(">---ScoreBInfo-54 --- Stage 3 - timeattackDamagedDoor: " + usr.timeattackDamagedDoor);
                                addBlock(usr.roomslot);
                                addBlock(usr.timeattackDamagedDoor);
                            }
                            if (v.Count() == 1)
                            {
                                addBlock(-1);
                                addBlock(0);
                            }
                            break;
                        }
                    case 4:
                        {
                            var v = r.users.Values.OrderByDescending(u => u.BossDamage).Take(2);
                            foreach (User usr in v)
                            {
                                Log.WriteInfo(">---ScoreBInfo-80 --- Stage 4 - BossDamage: " + usr.BossDamage);
                                addBlock(usr.roomslot);
                                addBlock(usr.BossDamage);
                            }
                            if (v.Count() == 1)
                            {
                                addBlock(-1);
                                addBlock(0);
                            }
                            break;
                        }
    
                    default:
                        {
                            addBlock(0);
                            addBlock(0);
                            addBlock(0);
                            addBlock(0);
                            break;
                        }

                }
            }
        }
    }
}
