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

namespace Game_Server
{
    class Messenger : IDisposable
    {
        public int id;
        public string nickname;
        public int status = 0;
        public int requesterId = 0;
        public bool isOnline = false;

        public Messenger(int id, string nickname, int status, int requesterId)
        {
            this.id = id;
            this.nickname = nickname;
            this.status = status;
            this.requesterId = requesterId;
            this.isOnline = false;

            User u = Managers.UserManager.GetUser(id);

            if (u != null)
            {
                this.isOnline = true;
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}