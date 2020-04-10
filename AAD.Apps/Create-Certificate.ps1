#Requires -RunAsAdministrator
param (
    [parameter (Mandatory=$false)]$IsSanCertificate
)

Import-Module PKI
$password = Read-Host -Prompt "Enter the password used to secure the certificate private key."
$certPwd = ConvertTo-SecureString -String $password -AsPlainText -Force
$friendlyName = Read-Host -Prompt "What is the friendly name for this cert?"

$newCert = $null
if ($IsSanCertificate) {
    $dnsName = Read-Host -Prompt "Enter the DNS or SAN name(s). Separate Multiple entries with a ', ' <comman><space>."
    $newCert = New-SelfSignedCertificate -DnsName $dnsName -CertStoreLocation Cert:\LocalMachine\My -FriendlyName $friendlyName
} else {
    $subject = Read-Host -Prompt "Enter the Subject starting with CN="
    $newCert = New-SelfSignedCertificate -Subject $subject -CertStoreLocation Cert:\LocalMachine\My -FriendlyName $friendlyName
}
$newCert

Export-PfxCertificate -Cert $newCert -FilePath (Join-Path ([Environment]::GetFolderPath("Desktop")) ("{0}.pfx" -f $friendlyName)) -Password $certPwd
Export-Certificate -Type CERT -Cert $newCert -FilePath (Join-Path ([Environment]::GetFolderPath("Desktop")) ("{0}.cer" -f $friendlyName))
"{0} Password: {1}" -f $friendlyName, $password | Out-File -FilePath (Join-Path ([Environment]::GetFolderPath("Desktop")) ("{0}.txt" -f $friendlyName))

$deleteAction = Read-Host -Prompt "Do you want to remove the certificate from the local certificate store? (y/n)"
if ($deleteAction.ToLower() -eq "y") {
    Remove-Item (Join-Path Cert:\LocalMachine\My $newCert.Thumbprint) -Force
}