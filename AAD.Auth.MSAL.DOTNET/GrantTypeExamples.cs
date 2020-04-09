/*##########################################################################################################
# Disclaimer
# This sample code, scripts, and other resources are not supported under any Microsoft standard support 
# program or service and are meant for illustrative purposes only.
#
# The sample code, scripts, and resources are provided AS IS without warranty of any kind. Microsoft 
# further disclaims all implied warranties including, without limitation, any implied warranties of 
# merchantability or of fitness for a particular purpose. The entire risk arising out of the use or 
# performance of this material and documentation remains with you. In no event shall Microsoft, its 
# authors, or anyone else involved in the creation, production, or delivery of the sample be liable 
# for any damages whatsoever (including, without limitation, damages for loss of business profits, 
# business interruption, loss of business information, or other pecuniary loss) arising out of the 
# use of or inability to use the samples or documentation, even if Microsoft has been advised of 
# the possibility of such damages.
##########################################################################################################*/

using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace AAD.Auth.MSAL.DOTNET
{
    class GrantTypeExamples
    {
        /********************************
         * App Configuration
         * 
         * Use these values to test your app. Depending on which grant type you are using
         * only a subset of these values will be used.
         * ******************************/
        string clientId;
        string clientSecret;
        string tenantId;
        string[] scopes;
        string username;
        string password;
        string redirectUri = "http://localhost";

        public GrantTypeExamples(IConfigurationRoot configuration)
        {
            clientId = configuration["clientId"];
            clientSecret = configuration["clientSecret"];
            tenantId = configuration["tenantId"];
            scopes = configuration["scopes"].Split(new char[] { ' ', ',', ';' });
            username = configuration["username"];
            password = configuration["password"];
        }

        public async Task<string> ClientCredentialsAsync()
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithTenantId(tenantId)
                .Build();

            AuthenticationResult auth = await app.AcquireTokenForClient(scopes)
                .ExecuteAsync();

            return auth.AccessToken;
        }

        public async Task<string> ResourceOwnerPasswordAsync()
        {
            IPublicClientApplication app = PublicClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantId)
                .Build();

            var securePassword = new SecureString();
            foreach (char c in password)
                securePassword.AppendChar(c);

            AuthenticationResult auth = await app.AcquireTokenByUsernamePassword(
                scopes,
                username,
                securePassword)
                            .ExecuteAsync();

            return auth.AccessToken;
        }

        public async Task<string> DeviceCodeAsync()
        {
            IPublicClientApplication app = PublicClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantId)
                .Build();

            AuthenticationResult auth = await app.AcquireTokenWithDeviceCode(scopes, deviceCodeCallback =>
            {
                Console.WriteLine(
                    deviceCodeCallback.Message
                );
                return Task.FromResult(0);
            }).ExecuteAsync();

            return auth.AccessToken;
        }

        public async Task<string> InteractiveAsync()
        {
            IPublicClientApplication app = PublicClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantId)
                .WithRedirectUri(redirectUri)
                .Build();

            AuthenticationResult auth = await app.AcquireTokenInteractive(scopes)
                .ExecuteAsync();

            return auth.AccessToken;
        }

        public async Task<string> LoginWithRefreshTokenAsync()
        {
            List<string> scopesWithRefresh = scopes.ToList();
            scopesWithRefresh.Add("offline_access");

            IPublicClientApplication app = PublicClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantId)
                .WithRedirectUri(redirectUri)
                .Build();

            TokenCacheSerialization.EnableSerialization(app.UserTokenCache);

            AuthenticationResult result = null;
            var accounts = await app.GetAccountsAsync();

            if (accounts.Any())
            {
                try
                {
                    // attempt to get a token from the cache or silently refresh
                    result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    // swallow exception
                }
            }

            // cache empty or no token for account
            if (result == null)
            {
                try
                {
                    result = await app.AcquireTokenInteractive(scopes)
                        .ExecuteAsync();
                }
                catch (MsalServiceException ex)
                {
                    throw;
                }
                catch (OperationCanceledException)
                {
                    result = null;
                }
                catch (MsalClientException ex)
                {
                    result = null;
                }
            }

            return result.AccessToken;
        }
    }
}
