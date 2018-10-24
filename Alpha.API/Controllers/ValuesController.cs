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
        private const string ClientId = "b07ad4ff-803d-45c3-b76f-49f6fe8b5d94";
        private const string Resource = "https://graph.windows.net/";
        private const string Secret = "6KHTyTSOtSCAntXJ4jo8Kkka0LdRsD64+6UJQWeZJlg=";
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
    }
}