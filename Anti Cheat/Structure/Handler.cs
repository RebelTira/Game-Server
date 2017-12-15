using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Anti_Cheat.Structure
{
    class Handler
    {
        private uint timeStamp = 0;
        public int packetId = 0;
        public string[] blocks;

        public void FillData(uint timeStamp, int packetId, string[] blocks)
        {
            this.timeStamp = timeStamp;
            this.packetId = packetId;
            this.blocks = blocks;
        }

        public string[] getAllBlocks
        {
            get
            {
                return this.blocks;
            }
        }

        public string getBlock(int i)
        {
            if (blocks[i] != null)
            {
                return blocks[i];
            }
            return null;
        }

        public virtual void Handle(Client usr)
        {
            /* Override */
        }
    }
}
