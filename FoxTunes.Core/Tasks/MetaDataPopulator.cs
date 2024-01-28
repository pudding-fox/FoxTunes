using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;

namespace FoxTunes.Tasks
{
    public class MetaDataPopulator : BaseComponent, IReportsProgress, IDisposable
    {
        public readonly object SyncRoot = new object();

        private MetaDataPopulator()
        {
            this.Commands = new ThreadLocal<MetaDataPopulatorCommands>(true);
        }

        public MetaDataPopulator(IDatabaseContext databaseContext, IDbTransaction transaction, string prefix) : this()
        {
            this.DatabaseContext = databaseContext;
            this.Transaction = transaction;
            this.Prefix = prefix;
        }

        public IDatabaseContext DatabaseContext { get; private set; }

        public IDbTransaction Transaction { get; private set; }

        public string Prefix { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        private ThreadLocal<MetaDataPopulatorCommands> Commands { get; set; }

        private string _Name { get; set; }

        public string Name
        {
            get
            {
                return this._Name;
            }
            protected set
            {
                this._Name = value;
                this.OnNameChanged();
            }
        }

        protected virtual void OnNameChanged()
        {
            if (this.NameChanged != null)
            {
                this.NameChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Name");
        }

        public event EventHandler NameChanged = delegate { };

        private string _Description { get; set; }

        public string Description
        {
            get
            {
                return this._Description;
            }
            protected set
            {
                this._Description = value;
                this.OnDescriptionChanged();
            }
        }

        protected virtual void OnDescriptionChanged()
        {
            if (this.DescriptionChanged != null)
            {
                this.DescriptionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Description");
        }

        public event EventHandler DescriptionChanged = delegate { };

        private int _Position { get; set; }

        public int Position
        {
            get
            {
                return this._Position;
            }
            protected set
            {
                this._Position = value;
                this.OnPositionChanged();
            }
        }

        protected virtual void OnPositionChanged()
        {
            if (this.PositionChanged != null)
            {
                this.PositionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Position");
        }

        public event EventHandler PositionChanged = delegate { };

        private int _Count { get; set; }

        public int Count
        {
            get
            {
                return this._Count;
            }
            protected set
            {
                this._Count = value;
                this.OnCountChanged();
            }
        }

        protected virtual void OnCountChanged()
        {
            if (this.CountChanged != null)
            {
                this.CountChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Count");
        }

        public event EventHandler CountChanged = delegate { };

        public bool IsIndeterminate
        {
            get
            {
                return false;
            }
        }

        public event EventHandler IsIndeterminateChanged = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            base.InitializeComponent(core);
        }

        public void Populate(IEnumerable<IFileData> fileDatas)
        {
            this.Name = "Populating meta data";
            this.Position = 0;
            this.Count = fileDatas.Count();

            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;

            fileDatas.AsParallel().ForAll(fileData =>
            {
                var commands = this.GetOrAddCommands();
                var metaDataSource = this.MetaDataSourceFactory.Create(fileData.FileName);

                commands.MetaDataParameters["itemId"] = fileData.Id;
                commands.PropertyParameters["itemId"] = fileData.Id;
                commands.ImageParameters["itemId"] = fileData.Id;

                foreach (var metaDataItem in metaDataSource.MetaDatas)
                {
                    commands.MetaDataParameters["name"] = metaDataItem.Name;
                    commands.MetaDataParameters["numericValue"] = metaDataItem.NumericValue;
                    commands.MetaDataParameters["textValue"] = metaDataItem.TextValue;
                    commands.MetaDataParameters["fileValue"] = metaDataItem.FileValue;
                    commands.MetaDataCommand.ExecuteNonQuery();
                }

                foreach (var propertyItem in metaDataSource.Properties)
                {
                    commands.PropertyParameters["name"] = propertyItem.Name;
                    commands.PropertyParameters["numericValue"] = propertyItem.NumericValue;
                    commands.PropertyParameters["textValue"] = propertyItem.TextValue;
                    commands.PropertyCommand.ExecuteNonQuery();
                }

                foreach (var imageItem in metaDataSource.Images)
                {
                    commands.ImageParameters["fileName"] = imageItem.FileName;
                    commands.ImageParameters["imageType"] = imageItem.ImageType;
                    commands.ImageCommand.ExecuteNonQuery();
                }

                if (position % interval == 0)
                {
                    lock (this.SyncRoot)
                    {
                        this.Description = new FileInfo(fileData.FileName).Name;
                        this.Position = position;
                    }
                }

                Interlocked.Increment(ref position);
            });
        }

        private MetaDataPopulatorCommands GetOrAddCommands()
        {
            if (this.Commands.IsValueCreated)
            {
                return this.Commands.Value;
            }
            return this.Commands.Value = new MetaDataPopulatorCommands(this.DatabaseContext, this.Transaction, this.Prefix);
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
            foreach (var commands in this.Commands.Values)
            {
                commands.Dispose();
            }
            this.Commands.Dispose();
        }

        private class MetaDataPopulatorCommands : BaseComponent
        {
            public MetaDataPopulatorCommands(IDatabaseContext databaseContext, IDbTransaction transaction, string prefix)
            {
                var metaDataParameters = default(IDbParameterCollection);
                var propertyParameters = default(IDbParameterCollection);
                var imageParameters = default(IDbParameterCollection);

                this.MetaDataCommand = databaseContext.Connection.CreateCommand(
                    string.Format(Resources.AddMetaDataItems, prefix),
                    new[] { "itemId", "name", "numericValue", "textValue", "fileValue" },
                    out metaDataParameters
                );
                this.PropertyCommand = databaseContext.Connection.CreateCommand(
                    string.Format(Resources.AddPropertyItems, prefix),
                    new[] { "itemId", "name", "numericValue", "textValue" },
                    out propertyParameters
                );
                this.ImageCommand = databaseContext.Connection.CreateCommand(
                    string.Format(Resources.AddImageItems, prefix),
                    new[] { "itemId", "fileName", "imageType" },
                    out imageParameters
                );

                this.MetaDataCommand.Transaction = transaction;
                this.PropertyCommand.Transaction = transaction;
                this.ImageCommand.Transaction = transaction;

                this.MetaDataParameters = metaDataParameters;
                this.PropertyParameters = propertyParameters;
                this.ImageParameters = imageParameters;
            }

            public IDbCommand MetaDataCommand { get; private set; }

            public IDbCommand PropertyCommand { get; private set; }

            public IDbCommand ImageCommand { get; private set; }

            public IDbParameterCollection MetaDataParameters { get; private set; }

            public IDbParameterCollection PropertyParameters { get; private set; }

            public IDbParameterCollection ImageParameters { get; private set; }

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
                this.MetaDataCommand.Dispose();
                this.PropertyCommand.Dispose();
                this.ImageCommand.Dispose();
            }
        }
    }
}
