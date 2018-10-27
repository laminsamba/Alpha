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
using Microsoft.AspNetCore.Http;
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
        private const string Resource = "https://graph.microsoft.com/";
        private const string ClientSecret = "HUkF4xqEcL2C/HkFlFQ1e2BLmG/PFB8kaMM0TsrttpU=";
        private const string ClientCredentials = "client_credentials";
        private const string ContentType = "application/json";
        private const string Accept = "application/json";

        private static async Task<string> GetToken()
        {
            using (var webClient = new WebClient())
            {
                var requestParameters = new NameValueCollection();
                requestParameters.Add("resource", Resource);
                requestParameters.Add("client_id", ClientId);
                requestParameters.Add("grant_type", ClientCredentials);
                requestParameters.Add("client_secret", ClientSecret);
                requestParameters.Add("Content-Type","application/json");
                requestParameters.Add("Accept", Accept);

                var url = $"https://login.microsoftonline.com/{Tenant}/oauth2/token";
                var responsebytes = await webClient.UploadValuesTaskAsync(url, "POST", requestParameters);
                var responsebody = Encoding.UTF8.GetString(responsebytes);
                var obj = JsonConvert.DeserializeObject<JObject>(responsebody);
                var token = obj["access_token"].Value<string>();

                return token;
            }
        }

        // Invalid 'HttpContent' instance provided. It does not have a content type header with a value of 'application/http; msgtype=response'.
        //    Parameter name: content

        [HttpGet]
        [Route("user/AzureAdUsers")]
        public async Task<object> GetAllAdUsers()
        {
            var token = await GetToken();
            using (var client = new HttpClient())
            {
                try
                {
                    var link = $"https://graph.microsoft.com/v1.0/users";
                    client.BaseAddress = new Uri(link);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = client.GetAsync(link);
                    
                    //response.Result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/http");
                    //response.Result.Content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("msgtype", "response"));

                    var resultContent = await response.Result.Content.ReadAsStringAsync();

                    var model = JsonConvert.SerializeObject(resultContent);
                    var donet = JsonConvert.DeserializeObject(resultContent);

                    return donet;
                }
                catch (Exception ex)
                {
                    throw ex;

                }
            }

        }
    }

    public class SentimentJsonModel
    {
        public string Email { get; set; }   
    }
}