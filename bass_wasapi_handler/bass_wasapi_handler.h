#include "bass/bass.h"

#ifndef BASSWASAPIHANDLERDEF
#define BASSWASAPIHANDLERDEF(f) WINAPI f
#endif

__declspec(dllexport)
BOOL BASSWASAPIHANDLERDEF(BASS_WASAPI_HANDLER_Init)(int device, DWORD freq, DWORD chans, DWORD flags, float buffer, float period, void *user);

__declspec(dllexport)
BOOL BASSWASAPIHANDLERDEF(BASS_WASAPI_HANDLER_Free)();

__declspec(dllexport)
BOOL BASSWASAPIHANDLERDEF(BASS_WASAPI_HANDLER_StreamGet)(DWORD* handle);

__declspec(dllexport)
BOOL BASSWASAPIHANDLERDEF(BASS_WASAPI_HANDLER_StreamSet)(DWORD handle);

DWORD CALLBACK BASS_WASAPI_HANDLER_StreamProc(void *buffer, DWORD length, void *user);