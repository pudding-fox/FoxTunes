#ifdef _DEBUG
#include <stdio.h>
#endif

#include "file_name.h"

BOOL get_file_name(const void* file, DWORD flags, const char* path) {
	if ((flags & BASS_UNICODE) == BASS_UNICODE) {
		if (WideCharToMultiByte(CP_ACP, 0, (LPCWSTR)file, -1, (LPSTR)path, MAX_PATH, NULL, NULL) <= 0) {
#if _DEBUG
			printf("Failed to read unicode string.\n");
#endif
			return FALSE;
		}
	}
	else {
		path = file;
	}
	return TRUE;
}