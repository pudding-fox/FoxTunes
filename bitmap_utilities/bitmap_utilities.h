#include <wtypes.h>

typedef struct RenderInfo
{
	INT32 BytesPerPixel;

	INT32 Width;

	INT32 Height;

	INT32 Stride;

	BYTE* Buffer;

	INT32 Blue;

	INT32 Green;

	INT32 Red;

	INT32 Alpha;
} RenderInfo;

typedef struct Int32Rect
{
	INT32 X;

	INT32 Y;

	INT32 Width;

	INT32 Height;
} Int32Rect;

BOOL WINAPI draw_rectangles(RenderInfo* info, Int32Rect* rectangles, size_t count);

BOOL WINAPI draw_rectangle(RenderInfo* info, INT32 x, INT32 y, INT32 width, INT32 height);

BOOL WINAPI draw_line(RenderInfo* info, INT32 x1, INT32 y1, INT32 x2, INT32 y2);

BOOL WINAPI  draw_dot(RenderInfo* info, INT32 x, INT32 y);

BOOL WINAPI shift_left(RenderInfo* info, INT32 count);

BOOL WINAPI clear(RenderInfo* info);