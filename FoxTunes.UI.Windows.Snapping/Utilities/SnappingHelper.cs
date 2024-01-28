using FoxTunes.Interfaces;
using System;
using System.Drawing;

namespace FoxTunes
{
    public static class SnappingHelper
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        static SnappingHelper()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration != null)
            {
                Proximity = configuration.GetElement<IntegerConfigurationElement>(
                    WindowSnappingBehaviourConfiguration.SECTION,
                    WindowSnappingBehaviourConfiguration.PROXIMITY
                );
            }
        }

        private static readonly IntegerConfigurationElement Proximity;

        public static SnapDirection Snap(ref Rectangle from, Rectangle to, bool inside, bool resize)
        {
            var result = SnapDirection.None;
            var proximity = Proximity.Value;
            if (from.Bottom >= to.Top - proximity && from.Top <= to.Bottom + proximity)
            {
                if (inside)
                {
                    if ((Math.Abs(from.Left - to.Left) <= proximity))
                    {
                        if (resize)
                        {
                            from.Width += from.Left - to.Left;
                        }
                        from.X += -(from.Left - to.Left);
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap inside left.");
                    }
                    if ((Math.Abs(from.Right - to.Right) <= proximity))
                    {
                        if (resize)
                        {
                            from.Width += -(from.Right - to.Right);
                        }
                        else
                        {
                            from.X += -(from.Right - to.Right);
                        }
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap inside right.");
                    }
                }
                else
                {
                    if (Math.Abs(from.Right - to.Left) <= proximity)
                    {
                        if (resize)
                        {
                            from.Width += -(from.Right - to.Left);
                        }
                        else
                        {
                            from.X += -(from.Right - to.Left);
                        }
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap left.");
                    }
                    else if (Math.Abs(from.Left - to.Left) <= proximity)
                    {
                        if (resize)
                        {
                            from.Width += from.Left - to.Left;
                        }
                        from.X += -(from.Left - to.Left);
                        result |= SnapDirection.Left;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap left.");
                    }
                    if (Math.Abs(from.Left - to.Right) <= proximity)
                    {
                        if (resize)
                        {
                            from.Width += from.Left - to.Right;
                        }
                        from.X += -(from.Left - to.Right);
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                    }
                    else if (Math.Abs(from.Right - to.Right) <= proximity)
                    {
                        if (resize)
                        {
                            from.Width += -(from.Right - to.Right);
                        }
                        else
                        {
                            from.X += -(from.Right - to.Right);
                        }
                        result |= SnapDirection.Right;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap right.");
                    }
                }
            }
            if (from.Right >= to.Left - proximity && from.Left <= to.Right + proximity)
            {
                if (inside)
                {
                    if (Math.Abs(from.Top - to.Top) <= proximity)
                    {
                        if (resize)
                        {
                            from.Height += from.Top - to.Top;
                        }
                        from.Y += -(from.Top - to.Top);
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap inside top.");
                    }
                    if (Math.Abs(from.Bottom - to.Bottom) <= proximity)
                    {
                        if (resize)
                        {
                            from.Height += -(from.Bottom - to.Bottom);
                        }
                        else
                        {
                            from.Y += -(from.Bottom - to.Bottom);
                        }
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap inside bottom.");
                    }
                }
                else
                {
                    if (Math.Abs(from.Bottom - to.Top) <= proximity)
                    {
                        if (resize)
                        {
                            from.Height += -(from.Bottom - to.Top);
                        }
                        else
                        {
                            from.Y += -(from.Bottom - to.Top);
                        }
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                    }
                    else if (Math.Abs(from.Top - to.Top) <= proximity)
                    {
                        if (resize)
                        {
                            from.Height += from.Top - to.Top;
                        }
                        from.Y += -(from.Top - to.Top);
                        result |= SnapDirection.Top;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap top.");
                    }
                    if (Math.Abs(from.Top - to.Bottom) <= proximity)
                    {
                        if (resize)
                        {
                            from.Height += from.Top - to.Bottom;
                        }
                        from.Y += -(from.Top - to.Bottom);
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                    }
                    else if (Math.Abs(from.Bottom - to.Bottom) <= proximity)
                    {
                        if (resize)
                        {
                            from.Height += -(from.Bottom - to.Bottom);
                        }
                        else
                        {
                            from.Y += -(from.Bottom - to.Bottom);
                        }
                        result |= SnapDirection.Bottom;
                        Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snap bottom.");
                    }
                }
            }
            return result;
        }

        public static SnapDirection IsSnapped(Rectangle from, Rectangle to)
        {
            var result = SnapDirection.None;
            if (from.Left == to.Right)
            {
                result |= SnapDirection.Left;
                Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snapped left.");
            }
            if (from.Right == to.Left)
            {
                result |= SnapDirection.Right;
                Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snapped right.");
            }
            if (from.Top == to.Bottom)
            {
                result |= SnapDirection.Top;
                Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snapped top.");
            }
            if (from.Bottom == to.Top)
            {
                result |= SnapDirection.Bottom;
                Logger.Write(typeof(SnappingHelper), LogLevel.Debug, "Snapped bottom.");
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
