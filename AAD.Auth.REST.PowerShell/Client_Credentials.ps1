
$authConfig = Get-Content .\M365x912691.json.user | ConvertFrom-Json

$authResult = Invoke-RestMethod -Method Post -Uri "$($authConfig.loginUrl)/$($authConfig.tenantId)/oauth2/token" -Headers @{"Content-Type"="application/x-www-form-urlencoded"} -Body "grant_type=client_credentials&client_id=$($authConfig.client_id)&client_secret=$($authConfig.client_secret)&scope=$($authConfig.scopes)"

$authResult