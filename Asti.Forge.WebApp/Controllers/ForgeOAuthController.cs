using Autodesk.Forge;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Asti.Forge.WebApp.Controllers
{
    /// <summary>
    /// Controller for Forge OAuth calls via HTTP routes
    /// </summary>
    public class ForgeOAuthController : Controller
    {
        public struct ForgeOAuthToken
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
        }

        [HttpGet]
        [Route("api/forge/oauth/token")]
        public async Task<ForgeOAuthToken> GetForgePublicTokenAsync()
        {
            //Get the credentials from the current session from the cookies
            ForgeOAuthCredentials forgeCredentials = await ForgeOAuthCredentials.GetForgeCredentialsFromSessionAsync(Request.Cookies, Response.Cookies);

            //If the credentials are null, the user has not signed in and been authorized
            if(forgeCredentials==null)
            {
                base.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return new ForgeOAuthToken();
            }
            else
            {
                //Otherwise, the user is authorized, so get the access token from the returned credentials
                return new ForgeOAuthToken()
                {
                    access_token = forgeCredentials.ForgeTokenPublic,
                    expires_in = (int)forgeCredentials.ForgeTokenExpiresAt.Subtract(DateTime.Now).TotalSeconds
                };
            }            
        }
    }    
}