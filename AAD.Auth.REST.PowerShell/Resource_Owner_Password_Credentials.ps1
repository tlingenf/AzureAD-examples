
$authConfig = Get-Content .\M365x912691.json.user | ConvertFrom-Json

$authResult = Invoke-RestMethod -Method Post -Uri "$($authConfig.loginUrl)/$($authConfig.tenantId)/oauth2/token" -Headers @{"Content-Type"="application/x-www-form-urlencoded"} -

$authResult