using FoxTunes.Interfaces;
using System;
using System.Drawing;

namespace FoxTunes
{
    public static class SnappingHelper
    {
        public const int PROXIMITY = 20;

        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static SnapDirection SnapMove(Rectangle from, Rectangle to, ref Point offset, bool inside)
        {
            var result = SnapDirection.None;
            if (from.Bottom >= (to.Top - PROXIMITY) && from.Top <= (to.Bottom + PROXIMITY))
            {
                if (inside)
                {
                    if ((Math.Abs(from.Left - to.Right) <= Math.Abs(offset.X)))
                    {
                        offset.X = to.Right - from.Left;
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap left.");
                    }
                    if ((Math.Abs(from.Left + from.Width - to.Left) <= Math.Abs(offset.X)))
                    {
                        offset.X = to.Left - from.Width - from.Left;
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                    }
                }
                if (Math.Abs(from.Left - to.Left) <= Math.Abs(offset.X))
                {
                    offset.X = to.Left - from.Left;
                    result |= SnapDirection.Left;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap left.");
                }
                if (Math.Abs(from.Left + from.Width - to.Left - to.Width) <= Math.Abs(offset.X))
                {
                    offset.X = to.Left + to.Width - from.Width - from.Left;
                    result |= SnapDirection.Right;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                }
            }
            if (from.Right >= (to.Left - PROXIMITY) && from.Left <= (to.Right + PROXIMITY))
            {
                if (inside)
                {
                    if (Math.Abs(from.Top - to.Bottom) <= Math.Abs(offset.Y))
                    {
                        offset.Y = to.Bottom - from.Top;
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                    }
                    if (Math.Abs(from.Top + from.Height - to.Top) <= Math.Abs(offset.Y))
                    {
                        offset.Y = to.Top - from.Height - from.Top;
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                    }
                }
                if (Math.Abs(from.Top - to.Top) <= Math.Abs(offset.Y))
                {
                    offset.Y = to.Top - from.Top;
                    result |= SnapDirection.Top;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                }
                if (Math.Abs(from.Top + from.Height - to.Top - to.Height) <= Math.Abs(offset.Y))
                {
                    offset.Y = to.Top + to.Height - from.Height - from.Top;
                    result |= SnapDirection.Bottom;
                    Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                }
            }
            return result;
        }

        public static SnapDirection SnapResize(Rectangle from, Rectangle to, ref Rectangle offset, ResizeDirection direction, bool inside)
        {
            var result = SnapDirection.None;
            if (from.Right >= (to.Left - PROXIMITY) && from.Left <= (to.Right + PROXIMITY))
            {
                if ((direction & ResizeDirection.Top) == ResizeDirection.Top)
                {
                    if (inside && Math.Abs(from.Top - to.Bottom) <= Math.Abs(offset.Top))
                    {
                        offset.Y = from.Top - to.Bottom;
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                    }
                    else if (Math.Abs(from.Top - to.Top) <= Math.Abs(offset.Top))
                    {
                        offset.Y = from.Top - to.Top;
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                    }
                }
                if ((direction & ResizeDirection.Bottom) == ResizeDirection.Bottom)
                {
                    if (inside && Math.Abs(from.Bottom - to.Top) <= Math.Abs(offset.Bottom))
                    {
                        offset.Height = to.Top - from.Bottom;
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                    }
                    else if (Math.Abs(from.Bottom - to.Bottom) <= Math.Abs(offset.Bottom))
                    {
                        offset.Height = to.Bottom - from.Bottom;
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                    }
                }
            }
            if (from.Bottom >= (to.Top - PROXIMITY) && from.Top <= (to.Bottom + PROXIMITY))
            {
                if ((direction & ResizeDirection.Right) == ResizeDirection.Right)
                {
                    if (inside && Math.Abs(from.Right - to.Left) <= Math.Abs(offset.Right))
                    {
                        offset.Width = to.Left - from.Right;
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                    }
                    else if (Math.Abs(from.Right - to.Right) <= Math.Abs(offset.Right))
                    {
                        offset.Width = to.Right - from.Right;
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                    }
                }
                if ((direction & ResizeDirection.Left) == ResizeDirection.Left)
                {
                    if (inside && Math.Abs(from.Left - to.Right) <= Math.Abs(offset.Left))
                    {
                        offset.X = from.Left - to.Right;
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap left.");
                    }
                    else if (Math.Abs(from.Left - to.Left) <= Math.Abs(offset.Left))
                    {
                        offset.X = from.Left - to.Left;
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap left.");
                    }
                }
            }
            return result;
        }
    }

    [Flags]
    public enum ResizeDirection : byte
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Left = 4,
        Right = 8
    }

    [Flags]
    public enum SnapDirection : byte
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Left = 4,
        Right = 8
    }
}
