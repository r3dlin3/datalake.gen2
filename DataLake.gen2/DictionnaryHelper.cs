using System.Collections.Generic;
using System.Net.Http.Headers;

namespace DataLake.gen2
{
    public class DictionnaryHelper
    {
        public static void AddDefaultValue(Dictionary<string, string> dict, string key, string defaultValue)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, defaultValue);
            }
        }

        public static string GetDefaultValue(Dictionary<string, string> dict, string key, string defaultValue="")
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            } else {
                return defaultValue;
            }
        }

         public static string GetDefaultValue(HttpRequestHeaders dict, string key, string defaultValue="")
        {
            if (dict.Contains(key))
            {
                return string.Join(",", dict.GetValues(key));
            } else {
                return defaultValue;
            }
        }
    }
}