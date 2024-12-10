﻿using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Artwork : ConfigurableViewModelBase
    {
        public Artwork()
        {
            this.ArtworkType = ArtworkType.FrontCover;
        }

        private ArtworkType _ArtworkType { get; set; }

        public ArtworkType ArtworkType
        {
            get
            {
                return this._ArtworkType;
            }
            set
            {
                this._ArtworkType = value;
                this.OnArtworkTypeChanged();
            }
        }

        protected virtual void OnArtworkTypeChanged()
        {
            if (this.ArtworkTypeChanged != null)
            {
                this.ArtworkTypeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ArtworkType");
        }

        public event EventHandler ArtworkTypeChanged;

        private string _FileName { get; set; }

        public string FileName
        {
            get
            {
                return this._FileName;
            }
            set
            {
                this._FileName = value;
                this.OnFileNameChanged();
            }
        }

        protected virtual void OnFileNameChanged()
        {
            if (this.FileNameChanged != null)
            {
                this.FileNameChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("FileName");
        }

        public event EventHandler FileNameChanged;

        private IFileData _FileData { get; set; }

        public IFileData FileData
        {
            get
            {
                return this._FileData;
            }
            set
            {
                this._FileData = value;
                this.OnFileDataChanged();
            }
        }

        protected virtual void OnFileDataChanged()
        {
            if (this.FileDataChanged != null)
            {
                this.FileDataChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("FileData");
        }

        public event EventHandler FileDataChanged;

        public IArtworkProvider ArtworkProvider { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ILibraryBrowser LibraryBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private bool _Blur { get; set; }

        public bool Blur
        {
            get
            {
                return this._Blur;
            }
            set
            {
                this._Blur = value;
                this.OnBlurChanged();
            }
        }

        protected virtual void OnBlurChanged()
        {
            if (this.BlurChanged != null)
            {
                this.BlurChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Blur");
        }

        public event EventHandler BlurChanged;

        protected override void InitializeComponent(ICore core)
        {
            this.ArtworkProvider = ComponentRegistry.Instance.GetComponent<IArtworkProvider>();
            this.PlaybackManager = Core.Instance.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            this.LibraryBrowser = core.Components.LibraryBrowser;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Dispatch(this.Refresh);
            base.InitializeComponent(core);
        }

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Configuration.GetElement<BooleanConfigurationElement>(
                    ArtworkConfiguration.SECTION,
                    ArtworkConfiguration.BLUR
                ).ConnectValue(value => this.Blur = value);
            }
            base.OnConfigurationChanged();
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.MetaDataUpdated:
                    return this.OnMetaDataUpdated(signal.State as MetaDataUpdatedSignalState);
                case CommonSignals.ImagesUpdated:
                    return this.Refresh();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual Task OnMetaDataUpdated(MetaDataUpdatedSignalState state)
        {
            if (state != null && state.Names != null)
            {
                return this.Refresh(state.Names);
            }
            else
            {
                return this.Refresh(Enumerable.Empty<string>());
            }
        }

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
            this.Dispatch(this.Refresh);
        }

        protected virtual Task Refresh(IEnumerable<string> names)
        {
            if (names != null && names.Any())
            {
                if (!names.Contains(CommonImageTypes.FrontCover, StringComparer.OrdinalIgnoreCase))
                {
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }
            }
            return this.Refresh();
        }

        public virtual async Task Refresh()
        {
            if (this.PlaybackManager == null || this.LibraryBrowser == null)
            {
                return;
            }
            var artworkType = this.ArtworkType;
            var fileName = default(string);
            var fileData = default(IFileData);
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream != null)
            {
                if (outputStream.PlaylistItem.LibraryItem_Id.HasValue)
                {
                    fileData = this.LibraryBrowser.Get(outputStream.PlaylistItem.LibraryItem_Id.Value);
                }
                else
                {
                    fileData = outputStream.PlaylistItem;
                }
                if (!object.ReferenceEquals(this.FileData, fileData))
                {
                    artworkType = ArtworkType.FrontCover;
                }
                fileName = await this.ArtworkProvider.Find(
                    fileData,
                    artworkType
                ).ConfigureAwait(false);
            }
            await Windows.Invoke(() =>
            {
                this.ArtworkType = artworkType;
                this.FileName = fileName;
                this.FileData = fileData;
            }).ConfigureAwait(false);
        }

        public virtual void Emit()
        {
            this.OnFileNameChanged();
            this.OnFileDataChanged();
        }

        public ICommand NextCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.Next);
            }
        }

        public Task Next()
        {
            if (this.ArtworkType == ArtworkType.FrontCover)
            {
                this.ArtworkType = ArtworkType.BackCover;
            }
            else
            {
                this.ArtworkType = ArtworkType.FrontCover;
            }
            return this.Refresh();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Artwork();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            }
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.Dispose(disposing);
        }
    }
}
