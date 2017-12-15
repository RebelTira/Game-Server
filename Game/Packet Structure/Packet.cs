using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Game_Server
{
    /// <summary>
    /// This class defines the packet structure
    /// </summary>
    class Packet : IDisposable
    {
        /* Causes crash on Linux - Mono */
        /*[DllImport("winmm.dll")]
        public static extern long timeGetTime();*/


        ushort packetId = 0;
        StringBuilder packet = new StringBuilder();

        public byte[] GetBytes()
        {
            string fullPacket = this.packet.ToString().Remove(packet.Length - 1, 1);

//>--- Para evitar tanto log
            string m = "";
            m = packet.ToString();
            if (!m.Contains ("25600") && !m.Contains("30016") && !m.Contains("31490"))
            { 
            Log.WriteDebug(">---PACKET OUT :: " + packet);
            }

            /* Default - for Windows */
            /* Windows-1250 - for Windows & Linux mono */

            byte[] data = Game.Cryption.encrypt(Encoding.GetEncoding("Windows-1250").GetBytes(fullPacket.ToString() + (char)0x20 + (char)0x0A));
            this.Dispose(); 
            return data;
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

        protected void newPacket(ushort packetId)
        {
            if (this.packetId == 0)
            {                           
                this.packetId = packetId;
                this.packet.Append(Environment.TickCount);
                this.packet.Append(" ");
                this.packet.Append(packetId); 
                this.packet.Append(" ");
            }
            else
            {
                Log.WriteError("Coudln't re-declare packetId!");
            }
        }

        protected void addBlock(object block)
        {
            block = block.ToString().Replace((char)0x20, (char)0x1D);
            this.packet.Append(block.ToString());
            this.packet.Append(" ");
        }

        protected void Fill(object block, int length)
        {
            for (int i = 0; i < length; i++)
            {
                addBlock(block);
            }
        }

        public void addRoomInfo(Room room)
        {
            //9 29 1 1 3 name 0 16 7 72 0 2 3 3 3 1 0 5 2 0 0 0 0 1 0 113 2 -1 
            //  2 2 2 9 name 0 16 12 66 0 5 4 3 3 1 0 0 0 1 0 0 0 1 0 67 0 -1
            //1 1 2 4 5 name 0 16 13 22 4 2 3 0 3 0 0 2 11110 0 0 0 0 1 0 0 -1
            Log.WriteInfo(">--- Packet-93 addRoomInfo - room.mode:" + room.mode + " users.Count: " + room.users.Count);
            addBlock(room.id);
            addBlock(room.status);
            addBlock(room.status);
            addBlock(room.master);
            addBlock(room.name);
            addBlock(room.enablepassword);
            addBlock(room.maxusers);
            addBlock(room.users.Count);
            addBlock(room.mapid);
            addBlock(room.channel == 3 ? room.zombiedifficulty : ((room.mode == (int)RoomMode.Explosive || room.mode == (int)RoomMode.HeroMode || room.mode == (int)RoomMode.Annihilation) ? room.rounds : 0));
            addBlock(room.rounds);
            addBlock(room.timelimit);

            addBlock(room.mode);
            addBlock(4);
            addBlock(room.isJoinable ? 1 : 0); // 0 = unjoinable(grey room)
            addBlock(0);
            addBlock(room.new_mode);
            addBlock(room.new_mode_sub);
            addBlock(room.supermaster ? 1 : 0); // 1 = Room has Supermaster
            addBlock(room.type);
            addBlock(room.levellimit);
            addBlock(room.premiumonly);
            addBlock(room.votekickOption);
            addBlock(room.autostart ? 1 : 0); // AutoStart
            addBlock(0); // Ping
            if (room.type == 1)
            {
                addBlock(1); //>--- ClanWar
                for (int i = 0; i < 2; i++)
                {
                    Clan clan = room.GetClan(i);
                    if (clan == null)
                    {
                        Fill(-1, 2);
                        addBlock("?");
                    }
                    else
                    {
                        addBlock(clan.id);
                        addBlock(clan.iconid);
                        addBlock(clan.name);
                    }
                }
            }
            else
            {
                addBlock(-1); //>--- No clanwar
            }
        }
    }
}