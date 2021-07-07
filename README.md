# midPoint Active Directory live password agent.

## Functional description

This ~~application~~ is built on top of the PoC from listens to AD password change requests using the filter and synchronizes the changes with midPoint using a Secure Persistent Queue.

## Components

- ADPasswordFilter.dll
- ADPasswordAgent.exe
- MidPointUpdatingService.exe

## Code compilation and instalation

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

4. Run command and wait for build finished

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

###2/ Manual instalation of MidPointUpdatingService.exe

1. Create service target folder on the target domain controller drive
2. Copy all files form /bin/Release folder to the created target folder
3. Edit the file MidPointUpdatingService.exe.config, and set up the settings parameters 
4. Open VisualStudio Command Windows
5. Issue in VS CMD : 
    CD {target folder}  
    installutil MidPointUpdatingService.exe
6. An interactive dialog appears requesting the user account and the password for service account. Fill it in.
7. Run Services.msc
8. Find the service named MidPoint Updating Service and start it.

###3/ Manual instalation of the Agent

1. Create agent target folder on the target domain controller drive
2. Copy all files form /bin/Release folder to the created target folder
3. Create Windows Registry entry in folder HKLM\SOFTWARE\ADPasswordFilter, named Agent of type STRING, and the value {agent target folder}/ADPasswordAgent.exe

###4/ Manual instalation of the Filter

1. Copy ADPAsswordFilter.dll form /bin/Release folder to the C:\Windows\SysWOW64 folder on domain controller
2. Create Windows Registry entry in folder HKLM\SYSTEM\CurrentControlSet\Control\Lsa, named Notification Packages of type MULTISTRING, and the value ADPasswordFilter.dll


####Settings:

* MidPoint Base URL -  BASEURL
* MidPoint Account Username - AUTHUSR
* MidPoint Account Password - AUTHPWD
* MidPoint Queue Identifier - QUEUEFLD  (do not change the default setting m if there is not more then ome MIdpoint synchronized from the same DC)
* Number of attempts on MidPoint call - RETRYCNT  (max 500 for performance reasons)
* Time in seconds to wait for queue availability - QUEUEWAIT  ( used for interprocess locking, change only to higher value if there are timeout exceptions of the agent in case of extreme load - 60 requests per second and above )
* Logging level 0-debug, 1-info, 2-warning, 3- Error to 4- Fatal error only - LOGLEVEL
* Log storage path - LOGPATH
* MidPoint SSL Setting 0-HTTP only, 1- HTTPS/TLS 1.2 with certificate in Local Computer repository, 2- HTTPS/TLS 1.2 with certificate in file X.509 - SSL
* Certificate Name in SSL mode 1 contains SubjectDN of the certificate in form CN=xxx.yyy.zzz , in SSL mode 2 the Path and full name of the X.509 CER file


## Technical description

ADPasswordFilter.dll runs in the context of an AD Domain Controller and listens for AD password change requests.

ADPasswordAgent.exe encrypting the passwords and sending them as ActionCalls to the Secure Persistent Queue located in the Isolated Storage of the domain controller.

![alt text](ServiceCodeMap.png)

MidPointUpdatingService.exe is installed and registered as a service of Windows OS, running permanently checking for presence of an ActionCall in the queue. 
If any ActionCall is present, is executed against the configured MidPoint instance, and if not successfull, by the means of the recoverable error (eg. Network Connection error),
a couple of attempts is made to retry in a rising time delay. When non-recoverable error occures, the ActionCall is dequeued and released and the information is written to the log.