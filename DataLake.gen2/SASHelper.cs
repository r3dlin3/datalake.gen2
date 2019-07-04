using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Linq;

namespace DataLake.gen2
{
    public class SASHelper
    {
        public static string SCHEME = "SharedKey";
        public static string GetAuthorizationHeader(HttpRequestMessage req, string accountName, string accountKey)
        {
            return string.Format("{0}:{1}",
                accountName,
                GetSharedAccessSignature(req, accountName, accountKey));

        }
        public static string GetSharedAccessSignature(HttpRequestMessage req, string accountName, string accountKey)
        {
            string verb = req.Method.Method.ToUpper();
            string contentEncoding = req.Content?.Headers?.ContentEncoding?.ToString() ?? "";
            var languages = req.Content?.Headers?.ContentLanguage ?? new string[0];
            string contentLanguage = string.Join(",", languages);
            string contentLength = req.Content?.Headers?.ContentLength?.ToString() ?? "";
            string contentMD5 = req.Content?.Headers?.ContentMD5?.ToString() ?? "";
            string contentType = req.Content?.Headers?.ContentType?.ToString() ?? "";
            string date = req.Headers.Date?.ToString("R") ?? "";
            //DictionnaryHelper.GetDefaultValue(req.Headers, Constants.HEADER_DATE);
            string ifModifiedSince = req.Headers.IfModifiedSince?.ToString() ?? "";
            string ifMatch = req.Headers.IfMatch?.ToString() ?? "";
            string ifNoneMatch = req.Headers.IfNoneMatch?.ToString() ?? "";
            string ifUnmodifiedSince = req.Headers.IfUnmodifiedSince?.ToString() ?? "";
            string canonicalizedHeaders = GetCanonicalizedHeaders(req);
            string canonicalizedResource = GetCanonicalizedResource(req, accountName);

            return GetSharedAccessSignature(
                    accountKey,
                    verb,
                    contentEncoding,
                    contentLanguage,
                    contentLength,
                    contentMD5,
                    contentType,
                    date,
                    ifModifiedSince,
                    ifMatch,
                    ifNoneMatch,
                    ifUnmodifiedSince,
                    canonicalizedHeaders,
                    canonicalizedResource);
        }

        public static string GetCanonicalizedResource(HttpRequestMessage req, string accountName)
        {
            StringBuilder sb = new StringBuilder("/");
            sb.Append(accountName);
            sb.Append(req.RequestUri.LocalPath);
            NameValueCollection queryParams = HttpUtility.ParseQueryString(req.RequestUri.Query);
            var queryParamsList = new List<string>();

            foreach (string key in queryParams)
            {
                
                queryParamsList.Add(
                    string.Format("\n{0}:{1}",
                        key.ToLower(),
                        WebUtility.UrlDecode(queryParams[key])));
            }
            queryParamsList.Sort();
            sb.AppendJoin("",queryParamsList);

            return sb.ToString();
        }

        public static string GetCanonicalizedHeaders(HttpRequestMessage req)
        {
            // Acquire keys and sort them.
            var list = new List<string>();

            foreach (var header in req.Headers)
            {
                string key = header.Key.ToLower();
                if (key.StartsWith("x-ms-"))
                {
                    list.Add(
                        string.Format("{0}:{1}", key, string.Join(",", header.Value).Trim())
                    );
                }
            }
            list.Sort();
            return string.Join("\n", list);

        }

        public static string GetSharedAccessSignature(
            string accountKey,
            string verb,
            string contentEncoding = "",
            string contentLanguage = "",
            string contentLength = "",
            string contentMD5 = "",
            string contentType = "",
            string date = "",
            string ifModifiedSince = "",
            string ifMatch = "",
            string ifNoneMatch = "",
            string ifUnmodifiedSince = "",
            string canonicalizedHeaders = "",
            string canonicalizedResource = ""
        )
        {
            // empty string if 0
            if (contentLength=="0") {
                contentLength = string.Empty;
            }

            string stringToSign = string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\n{8}\n{9}\n{10}\n\n{11}\n{12}",
                verb, contentEncoding, contentLanguage, contentLength, contentMD5, contentType, date,
                ifModifiedSince, ifMatch, ifNoneMatch, ifUnmodifiedSince, canonicalizedHeaders, canonicalizedResource);

            Console.WriteLine("stringToSign=");
            Console.WriteLine(stringToSign);

            string signature = Convert.ToBase64String(
                    GetHMACSHA256(
                        Encoding.UTF8.GetBytes(stringToSign),
                        Convert.FromBase64String(accountKey)));
            return signature;
        }

        public static byte[] GetHMACSHA256(byte[] content, byte[] keyBytes)
        {

            byte[] hashBytes;

            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(content);

            return hashBytes;
        }
    }
}