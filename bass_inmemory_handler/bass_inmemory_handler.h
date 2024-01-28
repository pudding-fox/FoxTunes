#include "bass/bass.h"

#ifndef BASSINMEMORYHANDLERDEF
#define BASSINMEMORYHANDLERDEF(f) WINAPI f
#endif

__declspec(dllexport)
BOOL BASSINMEMORYHANDLERDEF(BASS_INMEMORY_HANDLER_Init)();

__declspec(dllexport)
BOOL BASSINMEMORYHANDLERDEF(BASS_INMEMORY_HANDLER_Free)();

__declspec(dllexport)
HSTREAM BASSINMEMORYHANDLERDEF(BASS_INMEMORY_HANDLER_StreamCreateFile)(BOOL mem, const void *file, QWORD offset, QWORD length, DWORD flags);