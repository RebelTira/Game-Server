/*
 _____   ___ __  __  _____ _____   ___  _  __              ___   ___   __    __ 
/__   \ /___\\ \/ /  \_   \\_   \ / __\( )/ _\            / __\ /___\ /__\  /__\
  / /\///  // \  /    / /\/ / /\// /   |/ \ \            / /   //  /// \// /_\  
 / /  / \_//  /  \ /\/ /_/\/ /_ / /___    _\ \          / /___/ \_/// _  \//__  
 \/   \___/  /_/\_\\____/\____/ \____/    \__/          \____/\___/ \/ \_/\__/  
__________________________________________________________________________________

Created by: ToXiiC
Thanks to: CodeDragon, Kill1212, Bubbleshit

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using MySql.Data.MySqlClient;

namespace Game_Server
{
    public class Statment : IDisposable
    {
        public string query;
        public readonly Dictionary<string, object> parameters = new Dictionary<string, object>();
        public Statment(string query)
        {
            this.query = query;
			//this.query = Database.Stripslash(query); // segun dice toxiic en el foro pero da error
        }

        public void AddValue(string key, object value)
        {
            if (!this.parameters.ContainsKey(key))
            {
                this.parameters.Add(key, value);
            }
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
    }
}
