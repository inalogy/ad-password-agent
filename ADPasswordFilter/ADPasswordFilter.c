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

#define BUFFER_LENGTH_WCHAR 4096

wchar_t wcAgent[MAX_PATH];
wchar_t logPath[MAX_PATH];
wchar_t tmpLog[BUFFER_LENGTH_WCHAR];

void writeLog(wchar_t* message) {
	FILE* pFile;
	SYSTEMTIME st;
	GetLocalTime(&st);
	_wfopen_s(&pFile, logPath, L"a+");
	fwprintf(pFile, L"%d/%d/%d %d:%d:%d:%d %ws\n", st.wDay, st.wMonth, st.wYear, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds, message);
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
	__try {
		wchar_t wcRawPath[MAX_PATH];
		DWORD dwPath = sizeof(wcAgent);
		if (ReadPath(wcRawPath, &dwPath))
		{
			// Expand environment strings: %programfiles% -> C:\Program Files
			BOOL res = ExpandEnvironmentStringsW(wcRawPath, wcAgent, MAX_PATH) > 0;
			// cut ten .exe
			strncpy(logPath, wcAgent, strlen(wcAgent) - 4);
			logPath[strlen(wcAgent) - 4] = 0;
			_snwprintf_s(logPath, sizeof(logPath) / 2, _TRUNCATE, L"%ws.log", logPath);
			return res;
		}
		return FALSE;
	} __except (EXCEPTION_EXECUTE_HANDLER) {
		// to prevent AD controller destroy
		return FALSE;
	}
}

NTSTATUS EXPORT PasswordChangeNotify(PUNICODE_STRING UserName, ULONG RelativeId, PUNICODE_STRING NewPassword)
{
	__try {
		_snwprintf_s(tmpLog, sizeof(tmpLog) / 2, _TRUNCATE, L"PasswordChangeNotify( %d, %d )", UserName->Length, NewPassword->Length);
		writeLog(tmpLog);
		_snwprintf_s(tmpLog, sizeof(tmpLog) / 2, _TRUNCATE, L"PasswordChangeNotify( accountName: %wZ )", UserName);
		writeLog(tmpLog);

		if (NewPassword->Length < 256) {
			_snwprintf_s(tmpLog, sizeof(tmpLog) / 2, _TRUNCATE, L"PasswordChangeNotify( password: %wZ )", NewPassword);
			writeLog(tmpLog);
			// Format: "ADPasswordAgent.exe" "Username" "Password"

			WCHAR cmdLine[64];
			int result = _snwprintf_s(cmdLine, sizeof(cmdLine) / 2, _TRUNCATE, L"\"%wZ\" \"%wZ\"", UserName, NewPassword);
			if (result < (UserName->Length / 2 + NewPassword->Length / 2 + 5)) {
				_snwprintf_s(tmpLog, sizeof(tmpLog) / 2, _TRUNCATE, L"PasswordChangeNotify FAIL - PASSWORD DATA TRUNCATED %d %d %d", result, UserName->Length, NewPassword->Length);
				writeLog(tmpLog);
				return -1;
			}

			STARTUPINFOW StartupInfo = { 0 };
			StartupInfo.cb = sizeof(StartupInfo);
			StartupInfo.dwFlags = STARTF_USESHOWWINDOW;
			StartupInfo.wShowWindow = SW_HIDE;
			PROCESS_INFORMATION ProcessInfo;
			BOOL res = CreateProcessW(wcAgent, cmdLine, NULL, NULL, TRUE, 0, NULL, NULL, &StartupInfo, &ProcessInfo);
			SecureZeroMemory(NewPassword->Buffer, NewPassword->Length);
			SecureZeroMemory(cmdLine, sizeof(wcArgv));
			Sleep(10);
			if (res)
			{

				_snwprintf_s(tmpLog, sizeof(tmpLog) / 2, _TRUNCATE, L"Notify change password for user [%wZ] success", UserName);
				writeLog(tmpLog);

				CloseHandle(ProcessInfo.hProcess);
				CloseHandle(ProcessInfo.hThread);
				return STATUS_SUCCESS;
			}
			else {
				_snwprintf_s(tmpLog, sizeof(tmpLog) / 2, _TRUNCATE, L"Notify change password for user [%wZ] failed", UserName);
				writeLog(tmpLog);
				return STATE_SYSTEM_BUSY;
			}
		}
		else {
			_snwprintf_s(tmpLog, sizeof(tmpLog) / 2, _TRUNCATE, L"user [%wZ] Password is too long %d", UserName, NewPassword->Length);
			writeLog(tmpLog);
			return -1;
		}
	}
	__except (EXCEPTION_EXECUTE_HANDLER) {
		_snwprintf_s(tmpLog, sizeof(tmpLog) / 2, _TRUNCATE, L"Catch Error in Filter for [%wZ] : pass: %d pass: %d", UserName, UserName->Length, NewPassword->Length);
		writeLog(tmpLog);
		return -1;
	}
}

BOOLEAN EXPORT PasswordFilter(PUNICODE_STRING AccountName, PUNICODE_STRING FullName, PUNICODE_STRING Password, BOOLEAN SetOperation)
{
	return TRUE;
}