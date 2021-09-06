#pragma warning( disable : 4005 )

#include <stdio.h>
#include <wchar.h>
#include <windows.h>
#include <ntsecapi.h>
#pragma warning(push, 0)
#include <ntstatus.h>
#pragma warning(pop)


#define UNICODE
#define _UNICODE
#define _WIN32_WINNT
#define EXPORT __declspec(dllexport)

#define BUFFER_LENGTH_WCHAR 2048

wchar_t wcPath[MAX_PATH];
wchar_t logPath[MAX_PATH];

void writeLog(wchar_t* message) {
	FILE* pFile;
	_wfopen_s(&pFile, logPath, L"a+");
	fwprintf(pFile, L"%ws\n", message);
	fclose(pFile);
}

BOOL ReadPath(wchar_t* pwcPath, DWORD* pdwPath)
{
	// Read HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\Agent

	HKEY hKey;
	if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\ADPasswordFilter", 0, KEY_READ, &hKey) == ERROR_SUCCESS)
	{
		if (RegQueryValueExW(hKey, L"Agent", NULL, NULL, (BYTE*)pwcPath, pdwPath) == STATUS_SUCCESS)
		{
			return TRUE;
		}
	}
	RegCloseKey(hKey);
	return FALSE;
}

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	return TRUE;
}

BOOLEAN EXPORT InitializeChangeNotify(void)
{
	wchar_t wcRawPath[MAX_PATH];
	DWORD dwPath = sizeof(wcPath);
	if (ReadPath(wcRawPath, &dwPath))
	{
		// Expand environment strings: %programfiles% -> C:\Program Files
		BOOL res = ExpandEnvironmentStringsW(wcRawPath, wcPath, MAX_PATH)>0;
		_snwprintf_s(logPath, sizeof(logPath) / 2, _TRUNCATE, L"%ws.log", wcPath);
		return res;
	}
	return FALSE;
}

NTSTATUS EXPORT PasswordChangeNotify(PUNICODE_STRING UserName, ULONG RelativeId, PUNICODE_STRING NewPassword)
{
	// Format: "ADPasswordAgent.exe" "Username" "Password"
	wchar_t wcArgv[BUFFER_LENGTH_WCHAR];
	wchar_t tmpLog[BUFFER_LENGTH_WCHAR];
	_snwprintf_s(wcArgv, sizeof(wcArgv)/2, _TRUNCATE, L"\"%ws\" \"%wZ\" \"%wZ\"", wcPath, UserName, NewPassword);

	STARTUPINFOW StartupInfo = { 0 };
	StartupInfo.cb = sizeof(StartupInfo);
	StartupInfo.dwFlags = STARTF_USESHOWWINDOW;
	StartupInfo.wShowWindow = SW_HIDE;
	PROCESS_INFORMATION ProcessInfo;
	BOOL res = CreateProcessW(NULL, wcArgv, NULL, NULL, TRUE, 0, NULL, NULL, &StartupInfo, &ProcessInfo);
	SecureZeroMemory(NewPassword->Buffer, NewPassword->Length);
	SecureZeroMemory(wcArgv, sizeof(wcArgv));
	Sleep(10);
	if (res)
	{
		
		_snwprintf_s(tmpLog, sizeof(tmpLog) / 2, _TRUNCATE, L"Notify change password for user [%wZ] success",UserName);
		writeLog(tmpLog);
		
		CloseHandle(ProcessInfo.hProcess);
		CloseHandle(ProcessInfo.hThread);
		return STATUS_SUCCESS;
	} else {
		_snwprintf_s(tmpLog, sizeof(tmpLog) / 2, _TRUNCATE, L"Notify change password for user [%wZ] failed", UserName);
		writeLog(tmpLog);
		return STATE_SYSTEM_BUSY;
	}

	
}

BOOLEAN EXPORT PasswordFilter(PUNICODE_STRING AccountName, PUNICODE_STRING FullName, PUNICODE_STRING Password, BOOLEAN SetOperation)
{
	return TRUE;
}