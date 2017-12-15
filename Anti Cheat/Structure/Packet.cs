using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Game_Server.Anti_Cheat.Structure
{
    /// <summary>
    /// This class defines the packet structure
    /// </summary>
    class Packet
    {
        /* Causes crash on Linux - Mono */
        /*[DllImport("winmm.dll")]
        public static extern long timeGetTime();*/

        int packetId;
        string[] blocks = new string[0];

        public byte[] GetBytes()
        {
            string packet = Environment.TickCount + " " + packetId + " ";

            for (int i = 0; i < blocks.Length; i++)
            {
                packet += blocks[i].Replace((char)0x20, (char)0x1D) + " ";
            }

            Log.WriteInfo(">---Packet-AntiCheat - OUT: " + packet );
            /* Default - for Windows */
            /* Windows-1250 - for Windows & Linux mono */

            return Game.Cryption.encrypt(Encoding.GetEncoding("Windows-1250").GetBytes(packet.ToString() + (char)0x20 + (char)0x0A));
        }

        protected void newPacket(int packetId)
        {
            this.packetId = packetId;
        }

        protected void addBlock(object block)
        {
            int length = blocks.Length + 1;
            Array.Resize(ref blocks, length);
            blocks[length - 1] = block.ToString();
        }

        protected void Fill(object block, int length)
        {
            int len = blocks.Length + length;
            Array.Resize(ref blocks, len);
            for (int i = 1; i <= length; i++)
            {
                blocks[len - i] = block.ToString();
            }
        }
    }
}