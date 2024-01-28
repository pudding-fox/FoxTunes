using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace FoxTunes
{
    public class LoggingBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public static string FILE_NAME = "Log.txt";

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                LoggingBehaviourConfiguration.SECTION,
                LoggingBehaviourConfiguration.TRACE_ELEMENT
            ).ConnectValue(value =>
            {
                if (value)
                {
                    this.EnableTracing();
                }
                else
                {
                    this.DisableTracing();
                }
            });
            base.InitializeComponent(core);
        }

        public virtual void EnableTracing()
        {
            AppDomain.CurrentDomain.FirstChanceException += this.OnFirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;
            Logger.Write(this, LogLevel.Debug, "Tracing enabled.");
        }

        public virtual void DisableTracing()
        {
            AppDomain.CurrentDomain.FirstChanceException -= this.OnFirstChanceException;
            AppDomain.CurrentDomain.UnhandledException -= this.OnUnhandledException;
            Logger.Write(this, LogLevel.Debug, "Tracing disabled.");
        }

        protected virtual void OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            Logger.Write(this, LogLevel.Debug, "Exception (Warn): {0} => {1}", e.Exception.Message, e.Exception.StackTrace);
        }

        protected virtual void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Write(this, LogLevel.Debug, "Exception (Fatal): {0} => {1}", (e.ExceptionObject as Exception).Message, (e.ExceptionObject as Exception).StackTrace);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return LoggingBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
