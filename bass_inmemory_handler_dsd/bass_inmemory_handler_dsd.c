#ifdef _DEBUG
#include <stdio.h>
#endif

#include "bass/bassdsd.h"

#include "bass_inmemory_handler_dsd.h"
#include "../bass_inmemory_handler/inmemory_stream.h"
#include "../bass_inmemory_handler/reader.h"
#include "../bass_inmemory_handler/file_name.h"

BOOL is_initialized = FALSE;

//I have no idea how to prevent linking against this routine in msvcrt.
//It doesn't exist on Windows XP.
//Hopefully it doesn't do anything important.
int _except_handler4_common() {
	return 0;
}

BOOL BASSINMEMORYHANDLERDSDDEF(BASS_INMEMORY_HANDLER_DSD_Init)() {
	if (is_initialized) {
		return FALSE;
	}
	is_initialized = TRUE;
#if _DEBUG
	printf("BASS INMEMORY HANDLER DSD initialized.\n");
#endif
	return TRUE;
}

BOOL BASSINMEMORYHANDLERDSDDEF(BASS_INMEMORY_HANDLER_DSD_Free)() {
	if (!is_initialized) {
		return FALSE;
	}
	is_initialized = FALSE;
#if _DEBUG
	printf("BASS INMEMORY HANDLER DSD released.\n");
#endif
	return TRUE;
}

HSTREAM BASSINMEMORYHANDLERDSDDEF(_BASS_DSD_StreamCreateFileUser)(DWORD system, DWORD flags, const BASS_FILEPROCS *procs, void *user) {
	return BASS_DSD_StreamCreateFileUser(system, flags, procs, user, 0);
}

HSTREAM BASSINMEMORYHANDLERDSDDEF(BASS_INMEMORY_HANDLER_DSD_StreamCreateFile)(BOOL mem, const void* file, QWORD offset, QWORD length, DWORD flags) {
	IM_STREAM* stream;
	BUFFER* buffer;
	const char path[MAX_PATH];
	if (!get_file_name(file, flags, path)) {
#if _DEBUG
		printf("Failed to determine file name.\n");
#endif
		return 0;
	}
	buffer = read_file_buffer(path, offset, length);
	if (buffer) {
		stream = inmemory_stream_create(buffer, &_BASS_DSD_StreamCreateFileUser, flags);
		if (stream) {
			return stream->handle;
		}
	}
	return 0;
}