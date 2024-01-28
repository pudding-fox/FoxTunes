#include "buffer.h"

typedef struct {
	const wchar_t file[MAX_PATH + 1];
	QWORD position;
	BUFFER* buffer;
	DWORD handle;
} IM_STREAM;

typedef HSTREAM(CALLBACK IM_STREAM_HANDLER)(DWORD system, DWORD flags, const BASS_FILEPROCS *proc, void *user);

IM_STREAM* inmemory_stream_create(const wchar_t* file, BUFFER* buffer, IM_STREAM_HANDLER* const handler, DWORD flags);

void inmemory_stream_free(IM_STREAM* stream);