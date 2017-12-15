using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server
{
    class Handler : IDisposable
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

        public virtual void Handle(User usr)
        {
            /* Override */
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void WritePacket() //>--- No se ejecuta. 0 Referencias
        {
            Log.WriteDebug(string.Join(">---HANDLER-53 WRITE PAKET ", getAllBlocks));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
