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
using System.Security.Cryptography.X509Certificates;
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
        string certificateName;
        string certificatePass;

        public GrantTypeExamples(IConfigurationRoot configuration)
        {
            clientId = configuration["clientId"];
            clientSecret = configuration["clientSecret"];
            tenantId = configuration["tenantId"];
            scopes = configuration["scopes"].Split(new char[] { ' ', ',', ';' });
            username = configuration["username"];
            password = configuration["password"];
            certificateName = configuration["certificateFile"];
            certificatePass = configuration["certificatePassword"];
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

        internal async Task<string> IntegratedAsync()
        {
            IPublicClientApplication app = PublicClientApplicationBuilder
                  .Create(clientId)
                  .WithTenantId(tenantId)
                  .Build();

            var accounts = await app.GetAccountsAsync();

            AuthenticationResult auth = null;
            if (accounts.Any())
            {
                auth = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
            }
            else
            {
                try
                {
                    auth = await app.AcquireTokenByIntegratedWindowsAuth(scopes)
                        .WithUsername(username)
                       .ExecuteAsync();
                }
                catch (MsalUiRequiredException ex)
                {
                    // MsalUiRequiredException: AADSTS65001: The user or administrator has not consented to use the application 
                    // with ID '{appId}' named '{appName}'.Send an interactive authorization request for this user and resource.

                    // you need to get user consent first. This can be done, if you are not using .NET Core (which does not have any Web UI)
                    // by doing (once only) an AcquireToken interactive.

                    // If you are using .NET core or don't want to do an AcquireTokenInteractive, you might want to suggest the user to navigate
                    // to a URL to consent: https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&scope=user.read

                    // AADSTS50079: The user is required to use multi-factor authentication.
                    // There is no mitigation - if MFA is configured for your tenant and AAD decides to enforce it, 
                    // you need to fallback to an interactive flows such as AcquireTokenInteractive or AcquireTokenByDeviceCode
                }
                catch (MsalServiceException ex)
                {
                    // Kind of errors you could have (in ex.Message)

                    // MsalServiceException: AADSTS90010: The grant type is not supported over the /common or /consumers endpoints. Please use the /organizations or tenant-specific endpoint.
                    // you used common.
                    // Mitigation: as explained in the message from Azure AD, the authoriy needs to be tenanted or otherwise organizations

                    // MsalServiceException: AADSTS70002: The request body must contain the following parameter: 'client_secret or client_assertion'.
                    // Explanation: this can happen if your application was not registered as a public client application in Azure AD 
                    // Mitigation: in the Azure portal, edit the manifest for your application and set the `allowPublicClient` to `true` 
                }
                catch (MsalClientException ex)
                {
                    // Error Code: unknown_user Message: Could not identify logged in user
                    // Explanation: the library was unable to query the current Windows logged-in user or this user is not AD or AAD 
                    // joined (work-place joined users are not supported). 

                    // Mitigation 1: on UWP, check that the application has the following capabilities: Enterprise Authentication, 
                    // Private Networks (Client and Server), User Account Information

                    // Mitigation 2: Implement your own logic to fetch the username (e.g. john@contoso.com) and use the 
                    // AcquireTokenByIntegratedWindowsAuth form that takes in the username

                    // Error Code: integrated_windows_auth_not_supported_managed_user
                    // Explanation: This method relies on an a protocol exposed by Active Directory (AD). If a user was created in Azure 
                    // Active Directory without AD backing ("managed" user), this method will fail. Users created in AD and backed by 
                    // AAD ("federated" users) can benefit from this non-interactive method of authentication.
                    // Mitigation: Use interactive authentication
                }
            }

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

        public async Task<string> CertificateAuthAsync()
        {
            var cert = new X509Certificate2(this.certificateName, this.certificatePass, X509KeyStorageFlags.EphemeralKeySet);

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithCertificate(cert)
                .WithTenantId(tenantId)
                .Build();

            AuthenticationResult auth = await app.AcquireTokenForClient(scopes)
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
