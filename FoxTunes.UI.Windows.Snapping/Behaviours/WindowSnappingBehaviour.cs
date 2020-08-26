using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class WindowSnappingBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        public WindowSnappingBehaviour()
        {
            this.Handles = new List<IntPtr>();
            this.Windows = new List<SnappingWindow>();
        }

        public IList<IntPtr> Handles { get; private set; }

        public IList<SnappingWindow> Windows { get; private set; }

        public ICore Core { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.UserInterface = core.Components.UserInterface;
            this.UserInterface.WindowCreated += this.OnWindowCreated;
            this.UserInterface.WindowDestroyed += this.OnWindowDestroyed;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                WindowSnappingBehaviourConfiguration.SECTION,
                WindowSnappingBehaviourConfiguration.ENABLED
            ).ConnectValue(value =>
            {
                if (value)
                {
                    this.Enable();
                }
                else
                {
                    this.Disable();
                }
            });
            base.InitializeComponent(core);
        }

        protected virtual void OnWindowCreated(object sender, UserInterfaceWindowEventArgs e)
        {
            this.Handles.Add(e.Handle);
        }

        protected virtual void OnWindowDestroyed(object sender, UserInterfaceWindowEventArgs e)
        {
            this.Handles.Remove(e.Handle);
            for (var a = 0; a < this.Windows.Count; a++)
            {
                if (this.Windows[a].Handle != e.Handle)
                {
                    continue;
                }
                this.Windows[a].Dispose();
                this.Windows.RemoveAt(a);
            }
        }

        public void Enable()
        {
            foreach (var handle in this.Handles)
            {
                var window = new SnappingWindow(handle);
                window.InitializeComponent(this.Core);
                this.Windows.Add(window);
            }
        }

        public void Disable()
        {
            foreach (var window in this.Windows)
            {
                window.Dispose();
            }
            this.Windows.Clear();
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return WindowSnappingBehaviourConfiguration.GetConfigurationSections();
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
            if (this.UserInterface != null)
            {
                this.UserInterface.WindowCreated -= this.OnWindowCreated;
                this.UserInterface.WindowDestroyed -= this.OnWindowDestroyed;
            }
        }

        ~WindowSnappingBehaviour()
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
