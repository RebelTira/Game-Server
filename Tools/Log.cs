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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Game_Server
{
    /// <summary>
    /// This class is used to print out logs in console
    /// </summary>
    class Log
    {
        private static object writeObj = new object();
        private static StreamWriter LogFile;

        /// <summary>
        /// Set up the text log file
        /// </summary>
        /// <param name="logFile"></param>
        /// <returns>The return value its boolean so it can be true or false</returns>
        public static bool setup(string logFile)
        {
            /*Console.WindowHeight = Console.LargestWindowHeight - 25;
            Console.WindowWidth = Console.LargestWindowWidth - 25;*/

            try
            {
                LogFile = new StreamWriter(logFile, true); //true to append data to the file; false to overwrite the file.
                LogFile.WriteLine("/* Empezamos: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " */");
                LogFile.WriteLine("");
                LogFile.Flush();
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Write a normal line (used for a common log view)
        /// </summary>
        /// <param name="str">Message</param>
        public static void WriteLine(string str)
        {
            writeline(str, ConsoleColor.DarkGreen);
        }

        /// <summary>
        /// Log out an error into the console, it will be logged also in the .txt file
        /// </summary>
        /// <param name="str">Message</param>
        public static void WriteError(string str)
        {
            writeline(str, ConsoleColor.DarkRed);

            if (LogFile != null)
            {
                DateTime _DTN = DateTime.Now;
                StackFrame _SF = new StackTrace().GetFrame(2);
                //LogFile.WriteLine("[" + _DTN.ToShortDateString() + " " + _DTN.ToLongTimeString() + "] [" + _SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name + "] » " + str);
                LogFile.WriteLine( _SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name + "] » " + str);
                LogFile.Flush();
            }
        }
        
        /// <summary>
        /// You can use this function in case you need to output some log just for debugging
        /// </summary>
        /// <param name="str">Message</param>
        public static void WriteDebug(string str)
        {
            writeline(str, ConsoleColor.DarkMagenta);

            if (LogFile != null)
            {
                DateTime _DTN = DateTime.Now;
                StackFrame _SF = new StackTrace().GetFrame(2);
                //LogFile.WriteLine("[" + _DTN.ToShortDateString() + " " + _DTN.ToLongTimeString() + "] [" + _SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name + "] » " + str);
                LogFile.WriteLine( _SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name + "] » " + str);
                LogFile.Flush();
            }
        }

        /// <summary>
        /// Output blank lines
        /// </summary>
        /// <param name="count">Count of the blan lines</param>
        public static void WriteBlank(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine("");
            }
        }
        //>--- +++              ----------------------   Añado esto para mensajes de info
        public static void WriteInfo(string str)
        {
            writeline(str, ConsoleColor.DarkCyan);
            if (LogFile != null)
            {
                DateTime _DTN = DateTime.Now;
                StackFrame _SF = new StackTrace().GetFrame(2);
                //LogFile.WriteLine("[" + _DTN.ToShortDateString() + " " + _DTN.ToLongTimeString() + "] [" + _SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name + "] » " + str);
                LogFile.WriteLine( _SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name + " » " + str);
                LogFile.Flush();
            }
        }
        //>--- ----------------------------
        private static void writeline(string str, ConsoleColor c)
        {
            lock (writeObj)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                //Console.Write("[" + DateTime.Now.ToString("hh:mm:ss:fff - dd/MM/yyyy") + "] > ");
                Console.ForegroundColor = c;
                Console.Write(str);
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        internal static void WriteFile(string outlog)
        {
            if (LogFile != null)
            {
                DateTime _DTN = DateTime.Now;
                StackFrame _SF = new StackTrace().GetFrame(2);
                //LogFile.WriteLine("[" + _DTN.ToShortDateString() + " " + _DTN.ToLongTimeString() + "] [" + _SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name + "] » " + outlog);
                LogFile.WriteLine( _SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name + "] » " + outlog);
                LogFile.Flush();
            }
        }
    }
}
