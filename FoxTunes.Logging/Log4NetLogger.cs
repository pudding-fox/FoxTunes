using FoxTunes.Interfaces;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using System;
using System.Collections.Generic;
using System.IO;

namespace FoxTunes
{
    [Component("3DC2C04A-CE99-4416-BC27-068E8AC02F56", ComponentSlots.Logger, priority: ComponentAttribute.PRIORITY_HIGH)]
    public class Log4NetLogger : StandardComponent, ILogger, IConfigurableComponent
    {
        public IConfiguration Configuration { get; private set; }

        public bool IsDebugEnabled(IBaseComponent component)
        {
            return this.IsDebugEnabled(component.GetType());
        }

        public bool IsErrorEnabled(IBaseComponent component)
        {
            return this.IsErrorEnabled(component.GetType());
        }

        public bool IsFatalEnabled(IBaseComponent component)
        {
            return this.IsFatalEnabled(component.GetType());
        }

        public bool IsInfoEnabled(IBaseComponent component)
        {
            return this.IsInfoEnabled(component.GetType());
        }

        public bool IsWarnEnabled(IBaseComponent component)
        {
            return this.IsWarnEnabled(component.GetType());
        }

        public bool IsDebugEnabled(Type type)
        {
            return global::log4net.LogManager.GetLogger(type).IsDebugEnabled;
        }

        public bool IsErrorEnabled(Type type)
        {
            return global::log4net.LogManager.GetLogger(type).IsErrorEnabled;
        }

        public bool IsFatalEnabled(Type type)
        {
            return global::log4net.LogManager.GetLogger(type).IsFatalEnabled;
        }

        public bool IsInfoEnabled(Type type)
        {
            return global::log4net.LogManager.GetLogger(type).IsInfoEnabled;
        }

        public bool IsWarnEnabled(Type type)
        {
            return global::log4net.LogManager.GetLogger(type).IsWarnEnabled;
        }

        public void Write(IBaseComponent component, LogLevel level, string message, params object[] args)
        {
            this.Write(component.GetType(), level, message, args);
        }

        public void Write(Type type, LogLevel level, string message, params object[] args)
        {
            var logger = global::log4net.LogManager.GetLogger(type);
            switch (level)
            {
                case LogLevel.Debug:
                    if (logger.IsDebugEnabled)
                    {
                        logger.DebugFormat(message, args);
                    }
                    break;
                case LogLevel.Error:
                    if (logger.IsErrorEnabled)
                    {
                        logger.ErrorFormat(message, args);
                    }
                    break;
                case LogLevel.Fatal:
                    if (logger.IsFatalEnabled)
                    {
                        logger.FatalFormat(message, args);
                    }
                    break;
                case LogLevel.Info:
                    if (logger.IsInfoEnabled)
                    {
                        logger.InfoFormat(message, args);
                    }
                    break;
                case LogLevel.Warn:
                    if (logger.IsWarnEnabled)
                    {
                        logger.WarnFormat(message, args);
                    }
                    break;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                LoggingBehaviourConfiguration.SECTION,
                Log4NetLoggerConfiguration.DEFAULT_APPENDER_ELEMENT
            ).ConnectValue(value =>
            {
                if (value)
                {
                    EnableFileAppender();
                }
                else
                {
                    DisableFileAppender();
                }
            });
            base.InitializeComponent(core);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return Log4NetLoggerConfiguration.GetConfigurationSections();
        }

        public static void EnableFileAppender()
        {
            BasicConfigurator.Configure(GetFileAppender());
        }

        public static void DisableFileAppender()
        {
            BasicConfigurator.Configure();
        }

        private static IAppender GetFileAppender()
        {
            var layout = new PatternLayout();
            layout.ConversionPattern = "%d [%2%t] %-5p [%-10c] %m%n";
            layout.ActivateOptions();
            var appender = new FileAppender()
            {
                AppendToFile = false,
                LockingModel = new FileAppender.MinimalLock(),
                Layout = layout,
                File = Path.Combine(ComponentScanner.Instance.Location, LoggingBehaviour.FILE_NAME)
            };
            appender.ActivateOptions();
            return appender;
        }
    }
}
