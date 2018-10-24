using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alpha.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        //  Constants
        private const string Tenant = "ee8e24c2-a7cc-49f7-a6e8-a45ed941a0df";
        private const string ClientId = "76aba709-8632-4832-84af-15972bdcbeea";
        private const string Resource = "https://graph.windows.net/";
        private const string Secret = "U5GKIjqusf2ekKLmVGaeRpQssSNMz8f5lG64gi4PhbU=";
        private const string ClientCredentials = "client_credentials";


        // GET api/values
        [HttpGet]
        [Route("user/ADUsers")]
        [Produces("application/json")]
        public ActionResult<string> GeAzureADtUsers()
        {
            var response = GetAllAdUsers().Result;
            return response;
        }
        private static async Task<string> GetToken()
        {
            using (var webClient = new WebClient())
            {
                var requestParameters = new NameValueCollection();
                requestParameters.Add("resource", Resource);
                requestParameters.Add("client_id", ClientId);
                requestParameters.Add("grant_type", ClientCredentials);
                requestParameters.Add("client_secret", Secret);

                var url = $"https://login.microsoftonline.com/{Tenant}/oauth2/token";
                var responsebytes = await webClient.UploadValuesTaskAsync(url, "POST", requestParameters);
                var responsebody = Encoding.UTF8.GetString(responsebytes);
                var obj = JsonConvert.DeserializeObject<JObject>(responsebody);
                var token = obj["access_token"].Value<string>();

                return token;
            }
        }
        private async Task<string> GetAllAdUsers()
        {
            var token = await GetToken();
            using (var client = new HttpClient())
            {
                var link = $"https://graph.windows.net/{Tenant}/users?api-version=1.6";
                client.BaseAddress = new Uri(link);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(link);
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }

        }

        [HttpGet]
        [Route("user/Index")]
        [Produces("application/json")]

        public async Task<ActionResult<string>> Index()
        {

            string clientId = ClientId;
            string clientSecret = Secret;

            var email = User.Identity.Name;

            AuthenticationContext authContext = new AuthenticationContext("https://login.windows.net/{Tenant}/oauth2/token");
            ClientCredential creds = new ClientCredential(clientId, clientSecret);
            AuthenticationResult authResult = await authContext.AcquireTokenAsync("https://graph.microsoft.com/", creds);

            HttpClient http = new HttpClient();
            string url = $"https://graph.microsoft.com/v1.0/users/{email}/$select=companyName";
            //url = "https://graph.windows.net/xxx.onmicrosoft.com/users?api-version=1.6";

            // Append the access token for the Graph API to the Authorization header of the request by using the Bearer scheme.
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            HttpResponseMessage response = await http.SendAsync(request);
            var json = response.Content.ReadAsStringAsync();

            return json.Result;
        }

    }
}