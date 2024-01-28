using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FoxTunes
{    /// <summary>
     /// Interaction logic for StaticImage.xaml
     /// </summary>
    [UIComponent("8A4E9DDB-2390-455D-9BA9-57C9A441CD75", role: UIComponentRole.System)]
    public partial class StaticImage : ConfigurableUIComponentBase, IInvocableComponent
    {
        public const string CATEGORY = "B11C3491-8765-4B6F-8AB8-D20E90A38400";

        public StaticImage()
        {
            this.InitializeComponent();
        }

        public DispatcherTimer Timer { get; private set; }

        public string Path { get; private set; }

        public string FileName { get; private set; }

        protected virtual void OnTick(object sender, EventArgs e)
        {
            this.Refresh();
        }

        public override IEnumerable<string> InvocationCategories
        {
            get
            {
                yield return CATEGORY;
            }
        }

        protected override void InitializeComponent(ICore core)
        {
            this.Timer = new DispatcherTimer(DispatcherPriority.Background);
            this.Timer.Tick += this.OnTick;
            base.InitializeComponent(core);
        }

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Configuration.GetElement<TextConfigurationElement>(
                    StaticImageConfiguration.SECTION,
                    StaticImageConfiguration.PATH
                ).ConnectValue(value =>
                {
                    this.Path = value;
                    this.Refresh();
                });
                this.Configuration.GetElement<IntegerConfigurationElement>(
                    StaticImageConfiguration.SECTION,
                    StaticImageConfiguration.INTERVAL
                ).ConnectValue(value =>
                {
                    this.Timer.Interval = TimeSpan.FromSeconds(value);
                    this.Timer.Start();
                });
            }
            base.OnConfigurationChanged();
        }

        public void Refresh()
        {
            if (!Directory.Exists(this.Path))
            {
                return;
            }
            var fileName = default(string);
            var fileNames = FileSystemHelper.EnumerateFiles(
                this.Path,
                "*.*",
                FileSystemHelper.SearchOption.Recursive | FileSystemHelper.SearchOption.Sort
            ).ToArray();
            if (!string.IsNullOrEmpty(this.FileName))
            {
                fileName = fileNames.FirstOrDefault();
            }
            else
            {
                var index = fileNames.IndexOf(this.FileName);
                if (index < 0 || index > fileNames.Length - 1)
                {
                    fileName = fileNames.FirstOrDefault();
                }
                else
                {
                    fileName = fileNames[index + 1];
                }
            }
            this.FileName = fileName;
            if (!string.IsNullOrEmpty(this.FileName))
            {
                this.LoadImage();
            }
        }

        protected virtual void LoadImage()
        {
            this.Image.Source = new BitmapImage(new Uri(this.FileName));
        }

        protected override Task<bool> ShowSettings()
        {
            return this.ShowSettings(
                Strings.StaticImageConfiguration_Path,
                StaticImageConfiguration.SECTION
            );
        }

        public override IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return StaticImageConfiguration.GetConfigurationSections();
        }

        protected override void OnDisposing()
        {
            if (this.Timer != null)
            {
                this.Timer.Tick -= this.OnTick;
            }
            base.OnDisposing();
        }
    }
}
