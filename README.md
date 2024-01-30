# midPoint Active Directory password agent.

## Functional description

midpoint Active Directory agent consists from two basic parts - AD password filter used for intercepting password being changed and midPoint updating service pushing intercepted password to midPoint IDM using Rest interface. In case of no availability of IDM Rest interface the intercepted information is encrypted and stored localy on domain controller for retry. Encryption is using private key of DC running on. Password agent has to be installed on all domain DC in target domain.

## Components

- ADPasswordFilter.dll
- MidPointUpdatingService.exe

#Installation

## Preffered method - compiled installer

1. Download Installer.msi and acompaning cmd files from link
2. Copy Installer.msi file and supplied cmd files to the target Domain Controller
3. On the target Domain Comtroller run install.CMD (it start msi in elevated prividledges mode)
4. Follow the installer GUI instructructions

## Code compilation and instalation

Use only, if custom changes has been made into the source code of the appliaction.

1. Download and install MSBuild Tools (free) to your local build computer

a. MS Build Tools
https://visualstudio.microsoft.com/cs/thank-you-downloading-visual-studio/?sku=BuildTools&rel=16#

b. .NET Framework 4.7.2 Developer Pack
https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net472-developer-pack-offline-installer

c. WIX Toolset 3.11 for the Installer build process
https://github.com/wixtoolset/wix3/releases/download/wix3112rtm/wix311.exe


2. Clone this repository to your local computer

3. Run MSBuild tools CMD window from the Start menu as Adminsitrator

4. Switch to the local GIT project repository folder

CD <local GIT project repository>

4. Run commands and wait for build finish

powershell .\CreateSigningCert.ps1

set VCTargetsPath="c:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Microsoft\VC\v160\"

msbuild ADPasswordAgent.sln /p:Configuration=Release /p:Platform="x64"

5. Complete built Installer.msi can be found then in the subfolder

.\Installer\bin\x64\Release

###1/ Automatic instalation of application

1. Copy Installer.msi file from folder .\Installer\bin\x64\Release to the target Domain Controller
2. On the target Domain Comtroller run CMD window as Administrator (elevated prividledges mode)
3. In the CMD window CD to the folder, where you have copied the Installer.msi
4. Type Installer.msi and press Enter
5. Follow the installer GUI instructructions

Manual instalation is only for debugging purpose and it is not recommended to useit in production environment.
----------------------------------------------------------

## Technical description

ADPasswordFilter.dll runs in the context of an AD Domain Controller and intercepts password change requests. Intercepted passwords are temporary stored in secured registry folder.

MidPointUpdatingService.exe is installed and registered as a service of Windows OS, running permanently checking for presence of an entry in secured registry folder. 
If any entry is present, is pushed into configured MidPoint instance, and if not successfull, by the means of the recoverable error (eg. Network Connection error), stored localy in retry queue.
Defined number of retry attempts is made to push information to configured midPoint instance. Each retry occures in rising time delay. When non-recoverable error occures, the operation is dequeued and released and the information is written to the log.
