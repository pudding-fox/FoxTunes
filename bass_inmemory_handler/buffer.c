#if _DEBUG
#include <stdio.h>
#endif

#include <math.h>
#include "buffer.h"

BOOL buffer_alloc(BUFFER* buffer, QWORD size) {
	DWORD segment_position;
	DWORD segment_count = (DWORD)ceil(
		(double)size / BUFFER_BLOCK_SIZE
	);
	DWORD segment_length;
	buffer->segments = calloc(sizeof(SEGMENT), segment_count);
	buffer->length = size;
	if (!buffer->segments) {
#if _DEBUG
		printf("Could not allocate buffer segments.\n");
#endif
		return FALSE;
	}
	for (segment_position = 0; segment_position < segment_count; segment_position++) {
		if (size > BUFFER_BLOCK_SIZE) {
			segment_length = BUFFER_BLOCK_SIZE;
		}
		else {
			segment_length = (DWORD)size;
		}
		buffer->segments[segment_position].data = malloc(segment_length);
		if (!buffer->segments[segment_position].data) {
#if _DEBUG
			printf("Could not allocate buffer segment.\n");
#endif
			return FALSE;
		}
		buffer->segments[segment_position].length = segment_length;
		size -= segment_length;
	}
	return TRUE;
}

BUFFER* buffer_create(QWORD size) {
	BUFFER* buffer = calloc(sizeof(BUFFER), 1);
	if (buffer) {
		if (!buffer_alloc(buffer, size)) {
			buffer_free(buffer);
			return NULL;
		}
	}
	return buffer;
}

QWORD buffer_read_segment(const BUFFER* buffer, QWORD position, QWORD length, DWORD* const segment_position, QWORD* segment_offset, void* data) {
	QWORD remaining = length - position;
	QWORD segment_remaining = BUFFER_BLOCK_SIZE - *segment_offset;
	if (segment_remaining > remaining) {
		memcpy(
			(BYTE*)data + position,
			(BYTE*)buffer->segments[*segment_position].data + *segment_offset,
			(size_t)remaining
		);
		*segment_offset += remaining;
		return remaining;
	}
	else {
		memcpy(
			(BYTE*)data + position,
			(BYTE*)buffer->segments[*segment_position].data + *segment_offset,
			(size_t)segment_remaining
		);
		(*segment_position)++;
		*segment_offset = 0;
		return segment_remaining;
	}
}

void buffer_read(const BUFFER* buffer, QWORD position, QWORD length, void* data) {
	DWORD segment_position = (DWORD)(position / BUFFER_BLOCK_SIZE);
	QWORD segment_offset = position % BUFFER_BLOCK_SIZE;
	for (position = 0; position < length; ) {
		position += buffer_read_segment(buffer, position, length, &segment_position, &segment_offset, data);
	}
}

QWORD buffer_write_segment(const BUFFER* buffer, QWORD position, QWORD length, DWORD* const segment_position, QWORD* segment_offset, const void* data) {
	QWORD remaining = length - position;
	QWORD segment_remaining = BUFFER_BLOCK_SIZE - *segment_offset;
	if (segment_remaining > remaining) {
		memcpy(
			(BYTE*)buffer->segments[*segment_position].data + *segment_offset,
			(BYTE*)data + position,
			(size_t)remaining
		);
		*segment_offset += remaining;
		return remaining;
	}
	else {
		memcpy(
			(BYTE*)buffer->segments[*segment_position].data + *segment_offset,
			(BYTE*)data + position,
			(size_t)segment_remaining
		);
		(*segment_position)++;
		*segment_offset = 0;
		return segment_remaining;
	}
}

void buffer_write(const BUFFER* buffer, QWORD position, QWORD length, const void* data) {
	DWORD segment_position = (DWORD)(position / BUFFER_BLOCK_SIZE);
	QWORD segment_offset = position % BUFFER_BLOCK_SIZE;
	for (position = 0; position < length; ) {
		position += buffer_write_segment(buffer, position, length, &segment_position, &segment_offset, data);
	}
}

void buffer_free(BUFFER* buffer) {
	DWORD position;
	DWORD count;
	if (buffer) {
		if (buffer->segments) {
			count = (DWORD)ceil(
				(double)buffer->length / BUFFER_BLOCK_SIZE
			);
			for (position = 0; position < count; position++) {
				if (buffer->segments[position].data) {
					free(buffer->segments[position].data);
					buffer->segments[position].data = NULL;
				}
			}
			free(buffer->segments);
			buffer->segments = NULL;
		}
		free(buffer);
		buffer = NULL;
	}
}