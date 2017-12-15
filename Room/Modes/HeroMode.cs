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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Game;
using Game_Server.Room_Data;

namespace Game_Server.GameModes
{
    class HeroMode
    {
        ~HeroMode()
        {
            GC.Collect();
        }
        Room room = null;

        int sleepTick = 0;

        public void CheckForNewRound()
        {
            if (room.derbHeroKill == 0 || room.niuHeroKill == 0 || room.timeleft <= 0)
            {
                int winningTeam = (room.timeleft <= 0 ? (room.derbHeroKill < room.niuHeroKill ? (int)Room.Side.NIU : (int)Room.Side.Derbaran) : (room.derbHeroKill == (int)Room.Side.Derbaran ? (int)Room.Side.NIU : (int)Room.Side.Derbaran));
                if (sleepTick >= 5 && !room.EndGamefreeze)
                {
                    if (winningTeam == (int)Room.Side.Derbaran) room.DerbRounds++; else room.NIURounds++;
                    sleepTick = 0;
                    room.derbHeroKill = room.niuHeroKill = 3;
                    room.derbHeroUsr = room.niuHeroUsr = -1;
                    room.timeleft = 300000;
                    room.updateTime();
                    room.Placements.Clear();

                    room.send(new SP_RoomDataNewRound(room, winningTeam, false));
                    room.send(new SP_InitializeNewRound(room));
                }
                else
                {
                    int winningTeamRounds = winningTeam == (int)Room.Side.Derbaran ? room.DerbRounds : room.NIURounds;
                    if (winningTeamRounds + 1 >= room.explosiveRounds)
                    {
                        if (winningTeam == (int)Room.Side.Derbaran) room.DerbRounds++; else room.NIURounds++;
                        room.EndGame();
                    }
                    else
                    {
                        if (sleepTick == 0)
                        {
                            room.send(new SP_RoomDataNewRound(room, winningTeam, true));

                            foreach (User usr in room.tempPlayers)
                            {
                                usr.isSpawned = false;
                                usr.throwNades = usr.throwRockets = 0;
                            }
                        }
                        sleepTick++;
                    }
                }
            }
        }

        public void Update()
        {
            if (room != null)
            {
                if (room.users.Count >= 1 && room.gameactive)
                {
                    if (room.AliveDerb > 0 && room.derbHeroUsr == -1)
                    {
                        room.derbHeroUsr = room.users.Values.Where(r => r != null && room.GetSide(r) == (int)Room.Side.Derbaran && r.IsAlive()).OrderBy(qu => Guid.NewGuid()).FirstOrDefault().roomslot;
                    }

                    if (room.AliveNIU > 0 && room.niuHeroUsr == -1)
                    {
                        room.niuHeroUsr = room.users.Values.Where(r => r != null && room.GetSide(r) == (int)Room.Side.NIU && r.IsAlive()).OrderBy(qu => Guid.NewGuid()).FirstOrDefault().roomslot;
                    }

                    if (room.NIURounds >= room.explosiveRounds || room.DerbRounds >= room.explosiveRounds) { room.EndGame(); return; }
                    CheckForNewRound();
                }
            }
        }

        public HeroMode(Room room)
        {
            this.room = room;
        }
    }
}