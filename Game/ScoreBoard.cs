using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class CP_ScoreBoard : Handler //>--- INUTIL - No se ejecuta
        
    {
        public override void Handle(User usr)
        {
            if (usr.room == null) { usr.disconnect(); return; }

            if (usr.room.channel == 3 && usr.room.mode != 12)  //>---  Para TimeAttack se usa SP_ScoreboardInformations
            {
                //Log.WriteInfo(">---ScoreBrd-16  SP_ScoreBoard_AI surv. defense ");
                usr.send(new SP_ScoreBoard_AI(usr.room));
            }
            else 
            {
                //Log.WriteInfo(">---ScoreBrd-21  SP_ScoreBoard normal ");
                usr.send(new SP_ScoreBoard(usr.room));
            }
        }
    }

    class SP_ScoreBoard : Packet  //>--- La tabla de puntuaciones sale cambiada - lose x win (No en AI)
    {
        public SP_ScoreBoard(Room Room)  
        {
            newPacket(30032);
            addBlock(1);
            addBlock((Room.mode == (int)RoomMode.Explosive || Room.mode == (int)RoomMode.HeroMode ? Room.DerbRounds : 0));
            addBlock((Room.mode == (int)RoomMode.Explosive || Room.mode == (int)RoomMode.HeroMode ? Room.NIURounds : 0));

            RoomMode mode = (RoomMode)Room.mode;
            Log.WriteInfo(">---ScoreBrd-29 mode:  " + mode);
            switch (mode)
            {

                case RoomMode.HeroMode:
                    {
                        addBlock(Room.derbHeroKill);
                        addBlock(Room.niuHeroKill);
                        Log.WriteInfo(">---ScoreBrd-60 derbHeroKill: " + Room.derbHeroKill + "  niuHeroKill: " + Room.niuHeroKill);  //>--- 
                        break;
                    }
                case RoomMode.FFA:
                    {
                        addBlock(Room.ffakillpoints);
                        addBlock(Room.highestkills);
                        break;
                    }
                case RoomMode.FourVersusFour:
                case RoomMode.TDM:
                case RoomMode.Conquest:
                case RoomMode.TotalWar:
                case RoomMode.BGExplosive:
                    {
                        addBlock(Room.KillsDerbaranLeft);
                        addBlock(Room.KillsNIULeft);
                        Log.WriteInfo(">---ScoreBrd-60 KillsDerbaranLeft: " + Room.KillsDerbaranLeft + "  KillsNIULeft: " + Room.KillsNIULeft);  //>--- 
                        break;
                    }
                default:
                    {
                        addBlock(0);
                        addBlock(0);
                        break;
                    }
            }
            // 30032 1 0 0 3 4 8 0 0 0 0 0 0 0 1 0 0 0 0 0 0 2 0 0 0 0 0 0 3 0 0 0 0 0 0 4 0 0 0 0 0 0 5 0 0 0 0 0 0 6 0 0 0 0 0 0 7 0 0 0 0 0 0
            addBlock(Room.users.Count);
            Log.WriteInfo(">---ScoreBrd-87  users.Count: " + Room.users.Count );  
            foreach (User RoomUser in Room.users.Values)
            {
                //5 0 0 0 0 0 0
                //0 2 0 0 2 0 0 0 <-- AI
                //0 1 0 0 1 0 0 0 <-- AI
                addBlock(RoomUser.roomslot);
                addBlock(RoomUser.rKills);
                addBlock(RoomUser.rDeaths);
                addBlock(RoomUser.rFlags);
                addBlock(RoomUser.rPoints);
                addBlock(1); // Assist in chapter 1
            }
        }
    }
    class SP_ScoreBoard_AI : Packet 
    {
        public SP_ScoreBoard_AI(Room Room)  //>---  Añado este paquete para modo Zombies
        {
            newPacket(30032);
            addBlock(1);
            addBlock(0);
            addBlock(0);
            addBlock(0);
            addBlock(0);
            addBlock(Room.users.Count);

            foreach (User RoomUser in Room.users.Values)
            {
                addBlock(RoomUser.roomslot);
                addBlock(RoomUser.rKills);
                addBlock(RoomUser.rDeaths);
                addBlock(0);
                addBlock(RoomUser.rPoints);
                addBlock(0);
                addBlock(0);
            }
        }
    }
}
