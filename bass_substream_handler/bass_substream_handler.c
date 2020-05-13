#ifdef _DEBUG
#include <stdio.h>
#endif

#include "bass_substream_handler.h"
#include "bass_substream.h"

BOOL is_initialized = FALSE;

//I have no idea how to prevent linking against this routine in msvcrt.
//It doesn't exist on Windows XP.
//Hopefully it doesn't do anything important.
int _except_handler4_common() {
	return 0;
}

BOOL BASSSUBSTREAMHANDLERDEF(BASS_SUBSTREAM_HANDLER_Init)() {
	if (is_initialized) {
		return FALSE;
	}
	is_initialized = TRUE;
#if _DEBUG
	printf("BASS SUBSTREAM HANDLER initialized.\n");
#endif
	return TRUE;
}

BOOL BASSSUBSTREAMHANDLERDEF(BASS_SUBSTREAM_HANDLER_Free)() {
	if (!is_initialized) {
		return FALSE;
	}
	is_initialized = FALSE;
#if _DEBUG
	printf("BASS SUBSTREAM HANDLER released.\n");
#endif
	return TRUE;
}

HSTREAM BASSSUBSTREAMHANDLERDEF(BASS_SUBSTREAM_HANDLER_StreamCreate)(HSTREAM handle, QWORD offset, QWORD length, DWORD flags) {
	return substream_create(handle, offset, length, flags);
}