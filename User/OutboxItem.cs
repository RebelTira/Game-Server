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
    public struct OutboxItem
    {
        public int id;
        public string itemcode;
        public ushort days;
        public ushort count;
        public int timestamp;
        public OutboxItem(int id, string itemcode, ushort days, int timestamp, ushort count)
        {
            this.id = id;
            this.itemcode = itemcode;
            this.days = days;
            this.timestamp = timestamp;
            this.count = count;
        }
    }
}
