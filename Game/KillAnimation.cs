using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class SP_KillAnimation : Packet
    {
        public enum Type : ushort
        {
            FirstKill = 0, // 3 points
            SecondKill = 1, // 3 points
            RevengeKill = 2, // 3 points 
            Assasin = 3, // 2 points

            GrenadeKill = 4, // 1 point
            HeadShot = 5, // 1 point

            DoubleKill = 6, // 1 point
            TripleKill = 7, // 1 point 
            QuadraKill = 8, // 2 points
            PentaKill = 9, // 2 points
            HexaKill = 10, // 3 points
            UltraKill = 11, // 3 points
            
            Unknown            
        }
        public SP_KillAnimation(Type opcode) //>--- FirstKill, DoubleKill, ...
        {
            newPacket(31510);
            addBlock((ushort)opcode);
            addBlock(0);
        }
    }
}
