#include "bass/bass.h"

#ifndef BASSASIOHANDLERDEF
#define BASSASIOHANDLERDEF(f) WINAPI f
#endif

__declspec(dllexport)
BOOL BASSASIOHANDLERDEF(BASS_ASIO_HANDLER_Init)();

__declspec(dllexport)
BOOL BASSASIOHANDLERDEF(BASS_ASIO_HANDLER_Free)();

__declspec(dllexport)
BOOL BASSASIOHANDLERDEF(BASS_ASIO_HANDLER_StreamGet)(DWORD* handle);

__declspec(dllexport)
BOOL BASSASIOHANDLERDEF(BASS_ASIO_HANDLER_StreamSet)(DWORD handle);

__declspec(dllexport)
BOOL BASSASIOHANDLERDEF(BASS_ASIO_HANDLER_ChannelEnable)(BOOL input, DWORD channel, void *user);

DWORD CALLBACK BASS_ASIO_HANDLER_StreamProc(BOOL input, DWORD channel, void *buffer, DWORD length, void *user);