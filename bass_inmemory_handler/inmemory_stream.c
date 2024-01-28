#if _DEBUG
#include <stdio.h>
#endif

#include "inmemory_stream.h"

void CALLBACK inmemory_stream_close(void* user) {
	IM_STREAM* stream = user;
	inmemory_stream_free(stream);
}

QWORD CALLBACK inmemory_stream_length(void *user) {
	IM_STREAM* stream = user;
	return stream->buffer->length;
}

DWORD CALLBACK inmemory_stream_read(void *buffer, DWORD length, void *user) {
	IM_STREAM* stream = user;
	QWORD result = stream->buffer->length - stream->position;
	if (result >= length) {
		result = length;
	}
	buffer_read(stream->buffer, stream->position, result, buffer);
	stream->position += result;
	if (result < length) {
#if _DEBUG
		printf("Buffer under run while writing to ASIO buffer.\n");
#endif
	}
	return (DWORD)result;
}

BOOL CALLBACK inmemory_stream_seek(QWORD offset, void *user) {
	IM_STREAM* stream = user;
	if (offset >= 0 && offset <= stream->buffer->length) {
		stream->position = offset;
		return TRUE;
	}
#if _DEBUG
	printf("Seek offset is invalid: %d.\n", offset);
#endif
	return FALSE;
}

const BASS_FILEPROCS inmemory_stream_procs = {
	&inmemory_stream_close,
	&inmemory_stream_length,
	&inmemory_stream_read,
	&inmemory_stream_seek
};

IM_STREAM* inmemory_stream_create(BUFFER* buffer, IM_STREAM_HANDLER* handler, DWORD flags) {
	IM_STREAM* stream = calloc(sizeof(IM_STREAM), 1);
	if (!stream) {
#if _DEBUG
		printf("Failed to allocate stream.\n");
#endif
		return stream;
	}
	stream->buffer = buffer;
	stream->handle = handler(STREAMFILE_NOBUFFER, flags, &inmemory_stream_procs, stream);
	if (!stream->handle) {
#if _DEBUG
		printf("Failed to create stream.\n");
#endif
		inmemory_stream_free(stream);
		return NULL;
	}
	return stream;
}

void inmemory_stream_free(IM_STREAM* stream) {
	if (stream) {
		if (stream->buffer) {
			buffer_free(stream->buffer);
			stream->buffer = NULL;
		}
		free(stream);
		stream = NULL;
	}
}