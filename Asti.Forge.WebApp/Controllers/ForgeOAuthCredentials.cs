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
    /// <summary> Create, refresh, and delete Forge OAuth three-legged token credentials </summary>
    internal class ForgeOAuthCredentials
    {
        #region Private Members

        /// <summary> The key of the cookie to store the credentials </summary>
        private const string ASTI_FORGE_COOKIE = "AstiForgeApp";
        /// <summary> Constructor of the ForgeCredentials for storing the token data </summary>
        private ForgeOAuthCredentials() { }

        #endregion Private Members

        #region Public Members

        /// <summary> The internal three-legged OAuth token </summary>
        public string ForgeTokenInternal { get; set; }
        /// <summary> The public three-legged OAuth token </summary>
        public string ForgeTokenPublic { get; set; }
        /// <summary> The three-legged OAuth token for refreshing credentials </summary>
        public string ForgeTokenRefresh { get; set; }
        /// <summary> The DateTime in which the credentials expire and need refreshed </summary>
        public DateTime ForgeTokenExpiresAt { get; set; }

        #endregion Public Members

        /// <summary> Gets a three-legged oauth token. </summary>
        /// <param name="authCode"></param>
        /// <param name="responseCookies"></param>
        /// <returns> ForgeCredentials </returns>
        public static async Task<ForgeOAuthCredentials> GetForgeCredentialAsync(string authCode, IResponseCookies responseCookies)
        {
            ThreeLeggedApi threeLeggedApiOAuth = new ThreeLeggedApi();

            dynamic credentialInternal = await threeLeggedApiOAuth.GettokenAsync(
                GetAppEnvironmentVariable("FORGE_CLIENT_ID"),
                GetAppEnvironmentVariable("FORGE_CLIENT_SECRET"),
                oAuthConstants.AUTHORIZATION_CODE,
                authCode,
                GetAppEnvironmentVariable("FORGE_CALLBACK_URL"));

            dynamic credentialPublic = await threeLeggedApiOAuth.RefreshtokenAsync(
                GetAppEnvironmentVariable("FORGE_CLIENT_ID"),
                GetAppEnvironmentVariable("FORGE_CLIENT_SECRET"),
                "refresh_token",
                credentialInternal.refresh_token,
                new Scope[] { Scope.DataWrite });

            ForgeOAuthCredentials forgeCredentials = new ForgeOAuthCredentials()
            {
                ForgeTokenInternal = credentialInternal.access_token,
                ForgeTokenPublic = credentialPublic.access_token,
                ForgeTokenRefresh = credentialPublic.refresh_token,
                ForgeTokenExpiresAt = DateTime.Now.AddSeconds(credentialInternal.expires_in),
            };

            responseCookies.Append(ASTI_FORGE_COOKIE, JsonConvert.SerializeObject(forgeCredentials));
            return forgeCredentials;
        }


        /// <summary>/ Gets the ForgeCredentials from the current session from the cookies and refreshes them if they are expired. </summary>
        /// <param name="requestCookies"></param>
        /// <param name="responseCookies"></param>
        /// <returns> Null if cookies does not contain the ASTI_FORGE_COOKIE. </returns>
        public static async Task<ForgeOAuthCredentials> GetForgeCredentialsFromSessionAsync(IRequestCookieCollection requestCookies, IResponseCookies responseCookies)
        {
            if (responseCookies == null || !requestCookies.ContainsKey(ASTI_FORGE_COOKIE))
            {
                return null;
            }

            ForgeOAuthCredentials forgeCredentials = JsonConvert.DeserializeObject<ForgeOAuthCredentials>(requestCookies[ASTI_FORGE_COOKIE]);
            if (forgeCredentials.ForgeTokenExpiresAt < DateTime.Now)
            {
                await forgeCredentials.RefreshForgeCredentialsAsync();
                responseCookies.Delete(ASTI_FORGE_COOKIE);
                responseCookies.Append(ASTI_FORGE_COOKIE, JsonConvert.SerializeObject(forgeCredentials));
            }

            return forgeCredentials;
        }


        /// <summary> Refreshes the internal and public Forge OAuth three-legged token if expired. </summary>
        /// <returns></returns>
        private async Task RefreshForgeCredentialsAsync()
        {
            ThreeLeggedApi threeLeggedApiOAuth = new ThreeLeggedApi();

            dynamic credentialInternal = await threeLeggedApiOAuth.RefreshtokenAsync(
                GetAppEnvironmentVariable("FORGE_CLIENT_ID"),
                GetAppEnvironmentVariable("FORGE_CLIENT_SECRET"),
                "refresh_token",
                ForgeTokenRefresh,
                new Scope[] { Scope.DataRead, Scope.DataCreate, Scope.DataWrite, Scope.ViewablesRead });

            dynamic credentialPublic = await threeLeggedApiOAuth.RefreshtokenAsync(
                GetAppEnvironmentVariable("FORGE_CLIENT_ID"),
                GetAppEnvironmentVariable("FORGE_CLIENT_SECRET"),
                "refresh_token",
                credentialInternal.refresh_token,
                new Scope[] { Scope.DataWrite });

            ForgeTokenInternal = credentialInternal.access_token;
            ForgeTokenPublic = credentialPublic.access_token;
            ForgeTokenRefresh = credentialPublic.refresh_token;
            ForgeTokenExpiresAt = DateTime.Now.AddSeconds(credentialInternal.expires_in);
        }


        /// <summary> Gets an Environment Variable by key name. </summary>
        /// <param name="key"></param>
        /// <returns>The environment variable value from the key/value pair</returns>
        public static string GetAppEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }


        /// <summary> Deletes the stored ForgeCredentials cookie when a user signs out. </summary>
        /// <param name="responseCookies"></param>
        public static void DeleteForgeCredentials(IResponseCookies responseCookies)
        {
            responseCookies.Delete(ASTI_FORGE_COOKIE);
        }
    }
}
