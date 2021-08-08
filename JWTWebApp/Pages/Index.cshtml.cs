using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JWTWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public string Token { get; set; }
        public string UserId { get; set; }
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet()
        {
            using (var client = new HttpClient())
            {
                #region FormContentForPostRequest
                var content = new FormUrlEncodedContent(new[]
               {
                    new KeyValuePair<string,string>("username","developer"),
                    new KeyValuePair<string,string>("password","developerRR2021")
                }
               );
                #endregion


                #region GetTokenByPostRequest
                var tokenResult = await client.PostAsync("https://localhost:5001/api/Account/SignInUser", content);
                var token = System.Text.Json.JsonSerializer.Deserialize<TokenObject>
                    (await tokenResult.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
                Token = token.Token;
                #endregion

                #region GetUserIdByGetRequest
                //add Authorization header for JWT to authenticate client
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer  {Token}");
                var userIdResult = await client.GetAsync("https://localhost:5001/api/Account/IsUserLogIn?username=developer");
                UserId = await userIdResult.Content.ReadAsStringAsync();
                #endregion
            }
        }
    }
    public class TokenObject
    {
        public string Token { get; set; }
    }
}
