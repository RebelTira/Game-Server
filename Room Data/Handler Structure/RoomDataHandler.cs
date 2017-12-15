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

namespace Game_Server.Room_Data
{
    class RoomDataHandler : IDisposable
    {
        private int subtype = 0;
        public object[] sendBlocks;
        public bool lobbychanges = false;
        public bool sendPacket = false;

        public void FillData(int subtype, object[] sendBlocks)
        {
            this.subtype = subtype;
            this.sendBlocks = sendBlocks;
            this.sendPacket = false;
        }

        public string[] getAllBlocks
        {
            get
            {
                return (string[])this.sendBlocks;
            }
        }
        
        public string getBlock(int i)
        {
            if (sendBlocks[i] != null)
                return sendBlocks[i].ToString();
            return null;
        }

        public virtual void Handle(User usr, Room room)
        {
            /* Override */
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