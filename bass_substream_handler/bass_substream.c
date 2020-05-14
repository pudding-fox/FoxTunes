#if _DEBUG
#include <stdio.h>
#endif

#include "bass_substream.h"

typedef struct {
	QWORD offset;
	QWORD length;
	DWORD handle;
	DWORD flags;
} SUBSTREAM;

DWORD CALLBACK substream_proc(HSTREAM handle, void* buffer, DWORD length, void* user) {
	SUBSTREAM* stream = user;
	DWORD remaining = (DWORD)((stream->offset + stream->length) - BASS_ChannelGetPosition(stream->handle, BASS_POS_BYTE));
	if (length > remaining) {
		length = remaining;
	}
	if (length && BASS_ChannelIsActive(stream->handle) == BASS_ACTIVE_PLAYING) {
		length = BASS_ChannelGetData(stream->handle, buffer, length);
	}
	else {
		length = BASS_STREAMPROC_END;
	}
	return length;
}

VOID CALLBACK substream_free(HSYNC handle, DWORD channel, DWORD data, void* user) {
	SUBSTREAM* stream = user;
	if (stream) {
		if ((stream->flags & BASS_STREAM_AUTOFREE) == BASS_STREAM_AUTOFREE) {
			BASS_StreamFree(stream->handle);
		}
		free(stream);
		stream = NULL;
	}
}

HSTREAM substream_create(HSTREAM handle, QWORD offset, QWORD length, DWORD flags) {
	BASS_CHANNELINFO info;
	SUBSTREAM* stream;
	HSTREAM result;
	if (!BASS_ChannelGetInfo(handle, &info)) {
#if _DEBUG
		printf("Failed to get stream info.\n");
#endif
		return BASS_ERROR_UNKNOWN;
	}
	stream = calloc(sizeof(SUBSTREAM), 1);
	if (!stream) {
#if _DEBUG
		printf("Failed to allocate stream.\n");
#endif
		return BASS_ERROR_UNKNOWN;
	}
	stream->handle = handle;
	stream->offset = offset;
	stream->length = length;
	stream->flags = flags;
	result = BASS_StreamCreate(info.freq, info.chans, info.flags, &substream_proc, stream);
	if (result != BASS_ERROR_UNKNOWN) {
		BASS_ChannelSetPosition(handle, offset, BASS_POS_BYTE);
		BASS_ChannelSetSync(result, BASS_SYNC_FREE, 0, &substream_free, stream);
	}
	else {
#if _DEBUG
		printf("Failed to create stream.\n");
#endif
		free(stream);
	}
	return result;
}