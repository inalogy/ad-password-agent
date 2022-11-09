cd C:\repos\ADPasswordAgent
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\msbuild" ADPasswordAgent.sln /p:Configuration=Release /p:Platform="x64"
ECHO "Sign Installer ..."
sign-installer.bat
PAUSE