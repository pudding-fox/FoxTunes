#include "bass/bass.h"
#include "bass/bassasio.h"

__declspec(dllexport)
HSTREAM BASSDEF(BASS_StreamCreateGaplessMaster)(DWORD freq, DWORD chans, DWORD flags, void *user);

__declspec(dllexport)
BOOL BASSASIODEF(BASS_ASIO_ChannelEnableGaplessMaster)(BOOL input, DWORD channel, void *user);

__declspec(dllexport)
DWORD BASSDEF(BASS_ChannelGetGaplessPrimary)();

__declspec(dllexport)
BOOL BASSDEF(BASS_ChannelSetGaplessPrimary)(DWORD channel);

__declspec(dllexport)
DWORD BASSDEF(BASS_ChannelGetGaplessSecondary)();

__declspec(dllexport)
BOOL BASSDEF(BASS_ChannelSetGaplessSecondary)(DWORD channel);