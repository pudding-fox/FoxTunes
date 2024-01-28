using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Windows;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class PixelSizeConverter : StandardComponent
    {
        public PixelSizeConverter()
        {
            this.Store = new Cache();
        }

        public Cache Store { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public double ScalingFactor { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            ).ConnectValue(value => this.ScalingFactor = value);
            base.InitializeComponent(core);
        }

        public Size Convert(Size size)
        {
            return this.Store.GetOrAdd(
                size,
                this.ScalingFactor,
                () => Windows.ActiveWindow.GetElementPixelSize(
                     size.Width * this.ScalingFactor,
                     size.Height * this.ScalingFactor
                 )
            );
        }

        public void Convert(ref int width, ref int height)
        {
            var size = this.Convert(new Size(width, height));
            width = global::System.Convert.ToInt32(size.Width);
            height = global::System.Convert.ToInt32(size.Height);
        }

        public class Cache
        {
            public Cache()
            {
                this.Store = new ConcurrentDictionary<Key, Size>();
            }

            public ConcurrentDictionary<Key, Size> Store { get; private set; }

            public Size GetOrAdd(Size size, double scalingFactor, Func<Size> factory)
            {
                var key = new Key(size, scalingFactor);
                return this.Store.GetOrAdd(key, factory);
            }

            public void Clear()
            {
                this.Store.Clear();
            }

            public class Key : IEquatable<Key>
            {
                public Key(Size size, double scalingFactor)
                {
                    this.Size = size;
                    this.ScalingFactor = scalingFactor;
                }

                public Size Size { get; private set; }

                public double ScalingFactor { get; private set; }

                public virtual bool Equals(Key other)
                {
                    if (other == null)
                    {
                        return false;
                    }
                    if (object.ReferenceEquals(this, other))
                    {
                        return true;
                    }
                    if (this.Size != other.Size)
                    {
                        return false;
                    }
                    if (this.ScalingFactor != other.ScalingFactor)
                    {
                        return false;
                    }
                    return true;
                }

                public override bool Equals(object obj)
                {
                    return this.Equals(obj as Key);
                }

                public override int GetHashCode()
                {
                    var hashCode = default(int);
                    unchecked
                    {
                        hashCode += this.Size.GetHashCode();
                        hashCode += this.ScalingFactor.GetHashCode();
                    }
                    return hashCode;
                }

                public static bool operator ==(Key a, Key b)
                {
                    if ((object)a == null && (object)b == null)
                    {
                        return true;
                    }
                    if ((object)a == null || (object)b == null)
                    {
                        return false;
                    }
                    if (object.ReferenceEquals((object)a, (object)b))
                    {
                        return true;
                    }
                    return a.Equals(b);
                }

                public static bool operator !=(Key a, Key b)
                {
                    return !(a == b);
                }
            }
        }
    }
}