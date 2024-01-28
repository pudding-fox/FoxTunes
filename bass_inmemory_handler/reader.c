#include <stdio.h>
#include "reader.h"
#include "cache.h"

QWORD get_file_length(FILE* file_handle) {
	QWORD length;
	if (fseek(file_handle, 0L, SEEK_END) != 0) {
#if _DEBUG
		char* error = strerror(errno);
		printf("Error seeking file: %s\n", error);
#endif
		return -1;
	}
	length = ftell(file_handle);
	if (fseek(file_handle, 0L, SEEK_SET) != 0) {
#if _DEBUG
		char* error = strerror(errno);
		printf("Error seeking file: %s\n", error);
#endif
		return -1;
	}
	return length;
}

BOOL populate_file_buffer(FILE* file_handle, const BUFFER* buffer) {
	QWORD length;
	QWORD position = 0;
	void* file_buffer = malloc(BUFFER_BLOCK_SIZE);
	if (!file_buffer) {
#if _DEBUG
		printf("Could not allocate temp buffer.\n");
#endif
		return FALSE;
	}
	do {
		length = fread(file_buffer, sizeof(BYTE), BUFFER_BLOCK_SIZE, file_handle);
		if (ferror(file_handle)) {
#if _DEBUG
			char* error = strerror(errno);
			printf("Error opening file: %s\n", error);
#endif
			free(file_buffer);
			return FALSE;
		}
		if (!length) {
			free(file_buffer);
			return TRUE;
		}
		buffer_write(buffer, position, length, file_buffer);
		position += length;
	} while (TRUE);
}

BUFFER* read_file_buffer(const wchar_t* file, QWORD offset, QWORD length) {
	BUFFER* buffer;
	QWORD file_length;
	FILE* file_handle;
	if (cache_acquire(file, &buffer)) {
		return buffer;
	}
	file_handle = _wfopen(file, L"rb");
	if (!file_handle) {
#if _DEBUG
		char* error = strerror(errno);
		printf("Error opening file: %s\n", error);
#endif
		return NULL;
}
	file_length = get_file_length(file_handle);
	if (file_length == -1) {
#if _DEBUG
		printf("Could not determine file length.\n");
#endif
		buffer = NULL;
	}
	else {
		buffer = buffer_create(file_length);
		if (buffer) {
			if (!populate_file_buffer(file_handle, buffer)) {
				buffer_free(buffer);
				buffer = NULL;
			}
		}
	}
	fclose(file_handle);
	if (buffer) {
		cache_add(file, buffer);
	}
	return buffer;
}