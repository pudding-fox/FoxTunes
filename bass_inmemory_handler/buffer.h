#ifndef INMEMORY_BUFFER_H
#define INMEMORY_BUFFER_H

#include "bass/bass.h"

#define BUFFER_BLOCK_SIZE 10000000

typedef struct {
	void* data;
	DWORD length;
} SEGMENT;

typedef struct {
	SEGMENT* segments;
	QWORD length;
} BUFFER;

BUFFER* buffer_create(QWORD size);

void buffer_read(BUFFER* buffer, QWORD position, QWORD length, void* data);

void buffer_write(BUFFER* buffer, QWORD position, QWORD length, void* data);

void buffer_free(BUFFER* buffer);

#endif