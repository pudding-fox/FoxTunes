#include "buffer.h"

typedef struct {
	QWORD position;
	BUFFER* buffer;
	DWORD handle;
} IM_STREAM;

typedef HSTREAM (CALLBACK IM_STREAM_HANDLER)(DWORD system, DWORD flags, const BASS_FILEPROCS *proc, void *user);

IM_STREAM* inmemory_stream_create(BUFFER* buffer, IM_STREAM_HANDLER* handler, DWORD flags);

void inmemory_stream_free(IM_STREAM* stream);