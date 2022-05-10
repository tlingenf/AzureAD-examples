Import-Module MSAL.PS
Import-Module PnP.PowerShell

# read app configuration from file
$environmentConfigFile = "Environment.json"
$config = Get-Content -Path $environmentConfigFile | ConvertFrom-Json

# create a client application and enable the token cache
$publicClientOptions = New-Object Microsoft.Identity.Client.PublicClientApplicationOptions -Property @{
    ClientId = $config.ClientId;
    TenantId = $config.tenantId;
    RedirectUri = $config.redirectUri;
}
$app = New-MsalClientApplication $publicClientOptions
Enable-MsalTokenCacheOnDisk -PublicClientApplication $app

# Authenticate
try {
    # Try to get token from cache
    $auth = Get-MsalToken -PublicClientApplication $app -Scopes $config.scopes -Silent
}
catch {
    # If unable to get token from cache, prompt user
    $auth = Get-MsalToken -PublicClientApplication $app -Scopes $config.scopes -Interactive
}

# exit if auth unsuccessful
if (-not($auth -and $auth.AccessToken)) {
    Write-Error "Unable to authenticate user"
    exit
}

# do work
Connect-PnPOnline -Url $config.sharePointSite -AccessToken $auth.AccessToken
Get-PnPWeb
