using NUnit.Framework;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DataLake.gen2
{
    public class SASHelperTests
    {
            private const string ACCOUNTNAME = "pocdlgen2";
            private const string ACCOUNTKEY = "xbDE/5G8BksH9DTumXdcQikZ3VpYKv21bac/v6i1puEIb48JlKJHvdoOYHyPoUt9gh0M7qAorlymo7WzU0isig==";
        HttpRequestMessage req;

        [SetUp]
        public void Setup()
        {
            req = new HttpRequestMessage();
        }

        [Test]
        public void TestGetcanonicalizedHeaders()
        {
            req.Headers.Add("Foo", "Bar");
            req.Headers.Add(Constants.HEADER_CLIENT_REQUEST_ID, "1234");
            req.Headers.Add(Constants.HEADER_VERSION, Constants.VERSION);
            req.Headers.Add(Constants.HEADER_DATE, "2019-06-30");

            string expected = Constants.HEADER_CLIENT_REQUEST_ID + ":1234\n"
                                + Constants.HEADER_DATE + ":2019-06-30\n"
                                + Constants.HEADER_VERSION + ":" + Constants.VERSION;

            Assert.AreEqual(expected, SASHelper.GetCanonicalizedHeaders(req));

        }

        [Test]
        public void TestGetCanonicalizedResource()
        {
            string accountname = "truc";
            req.RequestUri = new System.Uri("https://bar/root/test1/file1.txt?param1=1234&foo=bar");


            string expected = "/" + accountname + "/root/test1/file1.txt\n"
                + "foo:bar\n"
                + "param1:1234";

            Assert.AreEqual(expected, SASHelper.GetCanonicalizedResource(req, accountname));

        }

        [Test]
        public void TestPutSignature()
        {         
            req.RequestUri = new System.Uri("https://pocdlgen2.dfs.core.windows.net/root/test2.txt?action=append&position=0&timeout=901");
            req.Method = HttpMethod.Put;
            req.Headers.Add(Constants.HEADER_CLIENT_REQUEST_ID, "f65e7182-4ee2-4ba5-47b7-65bd5a88ddf2");
            req.Headers.Add(Constants.HEADER_VERSION, "2018-11-09");
            req.Headers.Add(Constants.HEADER_DATE, "Wed, 03 Jul 2019 18:10:45 GMT");
            req.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            req.Content = new StringContent("");
            req.Content.Headers.ContentLength = 3125;
            req.Content.Headers.ContentType = null;
            string expected = "pocdlgen2:CF/+ZvLYvSOy1oMtPt9r5Qu2H5Ob/OX8X6E5JU4vaTc=";
            Assert.AreEqual(
                expected,
                SASHelper.GetAuthorizationHeader(req, ACCOUNTNAME, ACCOUNTKEY)
            );

        }

        [Test]
        public void TestSignatureGetWithContentType()
        {
            req.RequestUri = new System.Uri("https://pocdlgen2.dfs.core.windows.net/root?directory=%2F&recursive=false&resource=filesystem&timeout=60");
            req.Method = HttpMethod.Get;
            //req.Headers.IfNoneMatch.Add(EntityTagHeaderValue.Any);
            req.Headers.Add(Constants.HEADER_CLIENT_REQUEST_ID, "9e18c0a0-b377-410d-8536-bb04df6df016");
            req.Headers.Add(Constants.HEADER_VERSION, "2018-11-09");
            req.Headers.Add(Constants.HEADER_DATE, "Wed, 03 Jul 2019 17:59:01 GMT");
            req.Content = new StringContent("",Encoding.UTF8,"application/json");
            string expected = "pocdlgen2:jK/nDdlhBjNhIGQMQ73P3t10CSIZnPBIPEYb4Dlmec0=";
            Assert.AreEqual(
                expected,
                SASHelper.GetAuthorizationHeader(req, ACCOUNTNAME, ACCOUNTKEY)
            );

        }
    }
}