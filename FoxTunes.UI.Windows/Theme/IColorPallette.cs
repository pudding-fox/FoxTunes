using System;

namespace FoxTunes
{
    public interface IColorPalette
    {
        string Id { get; }

        ColorPaletteRole Role { get; }

        string Name { get; }

        string Description { get; }

        string Value { get; }
    }

    [Flags]
    public enum ColorPaletteRole : byte
    {
        None,
        Visualization
    }
}
