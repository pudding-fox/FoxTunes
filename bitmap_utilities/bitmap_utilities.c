#include "bitmap_utilities.h"

//I have no idea how to prevent linking against this routine in msvcrt.
//It doesn't exist on Windows XP.
//Hopefully it doesn't do anything important.
int _except_handler4_common() {
	return 0;
}

ColorPalette* WINAPI create_palette(Int32Color* colors, INT32 count, INT32 flags) {
	ColorPalette* palette = (ColorPalette*)calloc(sizeof(ColorPalette), 1);
	if (!palette) {
		return NULL;
	}
	palette->Colors = (Int32Color*)calloc(sizeof(Int32Color), count);
	if (!palette->Colors) {
		destroy_palette(&palette);
		return NULL;
	}
	memcpy(palette->Colors, colors, sizeof(Int32Color) * count);
	palette->Count = count;
	palette->Flags = flags;
	return palette;
}

BOOL WINAPI destroy_palette(ColorPalette** palette) {
	if (!palette || !*palette) {
		return FALSE;
	}
	free(*palette);
	*palette = NULL;
	return TRUE;
}

BOOL get_color(ColorPalette* palette, INT32 position, Int32Color* color) {
	if (!palette || palette->Count < position) {
		return FALSE;
	}
	*color = palette->Colors[position];
	return TRUE;
}

BOOL write_color(ColorPalette* palette, INT32 position, BYTE* buffer) {
	Int32Color color;
	if (!get_color(palette, position, &color)) {
		return FALSE;
	}
	memset(buffer + 0, color.Blue, 1);
	memset(buffer + 1, color.Green, 1);
	memset(buffer + 2, color.Red, 1);
	memset(buffer + 3, color.Alpha, 1);
	return TRUE;
}

BOOL blend_color(ColorPalette* palette, INT32 position, BYTE* buffer) {
	Int32Color color1;
	Int32Color color2;
	Int32Color color3;
	color1.Blue = *(buffer + 0);
	color1.Green = *(buffer + 1);
	color1.Red = *(buffer + 2);
	color1.Alpha = *(buffer + 3);
	if (!get_color(palette, position, &color2)) {
		return FALSE;
	}
	if (color2.Alpha < 0xff) {
		color3.Blue = ((color2.Blue * color2.Alpha) + (color1.Blue * (255 - color2.Alpha))) / 255;
		color3.Green = ((color2.Green * color2.Alpha) + (color1.Green * (255 - color2.Alpha))) / 255;
		color3.Red = ((color2.Red * color2.Alpha) + (color1.Red * (255 - color2.Alpha))) / 255;
		//color3.Alpha = color1.Alpha; //TODO: Not sure what the alpha should be.
		color3.Alpha = 0xff;
		memset(buffer + 0, color3.Blue, 1);
		memset(buffer + 1, color3.Green, 1);
		memset(buffer + 2, color3.Red, 1);
		memset(buffer + 3, color3.Alpha, 1);
	}
	else {
		memset(buffer + 0, color2.Blue, 1);
		memset(buffer + 1, color2.Green, 1);
		memset(buffer + 2, color2.Red, 1);
		memset(buffer + 3, color2.Alpha, 1);
	}
	return TRUE;
}

BOOL WINAPI draw_rectangles(RenderInfo* info, Int32Rect* rectangles, INT32 count) {
	BOOL result = TRUE;
	for (INT32 position = 0; position < count; position++) {
		Int32Rect rectangle = rectangles[position];
		result &= draw_rectangle(info, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}
	return result;
}

BOOL WINAPI draw_blended_rectangle(RenderInfo* info, INT32 x, INT32 y, INT32 width, INT32 height) {
	BYTE* topLeft = info->Buffer + ((x * info->BytesPerPixel) + (y * info->Stride));

	for (INT32 xposition = 0; xposition < width; xposition++)
	{
		for (INT32 yposition = 0; yposition < height; yposition++)
		{
			INT32 color;
			if ((info->Palette->Flags & COLOR_FROM_X) == COLOR_FROM_X)
			{
				color = (INT32)(((float)(x + xposition) / info->Width) * info->Palette->Count);
			}
			else if ((info->Palette->Flags & COLOR_FROM_Y) == COLOR_FROM_Y)
			{
				color = (INT32)(((float)(y + yposition) / info->Height) * info->Palette->Count);
			}
			else
			{
				color = 0;
			}
			BYTE* pixel = topLeft + ((xposition * info->BytesPerPixel) + (yposition * info->Stride));
			if (!blend_color(info->Palette, color, pixel))
			{
				return FALSE;
			}
		}
	}

	return TRUE;
}

BOOL WINAPI draw_flat_rectangle(RenderInfo* info, INT32 x, INT32 y, INT32 width, INT32 height) {
	BYTE* topLeft = info->Buffer + ((x * info->BytesPerPixel) + (y * info->Stride));

	//Set initial pixel.
	if (!write_color(info->Palette, 0, topLeft)) {
		return FALSE;
	}

	//Fill first line by copying initial pixel.
	INT32 position = 1;
	BYTE* linePosition = topLeft + info->BytesPerPixel;
	while (position < width)
	{
		//Double the number of pixels we copy until there isn't enough room.
		if (position * 2 < width)
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

BOOL WINAPI draw_horizontal_line(RenderInfo* info, INT32 color, INT32 x, INT32 y, INT32 width) {
	BYTE* topLeft = info->Buffer + ((x * info->BytesPerPixel) + (y * info->Stride));

	//Set initial pixel.
	if (!write_color(info->Palette, color, topLeft)) {
		return FALSE;
	}

	INT32 position = 1;
	BYTE* linePosition = topLeft + info->BytesPerPixel;
	while (position < width)
	{
		//Double the number of pixels we copy until there isn't enough room.
		if (position * 2 < width)
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
	return TRUE;
}

BOOL WINAPI draw_vertical_line(RenderInfo* info, INT32 color, INT32 x, INT32 y, INT32 height) {
	BYTE* topLeft = info->Buffer + ((x * info->BytesPerPixel) + (y * info->Stride));

	//Set initial pixel.
	if (!write_color(info->Palette, color, topLeft)) {
		return FALSE;
	}

	INT32 position = 1;
	BYTE* linePosition = topLeft + info->Stride;
	while (position < height)
	{
		memcpy(linePosition, topLeft, info->BytesPerPixel);
		linePosition += info->Stride;
		position++;
	}

	return TRUE;
}

BOOL WINAPI draw_vertical_gradient_rectangle(RenderInfo* info, INT32 x, INT32 y, INT32 width, INT32 height) {
	BOOL result = TRUE;
	for (INT32 position = 0; position < height; position++) {
		INT32 color = (INT32)(((float)(y + position) / info->Height) * info->Palette->Count);
		result &= draw_horizontal_line(info, color, x, y + position, width);
	}
	return result;
}

BOOL WINAPI draw_horizontal_gradient_rectangle(RenderInfo* info, INT32 x, INT32 y, INT32 width, INT32 height) {
	BOOL result = TRUE;
	for (INT32 position = 0; position < width; position++) {
		INT32 color = (INT32)(((float)(x + position) / info->Width) * info->Palette->Count);
		result &= draw_vertical_line(info, color, x + position, y, height);
	}
	return result;
}

BOOL WINAPI draw_gradient_rectangle(RenderInfo* info, INT32 x, INT32 y, INT32 width, INT32 height) {
	if ((info->Palette->Flags & COLOR_FROM_X) == COLOR_FROM_X) {
		return draw_horizontal_gradient_rectangle(info, x, y, width, height);
	}
	else if ((info->Palette->Flags & COLOR_FROM_Y) == COLOR_FROM_Y) {
		return draw_vertical_gradient_rectangle(info, x, y, width, height);
	}
	else {
		return draw_flat_rectangle(info, x, y, width, height);
	}
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

	if ((info->Palette->Flags & ALPHA_BLENDING) == ALPHA_BLENDING)
	{
		return draw_blended_rectangle(info, x, y, width, height);
	}

	if (info->Palette->Count == 1)
	{
		return draw_flat_rectangle(info, x, y, width, height);
	}
	else
	{
		return draw_gradient_rectangle(info, x, y, width, height);
	}
}

BOOL WINAPI draw_lines(RenderInfo* info, Int32Point* points, INT32 dimentions, INT32 count) {
	BOOL result = TRUE;
	for (INT32 dimention = 0; dimention < dimentions; dimention++)
	{
		INT32 offset = count * dimention;
		for (INT32 position = 0; position < count - 1; position++)
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
	if (!write_color(info->Palette, 0, source)) {
		return FALSE;
	}

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

BOOL WINAPI  draw_pixels(RenderInfo* info, Int32Pixel* pixels, INT32 count) {
	BOOL result = TRUE;
	for (INT32 position = 0; position < count; position++) {
		Int32Pixel pixel = pixels[position];
		result &= draw_pixel(info, pixel.Color, pixel.X, pixel.Y);
	}
	return result;
}

BOOL WINAPI  draw_pixel(RenderInfo* info, INT32 color, INT32 x, INT32 y)
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

	return write_color(info->Palette, color, buffer);
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

BOOL WINAPI clear(RenderInfo* info, Int32Color* color) {
	if (color)
	{
		memset(info->Buffer + 0, color->Blue, 1);
		memset(info->Buffer + 1, color->Green, 1);
		memset(info->Buffer + 2, color->Red, 1);
		memset(info->Buffer + 3, color->Alpha, 1);
		for (INT32 a = info->BytesPerPixel, b = info->Height * info->Stride; a < b; a += info->BytesPerPixel)
		{
			memcpy(info->Buffer + a, info->Buffer, info->BytesPerPixel);
		}
	}
	else
	{
		memset(info->Buffer, 0, info->Height * info->Stride);
	}
	return TRUE;
}