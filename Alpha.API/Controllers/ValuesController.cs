using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
        private static IOptions<ConfigurationSetting> _appSettings;

        public ValuesController(IOptions<ConfigurationSetting> appSettings)
        {
            _appSettings = appSettings;
        }

        private static async Task<string> GetToken()
        {
            using (var webClient = new WebClient())
            {
                //var test = _configuration.GetValue<string>("AzureAd:ClientId");
                var clientId = _appSettings.Value.ClientId;
                var contentType = _appSettings.Value.ContentType;
                var accept = _appSettings.Value.Accept;
                var clientCredentials = _appSettings.Value.ClientCredentials;
                var clientSecret = _appSettings.Value.ClientSecret;
                var resource = _appSettings.Value.Resource;
                var tenantId = _appSettings.Value.TenantId;



                var requestParameters = new NameValueCollection();
                requestParameters.Add("resource", resource);
                requestParameters.Add("client_id", clientId);
                requestParameters.Add("grant_type", clientCredentials);
                requestParameters.Add("client_secret", clientSecret);
                requestParameters.Add("Content-Type", contentType);
                requestParameters.Add("Accept", accept);

                var url = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";
                var responsebytes = await webClient.UploadValuesTaskAsync(url, "POST", requestParameters);
                var responsebody = Encoding.UTF8.GetString(responsebytes);
                var obj = JsonConvert.DeserializeObject<JObject>(responsebody);
                var token = obj["access_token"].Value<string>();

                return token;
            }
        }

        [HttpGet]
        [Route("user/AzureAdUsers")]
        public async Task<object> GetAllAdUsers()
        {
            var token = await GetToken();
            using (var client = new HttpClient())
            {
                var link = $"https://graph.microsoft.com/v1.0/users";
                client.BaseAddress = new Uri(link);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = client.GetAsync(link);
                var resultContent = await response.Result.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject(resultContent);

                return result;
            }

        }

        [HttpGet]
        [Route("user/AzureCurrentUser")]
        public async Task<object> GetCurrentUser()
        {
            var token = await GetToken();
            using (var client = new HttpClient())
            {
                ClaimsIdentity identity = this.User.Identity as ClaimsIdentity;
                string objectId = identity.Claims.FirstOrDefault(x => x.Type ==  "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

                var link = $"https://graph.microsoft.com/v1.0/users?$filter=id eq '{objectId}'&$select= "+"{'mail'}";

                client.BaseAddress = new Uri(link);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = client.GetAsync(link);
                var resultContent = await response.Result.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject(resultContent);

                return result;
            }

        }


    }


}