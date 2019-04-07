#include "bass/bass.h"

#ifndef BASSINMEMORYHANDLERDSDDEF
#define BASSINMEMORYHANDLERDSDDEF(f) WINAPI f
#endif

__declspec(dllexport)
BOOL BASSINMEMORYHANDLERDSDDEF(BASS_INMEMORY_HANDLER_DSD_Init)();

__declspec(dllexport)
BOOL BASSINMEMORYHANDLERDSDDEF(BASS_INMEMORY_HANDLER_DSD_Free)();

__declspec(dllexport)
HSTREAM BASSINMEMORYHANDLERDSDDEF(BASS_INMEMORY_HANDLER_DSD_StreamCreateFile)(BOOL mem, const void *file, QWORD offset, QWORD length, DWORD flags);