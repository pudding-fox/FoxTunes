#include "cache.h"

#define ENTRIES 128

typedef struct {
	const wchar_t file[MAX_PATH + 1];
	WORD count;
	const BUFFER* buffer;
} ENTRY;

static ENTRY entries[ENTRIES];

BOOL cache_entry(const wchar_t* file, DWORD* const index) {
	DWORD position;
	for (position = 0; position < ENTRIES; position++) {
		if (entries[position].file && wcscmp(file, entries[position].file) == 0) {
			*index = position;
			return TRUE;
		}
	}
	return FALSE;
}

BOOL cache_add(const wchar_t* file, const BUFFER* buffer) {
	DWORD position;
	if (cache_entry(file, &position)) {
		//Already exists.
		return FALSE;
	}
	//Look for an empty entry.
	for (position = 0; position < ENTRIES; position++) {
		if (entries[position].count) {
			continue;
		}
		//Initialise the entry.
		wcscpy((wchar_t*)entries[position].file, file);
		entries[position].count = 1;
		entries[position].buffer = buffer;
		return TRUE;
	}
	//The cache is full.
	return FALSE;
}

BOOL cache_acquire(const wchar_t* file, const BUFFER** buffer) {
	DWORD position;
	if (!cache_entry(file, &position)) {
		//Doesn't exist.
		return FALSE;
	}
	//Increment the counter and read the buffer.
	entries[position].count++;
	*buffer = entries[position].buffer;
	return TRUE;
}

BOOL cache_release(const wchar_t* file) {
	DWORD position;
	if (!cache_entry(file, &position)) {
		//Doesn't exist.
		return FALSE;
	}
	//Decrement the counter.
	entries[position].count--;
	if (!entries[position].count) {
		//If the count is zero then clear the entry.
		wcscpy((wchar_t*)entries[position].file, L"");
		entries[position].buffer = NULL;
		return TRUE;
	}
	return FALSE;
}