$cert = New-SelfSignedCertificate -DnsName www.yourwebsite.com -Type CodeSigning -CertStoreLocation Cert:\CurrentUser\My
$CertPassword = ConvertTo-SecureString -String "sample-password" -Force -AsPlainText
Export-PfxCertificate -Cert "cert:\CurrentUser\My\$($cert.Thumbprint)" -FilePath ".\MidPointUpdatingService\MidPointUpdatingService_TemporaryKey.pfx" -Password $CertPassword
