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
using System.Diagnostics;

namespace Game_Server
{
    class Generic
    {
        private static Random r = new Random();
        public static int timestamp
        {
            get
            {
                TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
                return (int)(ts.TotalSeconds);
            }
        }

        public static string currentDate
        {
            get
            {
                return DateTime.Now.ToString("HH:mm:ss - dd/MM/yy");
            }
        }

        public static int random(int r1, int r2)
        {
            return r.Next(r1, r2);
        }

        public static string runningSince
        {
            get
            {
                TimeSpan ts = DateTime.Now - Process.GetCurrentProcess().StartTime;
                return "Running since " + ts.Days + " days, " + ts.Hours + " hours, " + ts.Minutes + " minutes!";
            }
        }

        public static string ReverseIP(string addr)
        {
            string[] c = addr.Split('.');
            Array.Reverse(c);
            return string.Join(".", c);
        }

        public static string runningSinceWeb
        {
            get
            {
                TimeSpan ts = DateTime.Now - Process.GetCurrentProcess().StartTime;
                return ts.Days + "d, " + ts.Hours + "h, " + ts.Minutes + "m";
            }
        }

        public static System.Drawing.Color ConvertHexToRGB(string color)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (color.StartsWith("#"))
            {
                color = color.Remove(0, 1);
            }

            if (color.Length == 3)
            {
                r = Convert.ToByte(color[0] + "" + color[0], 16);
                g = Convert.ToByte(color[1] + "" + color[1], 16);
                b = Convert.ToByte(color[2] + "" + color[2], 16);
            }
            else if (color.Length == 6)
            {
                r = Convert.ToByte(color[0] + "" + color[1], 16);
                g = Convert.ToByte(color[2] + "" + color[3], 16);
                b = Convert.ToByte(color[4] + "" + color[5], 16);
            }
            return System.Drawing.Color.FromArgb(r, g, b);
        }

        public static float ByteToFloat(byte[] packet, int offset)
        {
            byte[] value = new byte[4];
            Array.Copy(packet, offset, value, 0, 4);
            return BitConverter.ToSingle(value, 0);
        }

        public static int ByteToInteger(byte[] packet, int offset)
        {
            byte[] value = new byte[4];
            Array.Copy(packet, offset, value, 0, 4);
            return BitConverter.ToInt32(value, 0);
        }

        public static ushort ByteToUShort(byte[] packet, int offset)
        {
            byte[] value = new byte[2];
            Array.Copy(packet, offset, value, 0, 2);
            return BitConverter.ToUInt16(value, 0);
        }

        public static string convertToMD5(string Input)
        {
            try
            {
                System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(Input);
                byte[] hash = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int I = 0; I < hash.Length; I++)
                {
                    sb.Append(hash[I].ToString("x2"));
                }

                return sb.ToString();
            }
            catch { return null; }
        }

        public static int WarRockDateTime
        {
            get
            {
                int StartTime = int.Parse(String.Format("{0:yyMMddHH}", DateTime.Now));
                return StartTime;
            }
        }

        public static bool isMacAddress(string mac)
        {
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex("^([:xdigit:]){12}$");
            return !r.IsMatch(mac);
        }

        public static bool IsAlphaNumeric(string input)
        {
            System.Text.RegularExpressions.Regex objAlphaNumericPattern = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9]");
            return !objAlphaNumericPattern.IsMatch(input);
        }

        public static bool IsMD5Hash(string input)
        {
            System.Text.RegularExpressions.Regex objMD5HashPattern = new System.Text.RegularExpressions.Regex("[0-9a-f]{32}");
            return objMD5HashPattern.IsMatch(input);
        }
    }
}
