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
        private const string Secret = "U5GKIjqusf2ekKLmVGaeRpQssSNMz8f5lG64gi4PhbU" + "=";
        private const string ClientCredentials = "client_credentials";
        private const string ApiBaseUri = "https://login.microsoftonline.com/ee8e24c2-a7cc-49f7-a6e8-a45ed941a0df/oauth2/token";
        private const string GraphUri = "https://graph.windows.net/ee8e24c2-a7cc-49f7-a6e8-a45ed941a0df/users?api-version=1.6";


        // GET api/values
        [HttpGet]
        [Route("ADUsers")]
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
                client.BaseAddress = new Uri(GraphUri);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(GraphUri);
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }

        }




        private static async Task<string[]> GetUserGroupsAsync(HttpClient client)
        {
            var payload = await client.GetStringAsync($"https://graph.microsoft.com/v1.0/users");
            var obj = JsonConvert.DeserializeObject<JObject>(payload);
            var groupDescription = from g in obj["value"]
                select g["displayName"].Value<string>();

            return groupDescription.ToArray();
        }
        private static async Task<string> AppAuthenticationAsync()
        {
            //  Constants
            var tenant = "ee8e24c2-a7cc-49f7-a6e8-a45ed941a0df";
            var clientID = "76aba709-8632-4832-84af-15972bdcbeea";
            var resource = "https://graph.microsoft.com/";
            var secret = "U5GKIjqusf2ekKLmVGaeRpQssSNMz8f5lG64gi4PhbU=";
            var grantType = "grant_type=client_credentials";

            //  Ceremony
            var authority = $"https://login.microsoftonline.com/{tenant}";
            var authContext = new AuthenticationContext(authority);
            var credentials = new ClientCredential(clientID, secret);

            var authResult = await authContext.AcquireTokenAsync(resource, credentials);

            return authResult.AccessToken;
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
                client.BaseAddress = new Uri(ApiBaseUri);

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
            var token = await GetToken();

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
