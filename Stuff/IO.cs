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
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using System.Linq;

namespace Game_Server
{
    public static class IO
    {
        public static string path = null;
        public static string workingDirectory;

        public static string ReadValue(string section, string value)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(path);

                XmlNode mainNode = document.DocumentElement.SelectNodes(section + @"/" + value).Cast<XmlElement>().First();

                return mainNode.InnerText;
            }
            catch
            {
                Log.WriteError("Error while reading " + section + " [" + value + "]");
            }
            return "0";
        }

        public static string ReadAttribute(string section, string value, string subvalue)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(path);

                var elements = document.DocumentElement.SelectNodes(section + @"/" + value).Cast<XmlElement>().First().Attributes;

                /* Lets check if there is atleast an attribute */
                if (elements.Cast<XmlAttribute>().Where(r => string.Compare(r.Name, subvalue, true) == 0).Count() > 0)
                {
                    /* Return attribute */
                    return elements.Cast<XmlAttribute>().Where(r => string.Compare(r.Name, subvalue, true) == 0).FirstOrDefault().Value;
                }
            }
            catch
            {
                Log.WriteError("Error while reading " + section + " [" + value + " _ " + subvalue + "]");
            }
            return null;
        }
    }
}
