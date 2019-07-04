using System;
using System.Collections.Generic;
using System.Web;

namespace DataLake.gen2.Extensions
{
    public static class HttpExtensions
    {
        public static Uri AddQueryParameter(this Uri uri, string name, string value)
        {
            var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

            httpValueCollection.Remove(name);
            httpValueCollection.Add(name, value);

            var ub = new UriBuilder(uri);
            ub.Query = httpValueCollection.ToString();

            return ub.Uri;
        }

        public static Uri AddQueryParameters(this Uri uri, Dictionary<string, string> queryParams)
        {
            var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);
            foreach (KeyValuePair<string, string> entry in queryParams)
            {
                httpValueCollection.Remove(entry.Key);
                httpValueCollection.Add(entry.Key, entry.Value);
            }

            var ub = new UriBuilder(uri);
            ub.Query = httpValueCollection.ToString();

            return ub.Uri;
        }
    }
}