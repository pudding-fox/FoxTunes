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
            if (from.Bottom >= to.Top - PROXIMITY && from.Top <= to.Bottom + PROXIMITY)
            {
                if (inside)
                {
                    if ((Math.Abs(from.Left - to.Left) <= PROXIMITY))
                    {
                        offset.X = -(from.Left - to.Left);
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap left.");
                    }
                    if ((Math.Abs(from.Right - to.Right) <= PROXIMITY))
                    {
                        offset.X = -(from.Right - to.Right);
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                    }
                }
                else
                {
                    if (Math.Abs(from.Right - to.Left) <= PROXIMITY)
                    {
                        offset.X = -(from.Right - to.Left);
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap left.");
                    }
                    else if (Math.Abs(from.Left - to.Left) <= PROXIMITY)
                    {
                        offset.X = -(from.Left - to.Left);
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap left.");
                    }
                    if (Math.Abs(from.Left - to.Right) <= PROXIMITY)
                    {
                        offset.X = -(from.Left - to.Right);
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                    }
                    else if (Math.Abs(from.Right - to.Right) <= PROXIMITY)
                    {
                        offset.X = -(from.Right - to.Right);
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                    }
                }
            }
            if (from.Right >= to.Left - PROXIMITY && from.Left <= to.Right + PROXIMITY)
            {
                if (inside)
                {
                    if (Math.Abs(from.Top - to.Top) <= PROXIMITY)
                    {
                        offset.Y = -(from.Top - to.Top);
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                    }
                    if (Math.Abs(from.Bottom - to.Bottom) <= PROXIMITY)
                    {
                        offset.Y = -(from.Bottom - to.Bottom);
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                    }
                }
                else
                {
                    if (Math.Abs(from.Bottom - to.Top) <= PROXIMITY)
                    {
                        offset.Y = -(from.Bottom - to.Top);
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                    }
                    else if (Math.Abs(from.Top - to.Top) <= PROXIMITY)
                    {
                        offset.Y = -(from.Top - to.Top);
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                    }
                    if (Math.Abs(from.Top - to.Bottom) <= PROXIMITY)
                    {
                        offset.Y = -(from.Top - to.Bottom);
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                    }
                    else if (Math.Abs(from.Bottom - to.Bottom) <= PROXIMITY)
                    {
                        offset.Y = -(from.Bottom - to.Bottom);
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                    }
                }
            }
            return result;
        }

        public static SnapDirection SnapResize(Rectangle from, Rectangle to, ref Rectangle offset, ResizeDirection direction, bool inside)
        {
            var result = SnapDirection.None;
            if (from.Right >= to.Left - PROXIMITY && from.Left <= to.Right + PROXIMITY)
            {
                if ((direction & ResizeDirection.Top) == ResizeDirection.Top)
                {
                    if (inside && Math.Abs(from.Top - to.Bottom) <= PROXIMITY)
                    {
                        offset.Y = from.Top - to.Bottom;
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                    }
                    else if (Math.Abs(from.Top - to.Top) <= PROXIMITY)
                    {
                        offset.Y = from.Top - to.Top;
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                    }
                }
                if ((direction & ResizeDirection.Bottom) == ResizeDirection.Bottom)
                {
                    if (inside && Math.Abs(from.Bottom - to.Top) <= PROXIMITY)
                    {
                        offset.Height = to.Top - from.Bottom;
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                    }
                    else if (Math.Abs(from.Bottom - to.Bottom) <= PROXIMITY)
                    {
                        offset.Height = to.Bottom - from.Bottom;
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                    }
                }
            }
            if (from.Bottom >= to.Top - PROXIMITY && from.Top <= to.Bottom + PROXIMITY)
            {
                if ((direction & ResizeDirection.Right) == ResizeDirection.Right)
                {
                    if (inside && Math.Abs(from.Right - to.Left) <= PROXIMITY)
                    {
                        offset.Width = to.Left - from.Right;
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                    }
                    else if (Math.Abs(from.Right - to.Right) <= PROXIMITY)
                    {
                        offset.Width = to.Right - from.Right;
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                    }
                }
                if ((direction & ResizeDirection.Left) == ResizeDirection.Left)
                {
                    if (inside && Math.Abs(from.Left - to.Right) <= PROXIMITY)
                    {
                        offset.X = from.Left - to.Right;
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap left.");
                    }
                    else if (Math.Abs(from.Left - to.Left) <= PROXIMITY)
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
