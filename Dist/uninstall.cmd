@echo off
:init
setlocal DisableDelayedExpansion
set "batchPath=%~0"
for %%k in (%0) do set batchName=%%~nk
set "vbsGetPrivileges=%temp%\OEgetPriv_%batchName%.vbs"
setlocal EnableDelayedExpansion

:checkPrivileges
NET FILE 1>NUL 2>NUL
if '%errorlevel%' == '0' ( goto gotPrivileges ) else ( goto getPrivileges )

:getPrivileges
if '%1'=='ELEV' (echo ELEV & shift /1 & goto gotPrivileges)
ECHO Set UAC = CreateObject^("Shell.Application"^) > "%vbsGetPrivileges%"
ECHO args = "ELEV " >> "%vbsGetPrivileges%"
ECHO For Each strArg in WScript.Arguments >> "%vbsGetPrivileges%"
ECHO args = args ^& strArg ^& " "  >> "%vbsGetPrivileges%"
ECHO Next >> "%vbsGetPrivileges%"
ECHO UAC.ShellExecute "!batchPath!", args, "", "runas", 1 >> "%vbsGetPrivileges%"
"%SystemRoot%\System32\WScript.exe" "%vbsGetPrivileges%" %*
exit /B

:gotPrivileges
setlocal & pushd .
cd /d %~dp0
if '%1'=='ELEV' (del "%vbsGetPrivileges%" 1>nul 2>nul  &  shift /1)

sc query state= all | findstr /C:"SERVICE_NAME: MidpointUpdatingService" 
if NOT ERRORLEVEL 1 (
echo Deleting MidPoint Updating Service
sc stop MidpointUpdatingService
sc delete MidpointUpdatingService
echo Deleting binaries of Midpoint Updating Service and Password agent
rmdir /S /Q "C:\Program Files\MidpointUpdatingService"
rmdir /S /Q "C:\Program Files\ADPasswordAgent"
echo Renaming password agent filter
c:
cd \Windows\system32
del /Q /F ADPasswordFilter1.dll
ren ADPasswordFilter.dll ADPasswordFilter1.dll
echo Reboot now
pause
exit
)
echo Now we are after reboot, deleting password filter directories
del c:\Windows\System32\ADPasswordFilter1.dll
rmdir /S /Q "C:\ProgramData\IsolatedStorage"
rmdir /S /Q "C:\ProgramData\Midpoint.ADPassword.Queue"
rmdir /S /Q "C:\ProgramData\Midpoint.ADPassword.Queue.Heap"
rmdir /S /Q "C:\Program Files\MidpointUpdatingService"
rmdir /S /Q "C:\Program Files\ADPasswordAgent"
