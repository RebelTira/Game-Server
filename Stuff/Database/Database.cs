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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using MySql.Data.MySqlClient;

namespace Game_Server
{
    public static class DB
    {
        private static string strConnection;

        static void dbConnection_StateChange(object usr, StateChangeEventArgs ev)
        {
            if (ev.CurrentState == ConnectionState.Broken)
            {
                System.Threading.Thread.Sleep(1000);
                dbConnection.Close();
            }

            if (ev.CurrentState == ConnectionState.Closed)
            {
                System.Threading.Thread.Sleep(1000);

                Log.WriteLine("Reconnecting to SQL Server 1...");
                dbConnection = new MySqlConnection(strConnection);
                dbConnection.StateChange += new System.Data.StateChangeEventHandler(dbConnection_StateChange);
                System.Threading.Thread.Sleep(2000);
                dbConnection.Open();
                Log.WriteLine("Reconnection to database 1 successful.");
            }
        }

        private static MySqlConnection dbConnection;
        #region Database connection management
        /// <summary> 
        /// Opens connection to the MySQL database with the supplied parameters, and returns a 'true' boolean when the connection has succeeded. Requires MySQL MySql 5.1 driver to be installed. 
        /// </summary> 
        /// <param name="dbHost">The hostname/IP address where the database server is located.</param> 
        /// <param name="dbPort">The port the database server is running on.</param> 
        /// <param name="dbName">The name of the data</param> 
        /// <param name="dbUsername">The username for authentication with the data</param> 
        /// <param name="dbPassword">The pasword for authentication with the data</param> 
        public static bool openConnection(string dbHost, int dbPort, string dbName, string dbUsername, string dbPassword, int dbPoolsize)
        {
            try
            {
                Log.WriteLine("Connecting to " + dbName + " at " + dbHost + ":" + dbPort + " for user '" + dbUsername + "'");
                strConnection = "Server=" + dbHost + ";Port=" + dbPort + ";Database=" + dbName + ";User=" + dbUsername + ";Password=" + dbPassword + ";Connection Timeout=99999999;";
                dbConnection = new MySqlConnection(strConnection);
                dbConnection.StateChange += new System.Data.StateChangeEventHandler(dbConnection_StateChange);
                dbConnection.Open();
                if (dbConnection.State == ConnectionState.Open)
                {
                    Log.WriteLine("Connection to database successfull.");
                    return true;
                }
                else
                {
                    Log.WriteError("Failed to connect to " + dbName + " at " + dbHost + ":" + dbPort + " for user '" + dbUsername + "'");
                    return false;
                }
            }

            catch (Exception ex)
            {
                Log.WriteError("Failed to connect! Error thrown was: " + ex.Message);
                return false;
            }
        }
        /// <summary> 
        /// Closes connection with the MySQL data Any errors are ignored. 
        /// </summary> 
        public static void closeConnection()
        {
            Log.WriteLine("Closing database connection...");
            try
            {
                dbConnection.Close();
                Log.WriteLine("Database connection closed.");
            }
            catch { Log.WriteError("No database connection."); }
        }
        #endregion

        #region Database data manipulation

        public static void RunQuery(string query)
        {
            QueryAsync(new Statment(query));
        }
        
        struct QueryObject<T>
        {
            public Action<T> OriginalCallback;
            public MySqlCommand MySQLCommand;

            public QueryObject(Action<T> callback, MySqlCommand cmd)
            {
                this.OriginalCallback = callback;
                this.MySQLCommand = cmd;
                this.MySQLCommand.CommandTimeout = 0; // No timeout
            }
        }

        private static void QueryAsync(Statment statement)
        {
            MySqlConnection c = new MySqlConnection(strConnection);
            c.Open();
            MySqlCommand cmd = c.CreateCommand();
            cmd.CommandTimeout = 0; // No timeout
            cmd.CommandText = statement.query;
            foreach (KeyValuePair<string, object> entry in statement.parameters)
                cmd.Parameters.AddWithValue(entry.Key, entry.Value);
            cmd.Prepare();
            cmd.BeginExecuteNonQuery(new AsyncCallback(QueryAsyncCallback), new QueryObject<bool>(null, cmd));
            statement.Dispose();
        }

        private static void QueryAsyncCallback(IAsyncResult iAr)
        {
            QueryObject<bool> qo = (QueryObject<bool>)iAr.AsyncState;
            try
            {
                MySqlConnection c = null;
                using (MySqlCommand cmd = qo.MySQLCommand)
                {
                    cmd.CommandTimeout = 0; // No timeout
                    cmd.EndExecuteNonQuery(iAr);
                    c = cmd.Connection;
                }

                if (c != null)
                    c.Dispose();
                if (qo.OriginalCallback != null)
                    qo.OriginalCallback(true);
            }
            catch (Exception ex)
            {
                if (qo.OriginalCallback != null)
                    qo.OriginalCallback(false);
                Log.WriteError("Error while executing query: " + ex.Message);
            }
        }

        public static void RunQueryNotAsync(string query)
        {
            Query(new Statment(query));
        }

        private static void Query(Statment statement)
        {
            using (MySqlConnection c = new MySqlConnection(strConnection))
            {
                c.Open();
                using (MySqlCommand cmd = c.CreateCommand())
                {
                    cmd.CommandTimeout = 0; // No timeout
                    cmd.CommandText = statement.query;
                    foreach (KeyValuePair<string, object> entry in statement.parameters)
                        cmd.Parameters.AddWithValue(entry.Key, entry.Value);
                    try
                    {
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteError("Error while executing query: " + ex.Message);
                    }
                    statement.Dispose();
                }
            }
        }
        #endregion

        #region Database data retrieval
        #region RunReader
        /// <summary> 
        /// Performs a SQL query and returns the first selected field as string. Other fields are ignored. 
        /// </summary> 
        /// <param name="Query">The SQL query that selects a field.</param> 
        public static DataTable RunReader(string Query)
        {
            return Read(new Statment(Query));
        }

        public static object RunReaderOnce(string var, string Query)
        {
            try
            {
                DataTable dt = Read(new Statment(Query + " LIMIT 1"));
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return row[var].ToString();
                }
            }
            catch (Exception ex)
            {
                Log.WriteError("Error while executing query: " + ex.Message);
            }
            return new object();
        }

        private static DataTable Read(Statment statement)
        {
            DataTable retVal = new DataTable();
            using (MySqlConnection c = new MySqlConnection(strConnection))
            {
                c.Open();
                using (MySqlCommand cmd = c.CreateCommand())
                {
                    cmd.CommandTimeout = 0; // No timeout
                    cmd.CommandText = statement.query;
                    foreach (KeyValuePair<string, object> entry in statement.parameters)
                        cmd.Parameters.AddWithValue(entry.Key, entry.Value);
                    cmd.Prepare();
                    try
                    {
                        retVal.Load(cmd.ExecuteReader());
                    }
                    catch (Exception ex)
                    {
                        Log.WriteError("Error while executing query: " + ex.Message);
                    }
                    statement.Dispose();
                }
            }
            return retVal;
        }
        #endregion
        #endregion

        #region Data availability checks
        /// <summary> 
        /// Tries to find fields matching the query. When there is at least one result, it returns True and stops. 
        /// </summary> 
        /// <param name="Query">The SQL query that contains the seeked fields and conditions. LIMIT 1 is added.</param> 
        public static bool checkExists(string Query)
        {
            try
            {
                using (MySqlConnection c = new MySqlConnection(strConnection))
                {
                    c.Open();
                    using (MySqlCommand cmd = new MySqlCommand(Query + " LIMIT 1", c))
                    {
                        return cmd.ExecuteReader().HasRows;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteError("Error '" + ex.Message + "' at '" + Query + "'");
                return false;
            }
        }
        #endregion

        #region Misc
        /// <summary> 
        /// Returns a stripslashed copy of the input string.
        /// </summary> 
        /// <param name="Query">The string to add stripslashes to.</param>
        public static string Stripslash(string Query)
        {
            return Query.Replace(@"\", "\\").Replace("'", "\'").Replace("'", @"\'");
        }
        #endregion
    }
}
