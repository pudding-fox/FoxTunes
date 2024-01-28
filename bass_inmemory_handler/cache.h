#include "buffer.h"

BOOL cache_add(const wchar_t* file, const BUFFER* buffer);

BOOL cache_acquire(const wchar_t* file, const BUFFER** buffer);

BOOL cache_release(const wchar_t* file);