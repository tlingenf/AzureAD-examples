# AAD.Auth.MSAL.DOTNET

This solution is provided by Travis Lingenfelder, Premier Field Engineer for Microsoft Services.


## Disclaimer
This sample code, scripts, and other resources are not supported under any Microsoft standard support program or service and are meant for illustrative purposes only. The sample code, scripts, and resources are provided AS IS without warranty of any kind. Microsoft further disclaims all implied warranties including, without limitation, any implied warranties of 
merchantability or of fitness for a particular purpose. The entire risk arising out of the use or performance of this material and documentation remains with you. In no event shall Microsoft, its authors, or anyone else involved in the creation, production, or delivery of the sample be liable for any damages whatsoever (including, without limitation, damages for loss of business profits, business interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability to use the samples or documentation, even if Microsoft has been advised of the possibility of such damages.

## About This Sample

This sample demonstrates how you can use the Microsoft Authentication Library (MSAL) and the various types of OAuth grant flows that 
you can use in your applications to log in users.

### OAuth Flows Shown

The OAuth flows demonstrated by this sample:
- Client Credentials - App Only
- Resource Owner Password Credentials - Delegated
- Device Code Flow - Delegated
- Interactive Login - Delegated
- Refresh Token - Delegated

*The Refresh Token example will also request the offline_access scope to obtain a refresh token. The first time you run this flow it will create an 
encrypted token cache file in your executable (bin) folder called **AAD.Auth.MSAL.DOTNET.dll.msalcache.bin3**. Each additional time it will utilize the refresh
token from the token cache. Remove the token cache file to prompt for user authentication again. The Refresh Token sample utilizes an interactive login to perform the first login, but you could use other OAuth flows for thi initial login, i.e. device code.*

### Azure AD App Configuration

You will need to create a new Azure AD App Registration and update the sample solution to include the details of your app registration. To update the sample solution, place the values for your app registration and login user account into the **appsettings.json** file as shown below:

```
{
  "clientId":  "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret":  "xxxxxxxxxxxxxxxxxxxxxxxxxx",
  "tenantId":  "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "scopes":  "https://graph.microsoft.com/.default",
  "username": "user@domain.com",
  "password":  "xxxxxxxxxxxxxxx"
}
```

When you create your Azure AD App registration make sure to set the following according the the OAuth flows you plan to use. *Not all OAuth flows use all of these values. For example, a public client will not need a clientSecret or user password.*

### Public Client
A public client is one that does not store a client secret. You set this in the Authentication blade of your app registration.<br/>
Required for OAuth Flows: ROPC and Device Code

### Redirect URI
There are many reasons for needing the Redirect URI. For this sample the Redirect URI is needed when an interactive login is used. This includes the Interactive Login and the Refresh Token examples. You set this in the Authentication blade of your app registration by adding a mobile and desktop application platform and specifying a redirect URI of http://localhost.
Required for OAuth Flows: Interactive Login and Refresh Token

### Client Secret
You can use either a client secret or a certificate. This sample uses a client secret which will work for access most resources with the exception of SharePoint APIs when using Application permissions.
Required for OAuth Flows: Client Credentials

## Permission Scopes
Permission scopes determine what types of actions are allowed to be performed through the application registration.
Required for OAuth Flows: All
