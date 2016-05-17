using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Pomelo.NetCore.Azure
{
    internal class Authenticator
    {
        private string _authorizationHdr;
        private DateTimeOffset _authorizationExpires = DateTimeOffset.MinValue;

        public string TenantId { get; set; }

        public string ClientId { get; set; }

        public string AppPassword { get; set; }

        /// <summary>
        /// Get access token.
        /// </summary>
        /// <returns></returns>
        public async Task GetAccessToken()
        {
            try
            {
                var authenticationContext = new AuthenticationContext("https://login.windows.net/" + TenantId);
                var credential = new ClientCredential(clientId: ClientId, clientSecret: AppPassword);
                var result = await authenticationContext.AcquireTokenAsync(resource: "https://management.core.windows.net/", clientCredential: credential);

                if (result == null)
                {
                    throw new InvalidOperationException("Failed to obtain the JWT token");
                }

                _authorizationHdr = result.CreateAuthorizationHeader();
                _authorizationExpires = result.ExpiresOn;
            }
            catch (NullReferenceException)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token (null ref)");
            }
        }


        /// <summary>
        /// Make a request to Azure Management Service.
        /// 
        /// POST content may not exceed 4GB, and must not be multipart or stream.
        /// </summary>
        /// <param name="method">Method of http request</param>
        /// <param name="uri">Designated Azure Management REST URI</param>
        /// <param name="mime">Mime type of POST content</param>
        /// <param name="requestContent">Byte array of POST content</param>
        /// <returns>Async AzureRESTResponse, i.e. StatusCode and Content</returns>
        internal async Task<AzureRESTResponse> Request(string method, Uri uri, string mime, byte[] requestContent)
        {
            if (DateTime.UtcNow > _authorizationExpires || _authorizationHdr == null)
            {
                await GetAccessToken();
            }

#if NET451
            var request = WebRequest.CreateHttp(uri) as HttpWebRequest;

            request.Method = method;
            request.ContentType = mime;
            request.Headers[HttpRequestHeader.Authorization] = _authorizationHdr;

            var reqStream = await request.GetRequestStreamAsync();
            reqStream.Write(requestContent, 0, requestContent.Length);

            HttpWebResponse response;
            try
            {
                response = await request.GetResponseAsync() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;
            }

            System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream());
            var content = await reader.ReadToEndAsync();
            response.Close();

            return new AzureRESTResponse { Content = content, StatusCode = response.StatusCode };
#else
            using (var handler = new HttpClientHandler())
            {
                using (var client = new HttpClient(handler))
                {
                    var httpContent = new ByteArrayContent(requestContent);
                    httpContent.Headers.ContentType.MediaType = mime;
                    var requestMessage = new HttpRequestMessage
                    {
                        Content = httpContent,
                        Method = new HttpMethod(method),
                        RequestUri = uri
                    };
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_authorizationHdr);
                    
                    var result = await client.SendAsync(requestMessage);
                    var content = await result.Content.ReadAsStringAsync();

                    return new AzureRESTResponse { Content = content, StatusCode = result.StatusCode };
                }
            }
#endif

        }

    }
}
