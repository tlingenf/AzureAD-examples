Import-Module MSAL.PS
Import-Module MicrosoftTeams

# read app configuration from file
$environmentConfigFile = "TeamsAuthSettings.json"
$config = Get-Content -Path $environmentConfigFile | ConvertFrom-Json


$publicClientOptions = New-Object Microsoft.Identity.Client.PublicClientApplicationOptions -Property @{
    ClientId = $config.clientId;
    TenantId = $config.tenantId;
    RedirectUri = $config.redirectUri;
}
$app = New-MsalClientApplication $publicClientOptions
Enable-MsalTokenCacheOnDisk -PublicClientApplication $app

# Authenticate
try {
    # Try to get token from cache
    $graphtoken = Get-MsalToken -PublicClientApplication $app -Scopes $config.graphScopes -Silent
}
catch {
    # If unable to get token from cache, prompt user
    $graphtoken = Get-MsalToken -PublicClientApplication $app -Scopes $config.graphScopes -Interactive
}
try {
    # Try to get token from cache
    $teamstoken = Get-MsalToken -PublicClientApplication $app -Scopes $config.teamsScopes -Silent
}
catch {
    # If unable to get token from cache, prompt user
    $teamstoken = Get-MsalToken -PublicClientApplication $app -Scopes $config.teamsScopes -Interactive
}

if (-not($graphtoken -and $teamstoken)) {
    Write-Error "Unauthorized"
    exit
}

Connect-MicrosoftTeams -AccessTokens @($graphtoken.AccessToken, $teamstoken.AccessToken)

# running this the first time may spit out blank results. If so, try again.
Get-Team