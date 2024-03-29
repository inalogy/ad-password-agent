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

wchar_t wcPath[MAX_PATH];
wchar_t logPath[MAX_PATH];
wchar_t errLogPath[MAX_PATH];

void writeLog(wchar_t* message) {
	FILE* pFile;
	SYSTEMTIME st;
	GetLocalTime(&st);
	_wfopen_s(&pFile, logPath, L"a+");
	if (pFile != NULL) {
		fwprintf(pFile, L"%d/%d/%d %d:%d:%d:%d %ws\n", st.wDay, st.wMonth, st.wYear, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds, message);
		fclose(pFile);
	}
}

void writeErr(wchar_t* message) {
	FILE* pFile;
	SYSTEMTIME st;
	GetLocalTime(&st);
	_wfopen_s(&pFile, errLogPath, L"a+");
	if (pFile != NULL) {
		fwprintf(pFile, L"%d/%d/%d %d:%d:%d:%d %ws\n", st.wDay, st.wMonth, st.wYear, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds, message);
		fclose(pFile);
	}
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
		SYSTEMTIME st;
		GetLocalTime(&st);
		_snwprintf_s(logPath, sizeof(logPath) / 2, _TRUNCATE, L"%ws-%d-%d-%d.log", wcPath, st.wYear, st.wMonth, st.wDay);
		_snwprintf_s(errLogPath, sizeof(errLogPath) / 2, _TRUNCATE, L"%ws-%d-%d-%d.err", wcPath, st.wYear, st.wMonth, st.wDay);
		writeLog(L"InitializeChangeNotify():");
		writeLog(logPath);
		return res;
	}
	return FALSE;
}

BOOLEAN EXPORT StartsWith(PUNICODE_STRING userName, const wchar_t* prefix)
{
	wchar_t wcPrefix[BUFFER_LENGTH_WCHAR];
	_snwprintf_s(wcPrefix, BUFFER_LENGTH_WCHAR, _TRUNCATE, L"%wZ", userName);
	return wcscmp(prefix, wcPrefix) == 0;
}

BOOLEAN EXPORT SkipAccount(PUNICODE_STRING userName)
{
	wchar_t wcUserName[BUFFER_LENGTH_WCHAR];
	_snwprintf_s(wcUserName, BUFFER_LENGTH_WCHAR, _TRUNCATE, L"%wZ", userName);
	size_t len = wcslen(wcUserName);
	return StartsWith(userName, L"HealthMailbox") || StartsWith(userName, L"apl_") || wcscmp(wcUserName + len - 1, L"$") == 0;
}

NTSTATUS EXPORT PasswordChangeNotify(PUNICODE_STRING UserName, ULONG RelativeId, PUNICODE_STRING NewPassword)
{
	wchar_t tmpLog[BUFFER_LENGTH_WCHAR];
	wchar_t wcArgv[BUFFER_LENGTH_WCHAR];
	__try {
		//_snwprintf_s(tmpLog, BUFFER_LENGTH_WCHAR, _TRUNCATE, L"PasswordChangeNotify( [%wZ], %lu, %d )", UserName, RelativeId, NewPassword->Length);
		// writeLog(tmpLog);

		if (!SkipAccount(UserName)) {
			// Format: "ADPasswordAgent.exe" "Username" "Password"
			_snwprintf_s(wcArgv, BUFFER_LENGTH_WCHAR, _TRUNCATE, L"\"%ws\" \"%wZ\" \"%wZ\"", wcPath, UserName, NewPassword);
			// writeLog(wcArgv);

			STARTUPINFOW StartupInfo = { 0 };
			StartupInfo.cb = sizeof(StartupInfo);
			StartupInfo.dwFlags = STARTF_USESHOWWINDOW;
			StartupInfo.wShowWindow = SW_HIDE;
			PROCESS_INFORMATION ProcessInfo;
			BOOL res = CreateProcessW(NULL, wcArgv, NULL, NULL, TRUE, 0, NULL, NULL, &StartupInfo, &ProcessInfo);
			SecureZeroMemory(wcArgv, sizeof(wcArgv));
			Sleep(50);
			if (res)
			{
				//_snwprintf_s(tmpLog, BUFFER_LENGTH_WCHAR, _TRUNCATE, L"Notify change password for user [%wZ] success", UserName);
				//writeLog(tmpLog);

				CloseHandle(ProcessInfo.hProcess);
				CloseHandle(ProcessInfo.hThread);
				return STATUS_SUCCESS;
			}
			else {
				_snwprintf_s(tmpLog, BUFFER_LENGTH_WCHAR, _TRUNCATE, L"Notify change password for user [%wZ] failed", UserName);
				writeErr(tmpLog);
				return STATUS_SUCCESS;
			}
		}
		else {
			//_snwprintf_s(tmpLog, BUFFER_LENGTH_WCHAR, _TRUNCATE, L"user [%wZ] skipped", UserName);
			//writeLog(tmpLog);
			return STATUS_SUCCESS;
		}
	}
	__except (EXCEPTION_EXECUTE_HANDLER) {
		_snwprintf_s(tmpLog, BUFFER_LENGTH_WCHAR, _TRUNCATE, L"Catch Error in Filter for [%wZ] errocode: %d", UserName, GetExceptionCode());
		writeErr(tmpLog);
		return STATUS_SUCCESS;
	}
}

BOOLEAN EXPORT PasswordFilter(PUNICODE_STRING AccountName, PUNICODE_STRING FullName, PUNICODE_STRING Password, BOOLEAN SetOperation)
{
	return TRUE;
}