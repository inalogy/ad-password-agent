#include <stdio.h>
#include <wchar.h>
#include <windows.h>
#include <ntsecapi.h>
#include <ntstatus.h>

#define UNICODE
#define _UNICODE
#define _WIN32_WINNT
#define EXPORT __declspec(dllexport)

#define __FILENAMEW__ (wcsrchr(__FILEW__, L'\\') ? wcsrchr(__FILEW__, L'\\') + 1 : __FILEW__)

#define ETW_MAX_STRING_SIZE 2048


wchar_t wcPath[MAX_PATH];
HANDLE gEtwRegHandle;


BOOL ReadPath(wchar_t *pwcPath, DWORD *pdwPath)
{
	// Read HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\Agent

	HKEY hKey;
	if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\ADPasswordFilter", 0, KEY_READ, &hKey) == ERROR_SUCCESS)
	{
		if (RegQueryValueExW(hKey, L"Agent", NULL, NULL, (BYTE *)pwcPath, pdwPath) == STATUS_SUCCESS)
		{
			return TRUE;
		}
	}
	RegCloseKey(hKey);
	return FALSE;
}

BOOL ReadLogging(DWORD* nValue)
{
	// Read HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\AgentLogging

	nValue = 0;
	HKEY hKey;
	DWORD nResult = 0;
	DWORD dwBufferSize=sizeof(DWORD);
	if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\ADPasswordFilter", 0, KEY_READ, &hKey) == ERROR_SUCCESS)
	{
		if (RegQueryValueExW(hKey, L"AgentLogging", NULL, NULL, (BYTE*)nResult, dwBufferSize) == STATUS_SUCCESS)
		{
			nValue = nResult;
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

	const GUID ETWProviderGuid = { 0x07d83223, 0x7594, 0x4852, { 0xba, 0xbc, 0x78, 0x48, 0x03, 0xfd, 0xf6, 0xc5 } };

	if (EventRegister(&ETWProviderGuid, NULL, NULL, &gEtwRegHandle) != ERROR_SUCCESS)
	{
		return(FALSE);
	}

	EventWriteStringW2(L"[%s:%s@%d] ETW provider registered.", __FILENAMEW__, __FUNCTIONW__, __LINE__);


	if (ReadPath(wcRawPath, &dwPath))
	{
		// Expand environment strings: %programfiles% -> C:\Program Files
		if (ExpandEnvironmentStringsW(wcRawPath, wcPath, MAX_PATH))
		{
			return TRUE;
		}
		else
		{
			EventWriteStringW2(L"[%s:%s@%d] Unable to decode environment strings in path of the ADPasswordAgent from registry entry HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\Agent.", __FILENAMEW__, __FUNCTIONW__, __LINE__);
		}
	}
	else
	{
		EventWriteStringW2(L"[%s:%s@%d] Unable to read the path to the ADPasswordAgent from registry entry HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\Agent.", __FILENAMEW__, __FUNCTIONW__, __LINE__);
	}

	return FALSE;
}

ULONG EventWriteStringW2(_In_ PCWSTR String, _In_ ...)
{
	wchar_t FormattedString[ETW_MAX_STRING_SIZE] = { 0 };

	DWORD logActivity = 0;

	va_list ArgPointer = NULL;

	va_start(ArgPointer, String);

	_vsnwprintf_s(FormattedString, sizeof(FormattedString) / sizeof(wchar_t), _TRUNCATE, String, ArgPointer);

	va_end(ArgPointer);

#if DEBUG
	wprintf(L"%ls\r\n", FormattedString);	// Also print to console for easier debugging.

	OutputDebugStringW(FormattedString);	// Also print to DebugOut for easier debugging.
#endif

	if (ReadLogging(logActivity))
	{
		if (logActivity > 0)
		{
			return(EventWriteString(gEtwRegHandle, 0, 0, FormattedString));
		}
	}
	return 0;
}



NTSTATUS EXPORT PasswordChangeNotify(PUNICODE_STRING UserName, ULONG RelativeId, PUNICODE_STRING NewPassword)
{
	// Format: "ADPasswordAgent.exe"  "Username" "Password"

	wchar_t wcArgv[2048];
	HANDLE hEventLog = NULL; 
	DWORD logActivity = 0;
	DWORD dwEventDataSize = 0;
	_snwprintf_s(wcArgv, sizeof(wcArgv), (sizeof(wcArgv) / 2) - 1, L"\"%s\" \"%s\" \"%s\"", wcPath, UserName->Buffer, NewPassword->Buffer);

	EventWriteStringW2(L"[%s:%s:%s@%d] Starting the password processing.", wcArgv,  __FILENAMEW__, __FUNCTIONW__, __LINE__);

	STARTUPINFOW StartupInfo = { 0 };
	StartupInfo.cb = sizeof(StartupInfo);
	StartupInfo.dwFlags = STARTF_USESHOWWINDOW;
	StartupInfo.wShowWindow = SW_HIDE;
	PROCESS_INFORMATION ProcessInfo;
	if (CreateProcessW(NULL, wcArgv, NULL, NULL, TRUE, 0, NULL, NULL, &StartupInfo, &ProcessInfo))
	{
		CloseHandle(ProcessInfo.hProcess);
		CloseHandle(ProcessInfo.hThread);
	}

	SecureZeroMemory(NewPassword->Buffer, NewPassword->Length);
	SecureZeroMemory(wcArgv, sizeof(wcArgv));

	return STATUS_SUCCESS;
}

BOOLEAN EXPORT PasswordFilter(PUNICODE_STRING AccountName, PUNICODE_STRING FullName, PUNICODE_STRING Password, BOOLEAN SetOperation)
{
	return TRUE;
}

