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

namespace Game_Server
{
    class Zombie
    {
        public int ID = 0;
        public string Name = null;
        public int Health = 0;
        public int Points = 1;
        public int Damage = 150;
        public bool SkillPoint = false;
        public int Type = 0;
        public int FollowUser = -1;
        public int timestamp = 0;
        public int respawn;

        public Zombie(int ID, int FollowUser, int timestamp, int Type) //>--- 0-3 Players - 4-31 Zombies. - (28 Max??)
        {
			          
            this.ID = ID;
            this.FollowUser = FollowUser;
            this.timestamp = timestamp;
            this.Type = Type;
            this.respawn = 0;
            this.Health = 0;
        }

        public void Reset()
        {
            ZombieManager.GetZombieData(this);
        }
    }
}