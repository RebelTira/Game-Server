using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_EndGame : Packet
    {
        public SP_EndGame(User usr)
        {
            Room Room = usr.room;
            //30048 1 0 1 0 0 0 0 0 0 3 0 4 10 0 87 97 481 1169318 2 0 1 2 2 0 41 76 267 2315317 0 0 16 7 8 0 42 100 443 1699204 12 0 16 
            //30048 1 31 60 31 60 0 0 0 0 0 0 0 0 0 186 130360
            //Log.WriteInfo(">---EndGame-15 newPacket(30048): ");
            newPacket(30048);
            addBlock(1);
            if (Room.channel != 3)
            {
                addBlock(usr.ExpEarned);
                addBlock(usr.DinarEarned);
                Fill(0, 2);
                if (Room.mode != 5) //>--- Mission Mode
                {
                    addBlock((Room.channel == 1 && (Room.mode == (int)RoomMode.Explosive || Room.mode == (int)RoomMode.HeroMode) ? Room.DerbRounds : Room.KillsDerbaranLeft)); // Rounds Won Derberan
                    addBlock((Room.channel == 1 && (Room.mode == (int)RoomMode.Explosive || Room.mode == (int)RoomMode.HeroMode) ? Room.NIURounds : Room.KillsNIULeft)); // Rounds Won NIU
                    Log.WriteInfo(">---EndGame-27 DerbRounds: " + Room.DerbRounds + "  KillsDerbaranLeft: " + Room.KillsDerbaranLeft);
                    Log.WriteInfo(">---EndGame-28 NIURounds: " + Room.NIURounds + "  KillsNIULeft: " + Room.KillsNIULeft);
                }
                else
                {
                    if (Room.mapid == 42) //>--- Siegewar
                    {
                        Log.WriteInfo(">---EndGame-34 Mission3: " + Room.Mission3);
                        addBlock(Room.Mission3 != null ? 1 : 0);
                        addBlock(Room.Mission3 != null ? 0 : 1);
                        //addBlock(Room.Mission3 != null ? 0 : 1); //>--- +++
                        //addBlock(Room.Mission3 != null ? 1 : 0); //>--- +++
                    }
                    else if (Room.mapid == 60) // >--- Antes 56 - SiegeWar2
                    {
                        Log.WriteInfo(">---EndGame-40 Mission2: " + Room.Mission2);
                        addBlock(Room.Mission2 != null ? 1 : 0);
                        addBlock(Room.Mission2 != null ? 0 : 1);
                    }
                    else
                    {
                        Fill(0, 2);
                    }
                }
                Fill(0, 6);
                /*
                addBlock(Room.KillsDerb);
                addBlock(Room.DeathDerb);
                addBlock(Room.KillsNiu);
                addBlock(Room.DeathNiu);
                addBlock(0);
                addBlock(0);
                addBlock(Room.Players.Count);
                foreach (usr in room.User.Values)
                {
                    addBlock(virtualUser.RoomSlot);
                    addBlock(virtualUser.rKills);
                    addBlock(virtualUser.rDeaths);
                    addBlock(virtualUser.rFlags);
                    addBlock(virtualUser.rPoints);
                    addBlock(virtualUser.DinarEarned);
                    addBlock(virtualUser.ExpEarned);
                    addBlock(virtualUser.Exp);
                    addBlock(0);
                    addBlock(0);
                }
                addBlock(Room.RoomMasterSlot);
                */
            }
            else //>--- RoomChannel = 3
            {
                //30048 1 0 106000 52 69 13 69 0 0 0 0 39 0 0 0 0 580 130685

                if (Room.zombie != null) //>--- Survival, defense
                {
                    Log.WriteInfo(">---EndGame-57 Survival, Defense ");
                    addBlock(Room.zombie.Wave >= (Room.zombiedifficulty > 0 ? 21 : 18) ? 1 : 0); //>--- Cambio "0 ? 18 : 21" x "0 ? 21 : 18"
                }
                else //>--- TimeAttack
                {
                    Log.WriteInfo(">---EndGame-62  Room.timeattack.Stage: "+ Room.timeattack.Stage);
                    addBlock(Room.timeattack.Stage >= (Room.zombiedifficulty > 0 ? 4 : 3) ? 1 : 0); //>---- Cambio "0 ? 2 : 3" x "0 ? 4 : 3
                }
                addBlock(Room.timespent);
                addBlock(usr.ExpEarned);
                addBlock(usr.DinarEarned);
                Fill(0, 10);
            }
            addBlock(Room.master);
            addBlock(usr.exp);
            addBlock(usr.dinar);
        }
    }
}