using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedBass;
using FoxTunes.Interfaces;

namespace FoxTunes.Encoder.Bass.Tests
{
    [TestFixture]
    [Explicit]
    public class ResamplerTests : TestBase
    {
        [SetUp]
        public override void SetUp()
        {
            global::ManagedBass.Bass.Init(global::ManagedBass.Bass.NoSoundDevice);
            base.SetUp();
        }

        [TearDown]
        public override void TearDown()
        {
            global::ManagedBass.Bass.Free();
            base.TearDown();
        }

        [TestCase(BassFlags.Default)]
        [TestCase(BassFlags.Float)]
        public void CanResample(BassFlags flags)
        {
            var profiles = ComponentRegistry.Instance.GetComponents<IBassEncoderSettings>().Select(
                settings => settings.Name
            ).ToArray();
            foreach (var profile in profiles)
            {
                foreach (var playlistItem in TestInfo.PlaylistItems)
                {
                    var streamFactory = ComponentRegistry.Instance.GetComponent<IBassStreamFactory>();
                    var stream = streamFactory.CreateBasicStream(playlistItem, flags);
                    using (var resampler = ResamplerFactory.Create(encoderItem, stream, profile))
                    {

                    }
                }
            }
        }
    }
}
