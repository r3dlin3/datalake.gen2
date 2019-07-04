﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DataLake.gen2
{

    class Program
    {

        public static readonly string ACCOUNT_NAME = "pocdlgen2";
        public static readonly string ACCOUNT_KEY = "xbDE/5G8BksH9DTumXdcQikZ3VpYKv21bac/v6i1puEIb48JlKJHvdoOYHyPoUt9gh0M7qAorlymo7WzU0isig==";
        public static readonly string FILESYSTEM = "root";
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string path = "test3.txt";

            byte[] content = File.ReadAllBytes(ToApplicationPath("test1.txt"));
            var client = new DataLakeStorageClient(ACCOUNT_NAME, ACCOUNT_KEY, FILESYSTEM);
            var res = client.CreateFile(path).Result;
            Console.WriteLine("StatusCode=" + res.StatusCode);
            Console.WriteLine("StatusCode=" + res.ToString());
            Console.WriteLine("================================");
            Console.WriteLine("Uploading");
            res = client.Append(path, content, "text/csv").Result;
            Console.WriteLine("StatusCode=" + res.StatusCode);
            Console.WriteLine("StatusCode=" + res.ToString());
            Console.WriteLine("================================");
            Console.WriteLine("Flushing");
            res = client.Flush(path, content.Length).Result;
            Console.WriteLine("StatusCode=" + res.StatusCode);
            Console.WriteLine("StatusCode=" + res.ToString());


        }

////
// Courtesy of http://codebuckets.com/2017/10/19/getting-the-root-directory-path-for-net-core-applications/
////
        public static string ToApplicationPath(string fileName)
        {
            var exePath = Path.GetDirectoryName(System.Reflection
                                .Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher=new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            return Path.Combine(appRoot, fileName);
        }
    }
}