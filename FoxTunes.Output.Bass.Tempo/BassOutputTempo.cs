using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class BassOutputTempo : BassOutputEffect, IOutputTempo, IStandardComponent, IDisposable
    {
        public override bool Enabled
        {
            get
            {
                return this.EnabledElement.Value;
            }
            set
            {
                this.EnabledElement.Value = value;
                if (this.IsInitialized)
                {
                    this.Configuration.Save();
                }
            }
        }

        public float Tempo
        {
            get
            {
                return Convert.ToSingle(this.TempoElement.Value);
            }
            set
            {
                this.TempoElement.Value = value;
                if (this.IsInitialized)
                {
                    this.Configuration.Save();
                }
            }
        }

        protected virtual void OnTempoChanged()
        {
            if (this.TempoChanged != null)
            {
                this.TempoChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Tempo");
        }

        public event EventHandler TempoChanged;

        public float Pitch
        {
            get
            {
                return Convert.ToSingle(this.PitchElement.Value);
            }
            set
            {
                this.PitchElement.Value = value;
                if (this.IsInitialized)
                {
                    this.Configuration.Save();
                }
            }
        }

        protected virtual void OnPitchChanged()
        {
            if (this.PitchChanged != null)
            {
                this.PitchChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Pitch");
        }

        public event EventHandler PitchChanged;

        public float Rate
        {
            get
            {
                return Convert.ToSingle(this.RateElement.Value);
            }
            set
            {
                this.RateElement.Value = value;
                if (this.IsInitialized)
                {
                    this.Configuration.Save();
                }
            }
        }

        protected virtual void OnRateChanged()
        {
            if (this.RateChanged != null)
            {
                this.RateChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Rate");
        }

        public event EventHandler RateChanged;

        public bool AAFilter
        {
            get
            {
                return this.AAFilterElement.Value;
            }
            set
            {
                this.AAFilterElement.Value = value;
                if (this.IsInitialized)
                {
                    this.Configuration.Save();
                }
            }
        }

        protected virtual void OnAAFilterChanged()
        {
            if (this.AAFilterChanged != null)
            {
                this.AAFilterChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("AAFilter");
        }

        public event EventHandler AAFilterChanged;

        public byte AAFilterLength
        {
            get
            {
                return Convert.ToByte(this.AAFilterLengthElement.Value);
            }
            set
            {
                this.AAFilterLengthElement.Value = value;
                if (this.IsInitialized)
                {
                    this.Configuration.Save();
                }
            }
        }

        protected virtual void OnAAFilterLengthChanged()
        {
            if (this.AAFilterLengthChanged != null)
            {
                this.AAFilterLengthChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("AAFilterLength");
        }

        public event EventHandler AAFilterLengthChanged;

        public ICore Core { get; private set; }

        public BooleanConfigurationElement EnabledElement { get; private set; }

        public DoubleConfigurationElement TempoElement { get; private set; }

        public DoubleConfigurationElement PitchElement { get; private set; }

        public DoubleConfigurationElement RateElement { get; private set; }

        public BooleanConfigurationElement AAFilterElement { get; private set; }

        public IntegerConfigurationElement AAFilterLengthElement { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.Core = core;
            this.EnabledElement = this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputTempoStreamComponentBehaviourConfiguration.ENABLED
            );
            this.TempoElement = this.Configuration.GetElement<DoubleConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputTempoStreamComponentBehaviourConfiguration.TEMPO
            );
            this.PitchElement = this.Configuration.GetElement<DoubleConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputTempoStreamComponentBehaviourConfiguration.PITCH
            );
            this.RateElement = this.Configuration.GetElement<DoubleConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputTempoStreamComponentBehaviourConfiguration.RATE
            );
            this.AAFilterElement = this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputTempoStreamComponentBehaviourConfiguration.AA_FILTER
            );
            this.AAFilterLengthElement = this.Configuration.GetElement<IntegerConfigurationElement>(
                 BassOutputConfiguration.SECTION,
                 BassOutputTempoStreamComponentBehaviourConfiguration.AA_FILTER_LENGTH
             );
            this.EnabledElement.ValueChanged += this.OnEnabledChanged;
            this.TempoElement.ValueChanged += this.OnTempoChanged;
            this.PitchElement.ValueChanged += this.OnPitchChanged;
            this.RateElement.ValueChanged += this.OnRateChanged;
            this.AAFilterElement.ValueChanged += this.OnAAFilterChanged;
            this.AAFilterLengthElement.ValueChanged += this.OnAAFilterLengthChanged;
        }

        protected virtual void OnEnabledChanged(object sender, EventArgs e)
        {
            this.OnEnabledChanged();
        }

        protected virtual void OnTempoChanged(object sender, EventArgs e)
        {
            this.OnTempoChanged();
        }

        protected virtual void OnPitchChanged(object sender, EventArgs e)
        {
            this.OnPitchChanged();
        }

        protected virtual void OnRateChanged(object sender, EventArgs e)
        {
            this.OnRateChanged();
        }

        protected virtual void OnAAFilterChanged(object sender, EventArgs e)
        {
            this.OnAAFilterChanged();
        }

        protected virtual void OnAAFilterLengthChanged(object sender, EventArgs e)
        {
            this.OnAAFilterLengthChanged();
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.EnabledElement != null)
            {
                this.EnabledElement.ValueChanged -= this.OnEnabledChanged;
            }
            if (this.TempoElement != null)
            {
                this.TempoElement.ValueChanged -= this.OnTempoChanged;
            }
            if (this.PitchElement != null)
            {
                this.PitchElement.ValueChanged -= this.OnPitchChanged;
            }
            if (this.RateElement != null)
            {
                this.RateElement.ValueChanged -= this.OnRateChanged;
            }
            if (this.AAFilterElement != null)
            {
                this.AAFilterElement.ValueChanged -= this.OnAAFilterChanged;
            }
            if (this.AAFilterLengthElement != null)
            {
                this.AAFilterLengthElement.ValueChanged -= this.OnAAFilterLengthChanged;
            }
        }

        ~BassOutputTempo()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
