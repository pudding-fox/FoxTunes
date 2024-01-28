#include "bitmap_utilities.h"

//I have no idea how to prevent linking against this routine in msvcrt.
//It doesn't exist on Windows XP.
//Hopefully it doesn't do anything important.
int _except_handler4_common() {
	return 0;
}

BOOL WINAPI draw_rectangles(RenderInfo* info, Int32Rect* rectangles, size_t count) {
	BOOL result = TRUE;
	for (size_t position = 0; position < count; position++) {
		Int32Rect rectangle = rectangles[position];
		result &= draw_rectangle(info, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}
	return result;
}

BOOL WINAPI draw_rectangle(RenderInfo* info, INT32 x, INT32 y, INT32 width, INT32 height) {
	//Check arguments are valid.
	if (x < 0 || y < 0 || width <= 0 || height <= 0)
	{
		return FALSE;
	}

	if (x + width > info->Width || y + height > info->Height)
	{
		return FALSE;
	}

	BYTE* topLeft = info->Buffer + ((x * info->BytesPerPixel) + (y * info->Stride));

	//Set initial pixel.
	memset(topLeft + 0, info->Blue, 1);
	memset(topLeft + 1, info->Green, 1);
	memset(topLeft + 2, info->Red, 1);
	memset(topLeft + 3, info->Alpha, 1);

	//Fill first line by copying initial pixel.
	INT32 position = 1;
	BYTE* linePosition = topLeft + info->BytesPerPixel;
	for (position = 1; position <= width;)
	{
		//Double the number of pixels we copy until there isn't enough room.
		if (position * 2 <= width)
		{
			memcpy(linePosition, topLeft, position * info->BytesPerPixel);
			linePosition += position * info->BytesPerPixel;
			position *= 2;
		}
		//Fill the remainder.
		else
		{
			INT32 remaining = width - position;
			memcpy(linePosition, topLeft, remaining * info->BytesPerPixel);
			break;
		}
	}

	if (height > 1)
	{
		//Fill each other line by copying the first line.
		BYTE* lineStart = topLeft + info->Stride;
		for (position = 1; position < height; position++)
		{
			memcpy(lineStart, topLeft, width * info->BytesPerPixel);
			lineStart += info->Stride;
		}
	}

	return TRUE;
}

BOOL WINAPI draw_lines(RenderInfo* info, Int32Point* points, size_t dimentions, size_t count) {
	BOOL result = TRUE;
	for (size_t dimention = 0; dimention < dimentions; dimention++)
	{
		size_t offset = count * dimention;
		for (size_t position = 0; position < count - 1; position++)
		{
			Int32Point point1 = points[offset + position];
			Int32Point point2 = points[offset + position + 1];
			result &= draw_line(info, point1.X, point1.Y, point2.X, point2.Y);
		}
	}
	return result;
}

BOOL WINAPI draw_line(RenderInfo* info, INT32 x1, INT32 y1, INT32 x2, INT32 y2)
{
	//Check arguments are valid.
	if (x1 < 0 || y1 < 0 || x2 < 0 || y2 < 0)
	{
		return FALSE;
	}

	if (max(x1, x2) >= info->Width || max(y1, y2) >= info->Height)
	{
		return FALSE;
	}

	BYTE* source = info->Buffer + ((x1 * info->BytesPerPixel) + (y1 * info->Stride));

	//Set initial pixel.
	memset(source + 0, info->Blue, 1);
	memset(source + 1, info->Green, 1);
	memset(source + 2, info->Red, 1);
	memset(source + 3, info->Alpha, 1);

	//This code influenced by https://rosettacode.org/wiki/Bitmap/Bresenham's_line_algorithm
	INT32 sx = 0;
	if (x1 == x2)
	{
		sx = 0;
	}
	else if (x1 < x2)
	{
		sx = 1;
	}
	else
	{
		sx = -1;
	}

	INT32 sy = 0;
	if (y1 == y2)
	{
		sy = 0;
	}
	else if (y1 < y2)
	{
		sy = 1;
	}
	else
	{
		sy = -1;
	}

	INT32 dx = abs(x2 - x1);
	INT32 dy = abs(y2 - y1);
	INT32 err = (dx > dy ? dx : -dy) / 2;

	while (x1 != x2 || y1 != y2)
	{
		INT32 e2 = err;
		if (e2 > -dx)
		{
			err -= dy;
			x1 += sx;
		}
		if (e2 < dy)
		{
			err += dx;
			y1 += sy;
		}

		BYTE* destination = info->Buffer + ((x1 * info->BytesPerPixel) + (y1 * info->Stride));
		memcpy(destination, source, info->BytesPerPixel);
	}

	return TRUE;
}

BOOL WINAPI  draw_dots(RenderInfo* info, Int32Pixel* pixels, size_t count) {
	BOOL result = TRUE;
	for (size_t position = 0; position < count; position++) {
		Int32Pixel pixel = pixels[position];
		info->Blue = pixel.Blue;
		info->Green = pixel.Green;
		info->Red = pixel.Red;
		info->Alpha = pixel.Alpha;
		result &= draw_dot(info, pixel.X, pixel.Y);
	}
	return result;
}

BOOL WINAPI  draw_dot(RenderInfo* info, INT32 x, INT32 y)
{
	//Check arguments are valid.
	if (x < 0 || y < 0)
	{
		return FALSE;
	}

	if (x >= info->Width || y >= info->Height)
	{
		return FALSE;
	}

	BYTE* buffer = info->Buffer + ((x * info->BytesPerPixel) + (y * info->Stride));

	memset(buffer + 0, info->Blue, 1);
	memset(buffer + 1, info->Green, 1);
	memset(buffer + 2, info->Red, 1);
	memset(buffer + 3, info->Alpha, 1);

	return TRUE;
}

BOOL WINAPI shift_left(RenderInfo* info, INT32 count)
{
	//Check arguments are valid.
	if (count < 0)
	{
		return FALSE;
	}

	if (count >= info->Width)
	{
		return FALSE;
	}

	for (INT32 y = 0; y < info->Height; y++)
	{
		BYTE* source = info->Buffer + ((count * info->BytesPerPixel) + (y * info->Stride));
		BYTE* destination = info->Buffer + (y * info->Stride);
		memmove(destination, source, (info->Width - count) * info->BytesPerPixel);
	}

	return TRUE;
}

BOOL WINAPI clear(RenderInfo* info) {
	memset(info->Buffer, 0, info->Height * info->Stride);
	return TRUE;
}