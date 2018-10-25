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
        private const string Resource = "https://graph.microsoft.com/";
        private const string ClientSecret = "rGxUT0qRvbXUtEkFm/HZBQCQq5zg2fq15sqZLLs04GY=";
        private const string ClientCredentials = "client_credentials";
        private const string ContentType = "application/json";
        private const string Accept = "application/json";

        [HttpGet]
        [Route("AzureAdUsers")]
        public string GetAzureAdUsers()
        {
            var response = GetAllAdUsers().Result;
            return response;
        }

        [HttpGet]
        [Route("AzureAdUserByName")]
        //public string GetAzureAdUserByName(string name)
        //{
        //    var response = GetAllAdUsersByName(name).Result;
        //    return response;
        //}
        private async Task<string> GetToken()
        {
            using (var webClient = new WebClient())
            {
                var requestParameters = new NameValueCollection();
                requestParameters.Add("resource", Resource);
                requestParameters.Add("client_id", ClientId);
                requestParameters.Add("grant_type", ClientCredentials);
                requestParameters.Add("client_secret", ClientSecret);
                requestParameters.Add("Content-Type", ContentType);

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
                var link = $"https://graph.microsoft.com/v1.0/users?$select=displayName,givenName,mail&$format=json";
                client.BaseAddress = new Uri(link);
         //       client.DefaultRequestHeaders.Clear();
             //   client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Accept));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync(link);
                var responseString = await response.Content.ReadAsStringAsync();

                return responseString;
            }

        }
        //private async Task<string> GetAllAdUsersByName(string name)
        //{
        //    var token = await GetToken();
        //    using (var client = new HttpClient())
        //    {
        //        var link = $"https://graph.microsoft.com/v1.0/users?$filter=startswith(displayName,{name})&$format=json";
        //        client.BaseAddress = new Uri(link);
        //        client.DefaultRequestHeaders.Clear();
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //        var response = await client.GetAsync(link);
        //        var responseString = await response.Content.ReadAsStringAsync();

        //        return responseString;
        //    }

        //}
    }
}