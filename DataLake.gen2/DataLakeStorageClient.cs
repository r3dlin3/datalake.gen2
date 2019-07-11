using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using DataLake.gen2.Extensions;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace DataLake.gen2
{
    public class DataLakeStorageClient
    {

        public string AccountName { get; set; }
        private string AccountKey;
        public string FileSystem { get; set; }
        private ActiveDirectoryServiceSettings AzureADServiceSettings;
        private AzureADSettings AzureADSettings;

        public DataLakeStorageClient(string accountName, string fileSystem, string accountKey)
        {
            this.AccountName = accountName;
            this.AccountKey = accountKey;
            this.FileSystem = fileSystem;
        }

        public DataLakeStorageClient(string accountName, string fileSystem, AzureADSettings azureADSettings)
        {
            this.AccountName = accountName;
            this.AzureADSettings = azureADSettings;
            AzureADServiceSettings = new ActiveDirectoryServiceSettings
            {
                AuthenticationEndpoint = new Uri(string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", azureADSettings.TenantId)),
                //TokenAudience = new Uri(string.Format("https://{0}{1}/", accountName, SUFFIX)),
                TokenAudience = new Uri("https://storage.azure.com"),
                ValidateAuthority = true
            };
            this.FileSystem = fileSystem;
        }
        private readonly HttpClient client = new HttpClient();
        private readonly HttpMethod PATCH = new HttpMethod("PATCH");

        public const string SUFFIX = ".dfs.core.windows.net";
        public async Task<HttpResponseMessage> CreateFile(string path)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("resource", "file");

            return await Send(HttpMethod.Put, path, null, queryParams);
        }

        public async Task<HttpResponseMessage> Append(string path, byte[] content, string contentType)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("action", "append");
            queryParams.Add("position", "0");

            return await Send(PATCH, path, content, queryParams, null, contentType);
        }

        public async Task<HttpResponseMessage> Flush(string path, long contentLength)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("action", "flush");
            queryParams.Add("position", contentLength.ToString());

            return await Send(PATCH, path, null, queryParams);
        }

        public async Task<HttpResponseMessage> Send(HttpMethod method, string path, byte[] content,
            Dictionary<string, string> queryParams = null, Dictionary<string, string> headers = null, string contentType = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            client.DefaultRequestHeaders.Accept.Clear();
            var req = new HttpRequestMessage(method, buildURL(path, queryParams));
            DictionnaryHelper.AddDefaultValue(headers, Constants.HEADER_VERSION, Constants.VERSION);
            DictionnaryHelper.AddDefaultValue(headers, Constants.HEADER_DATE, DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture));
            DictionnaryHelper.AddDefaultValue(headers, Constants.HEADER_CLIENT_REQUEST_ID, Guid.NewGuid().ToString());

            foreach (var item in headers)
            {
                req.Headers.Add(item.Key, item.Value);
            }
            if (content == null)
            {
                req.Content = new StringContent(string.Empty);
                req.Content.Headers.ContentLength = 0;
                req.Content.Headers.ContentType = null;
            }
            else
            {
                req.Content = new ByteArrayContent(content);
                req.Content.Headers.ContentLength = content.Length;
                req.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }

            if (null != this.AzureADSettings)
            {
                // cf. https://stackoverflow.com/a/54256816
                await ApplicationTokenProvider.LoginSilentAsync(
                AzureADSettings.TenantId,
                AzureADSettings.ServicePrincipalId,
                AzureADSettings.ServicePrincipalSecret,
                    AzureADServiceSettings,
                    TokenCache.DefaultShared);

                var token = TokenCache.DefaultShared.ReadItems()
                    .Where(t => t.ClientId == AzureADSettings.ServicePrincipalId)
                    .OrderByDescending(t => t.ExpiresOn)
                    .First();
                req.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token.AccessToken);
            }
            else
            {
                req.Headers.Authorization = new AuthenticationHeaderValue(
                    SASHelper.SCHEME,
                    SASHelper.GetAuthorizationHeader(req, AccountName, AccountKey));
            }

            showHeaders(req);
            return await client.SendAsync(req);
        }

        private Uri buildURL(string path, Dictionary<string, string> queryParams = null)
        {
            var builder = new UriBuilder()
            {
                Scheme = "https",
                Host = AccountName + SUFFIX,
                Path = FileSystem + "/" + path
            };
            var url = builder.Uri;
            if (queryParams != null)
            {
                url = url.AddQueryParameters(queryParams);
            }
            return url;

        }


        private void showHeaders(HttpRequestMessage req)
        {
            Console.WriteLine("Request to: " + req.RequestUri.ToString());
            req.Headers.ToList().ForEach(kv => Console.WriteLine(kv.Key + ": " + kv.Value));

        }

    }
}