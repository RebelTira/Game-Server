using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;

namespace Game_Server.Managers
{
    class WordFilter
    {
        public string normal, replace;
        public WordFilter(string normal, string replace)
        {
            this.normal = normal;
            this.replace = replace;
        }
    }

    class WordFilterManager
    {
        public static List<WordFilter> filters = new List<WordFilter>();
        public static void Load()
        {
            filters.Clear();
            DataTable dt = DB.RunReader("SELECT * FROM wordfilter");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row != null)
                {
                    string normal = row["normal"].ToString();
                    string replace = row["replace"].ToString();
                    WordFilter wf = new WordFilter(normal, replace);
                    filters.Add(wf);
                }
            }
            Log.WriteLine("Successfully loaded [" + filters.Count + "] Word Filters");
        }

        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        public static string Replace(string text)
        {
            string[] s = text.Split(' ');
            for (int i = 0; i < s.Length; i++)
            {
                s[i] = ReplaceWord(s[i]);
            }
            return string.Join(" ", s);
        }

        private static string ReplaceWord(string p)
        {
            foreach (WordFilter f in filters)
            {
                if (RemoveSpecialCharacters(p.ToLower()) == RemoveSpecialCharacters(f.normal.ToLower()))
                {
                    return f.replace;
                }
            }
            return p;
        }
    }
}
