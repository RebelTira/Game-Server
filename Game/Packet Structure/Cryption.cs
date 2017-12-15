using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sniffer;

namespace Game_Server.Game
{
    /// <summary>
    /// This class is used for crypt/uncrypt packets or MD5
    /// </summary>
    class Cryption
    {
        public const byte clientXor = 0x3E; // 0x3E-OK  62; // Original = 10 // Antes 0x1E 30
        public const byte serverXor = 0x60; // 0x60-OK  96; // Original = 91 // Antes 0x1B 27

        public static byte[] encrypt(byte[] inputByte)
        {
            for (int i = 0; i < inputByte.Length; i++)
            {
                inputByte[i] ^= serverXor;
            }

            return inputByte;
        }
        public static byte[] decrypt(byte[] inputByte)
        {


            for (int i = 0; i < inputByte.Length; i++)
            {
                inputByte[i] ^= clientXor;
            }

            return inputByte;
        }
    }
}
