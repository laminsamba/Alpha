﻿using System;
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
        private readonly string _resourceId;
        private readonly string _tenantid;
        private readonly string _authority;
        private readonly string _appId;
        private readonly string _grant_type;
        private readonly string _appSecret;
        private readonly HttpClient _client;
        private bool tokenSet = false;

        private string apiBaseUri = "https://login.microsoftonline.com/ee8e24c2-a7cc-49f7-a6e8-a45ed941a0df/oauth2/token";
        private string graphUri = "https://graph.windows.net/ee8e24c2-a7cc-49f7-a6e8-a45ed941a0df/users?api-version=1.6";

        // GET api/values
        [HttpGet]
        public ActionResult<string[]> Get()
        {
            var response = GetAllAdUsers().Result;
            return response;
        }


        private async Task<string[]> GetAllAdUsers()
        {
            var token = await AppAuthenticationAsync();

            using (var client = new HttpClient())
            {
                //setup client
                client.BaseAddress = new Uri(graphUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", token);

                //make request
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                var responseString = await response.Content.ReadAsStringAsync();
            //    return responseString;
              //  var content = await users.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string[]>(responseString);
            }

        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
            // For more information on protecting this API from Cross Site Request Forgery (CSRF) attacks, see https://go.microsoft.com/fwlink/?LinkID=717803
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            // For more information on protecting this API from Cross Site Request Forgery (CSRF) attacks, see https://go.microsoft.com/fwlink/?LinkID=717803
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            // For more information on protecting this API from Cross Site Request Forgery (CSRF) attacks, see https://go.microsoft.com/fwlink/?LinkID=717803
        }

        //********************************

        private static async Task<string> AppAuthenticationAsync()
        {
            //  Constants
            var tenant = "ee8e24c2-a7cc-49f7-a6e8-a45ed941a0df";
            var clientID = "76aba709-8632-4832-84af-15972bdcbeea";
            var resource = "https://graph.microsoft.com/";
            var secret = "U5GKIjqusf2ekKLmVGaeRpQssSNMz8f5lG64gi4PhbU=";

            //  Ceremony
            var authority = $"https://login.microsoftonline.com/{tenant}";
            var authContext = new AuthenticationContext(authority);
            var credentials = new ClientCredential(clientID, secret);
            var authResult = await authContext.AcquireTokenAsync(resource, credentials);

            return authResult.AccessToken;
        }

        private static async Task<string> HttpAppAuthenticationAsync()
        {
            //  Constants
            var tenant = "ee8e24c2-a7cc-49f7-a6e8-a45ed941a0df";
            var clientID = "76aba709-8632-4832-84af-15972bdcbeea";
            var resource = "https://graph.microsoft.com/";
            var secret = "2NWSiSynjhpZhUFrB1OaNrBx80VpigTi6njgSRZ0QoQ=";

            using (var webClient = new WebClient())
            {
                var requestParameters = new NameValueCollection();

                requestParameters.Add("resource", resource);
                requestParameters.Add("client_id", clientID);
                requestParameters.Add("grant_type", "client_credentials");
                requestParameters.Add("client_secret", secret);

                var url = $"https://login.microsoftonline.com/{tenant}/oauth2/token";
                var responsebytes = await webClient.UploadValuesTaskAsync(url, "POST", requestParameters);
                var responsebody = Encoding.UTF8.GetString(responsebytes);
                var obj = JsonConvert.DeserializeObject<JObject>(responsebody);
                var token = obj["access_token"].Value<string>();

                return token;
            }
        }

        private async Task<string> GetAccessToken()
        {
            var tenant = "ee8e24c2-a7cc-49f7-a6e8-a45ed941a0df";
            var clientID = "76aba709-8632-4832-84af-15972bdcbeea";
            var resource = "https://graph.microsoft.com/";
            var secret = "client_credentials";
            var grant_type = "2NWSiSynjhpZhUFrB1OaNrBx80VpigTi6njgSRZ0QoQ=";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiBaseUri);

                // We want the response to be JSON.
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Build up the data to POST.
                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("grant_type", grant_type));
                postData.Add(new KeyValuePair<string, string>("client_id", clientID));
                postData.Add(new KeyValuePair<string, string>("client_secret", secret));

                FormUrlEncodedContent content = new FormUrlEncodedContent(postData);

                // Post to the Server and parse the response.
                HttpResponseMessage response = await client.PostAsync("Token", content);
                string jsonString = await response.Content.ReadAsStringAsync();
                object responseData = JsonConvert.DeserializeObject(jsonString);

                // return the Access Token.
                return ((dynamic)responseData).access_token;
            }
        }


        private static async Task<bool> DoesUserExistsAsync(HttpClient client, string user)
        {
            try
            {
                var payload = await client.GetStringAsync($"https://graph.microsoft.com/v1.0/users/{user}");

                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        private static async Task<string[]> GetUserGroupsAsync(HttpClient client, string user)
        {
            var payload = await client.GetStringAsync(
                $"https://graph.microsoft.com/v1.0/users/{user}/memberOf");
            var obj = JsonConvert.DeserializeObject<JObject>(payload);
            var groupDescription = from g in obj["value"]
                                   select g["displayName"].Value<string>();

            return groupDescription.ToArray();
        }

        private static async Task CreateUserAsync(HttpClient client, string user, string domain)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                var payload = new
                {
                    accountEnabled = true,
                    displayName = user,
                    mailNickname = user,
                    userPrincipalName = $"{user}@{domain}",
                    passwordProfile = new
                    {
                        forceChangePasswordNextSignIn = true,
                        password = "tempPa$$w0rd"
                    }
                };
                var payloadText = JsonConvert.SerializeObject(payload);

                writer.Write(payloadText);
                writer.Flush();
                stream.Flush();
                stream.Position = 0;

                using (var content = new StreamContent(stream))
                {
                    content.Headers.Add("Content-Type", "application/json");

                    var response = await client.PostAsync("https://graph.microsoft.com/v1.0/users/", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(response.ReasonPhrase);
                    }
                }
            }
        }

        private static async Task Test()
        {
            //var token = await AppAuthenticationAsync();
            var token = await HttpAppAuthenticationAsync();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var user = "test@LdapVplDemo.onmicrosoft.com";
                var userExist = await DoesUserExistsAsync(client, user);

                Console.WriteLine($"Does user exists?  {userExist}");

                if (userExist)
                {
                    var groups = await GetUserGroupsAsync(client, user);

                    foreach (var g in groups)
                    {
                        Console.WriteLine($"Group:  {g}");
                    }

                    await CreateUserAsync(client, "newuser", "LdapVplDemo.onmicrosoft.com");
                }
            }
        }


    }
}
