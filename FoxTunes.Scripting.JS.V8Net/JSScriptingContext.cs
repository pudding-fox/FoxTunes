using FoxTunes.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using V8.Net;

namespace FoxTunes
{
    public class JSScriptingContext : ScriptingContext
    {
        private JSScriptingContext()
        {
            this.Scripts = new Dictionary<string, InternalHandle>();
        }

        public JSScriptingContext(V8Engine engine) : this()
        {
            this.Engine = engine;
            this.Context = this.Engine.CreateContext();
            this.Engine.SetContext(this.Context);
        }

        public IDictionary<string, InternalHandle> Scripts { get; private set; }

        public V8Engine Engine { get; private set; }

        public Context Context { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Engine.Execute(JSCoreScripts.Instance.Utils, throwExceptionOnError: true);
            this.Engine.RegisterType(typeof(Publication), memberSecurity: ScriptMemberSecurity.Locked);
            this.Engine.GlobalObject.SetProperty(typeof(Publication));
            this.Engine.RegisterType<DateHelper>(memberSecurity: ScriptMemberSecurity.Locked);
            this.Engine.GlobalObject.SetProperty(typeof(DateHelper));
            this.Engine.RegisterType<NumberHelper>(memberSecurity: ScriptMemberSecurity.Locked);
            this.Engine.GlobalObject.SetProperty(typeof(NumberHelper));
            //Note: Lower case to match tag, property etc.
            this.SetValue(this.Engine.GlobalObject, "strings", StringsHelper.Strings);
            base.InitializeComponent(core);
        }

        public override void SetValue(string name, object value)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            this.SetValue(this.Engine.GlobalObject, name, value);
        }

        protected virtual void SetValue(InternalHandle target, string name, object value)
        {
            if (value is IDictionary dictionary)
            {
                this.SetValue(target, name, dictionary);
            }
            else
            {
                target.SetProperty(name, value);
            }
        }

        protected virtual void SetValue(InternalHandle target, string name, IDictionary dictionary)
        {
            var value = this.Engine.CreateObject();
            foreach (var key in dictionary.Keys.OfType<string>())
            {
                this.SetValue(value, key, dictionary[key]);
            }
            target.SetProperty(name, value);
        }

        public override object GetValue(string name)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            return this.GetValue(this.Engine.GlobalObject.GetProperty(name));
        }

        protected virtual object GetValue(InternalHandle target)
        {
            if (target.IsUndefined)
            {
                return null;
            }
            return target.LastValue;
        }

        [DebuggerNonUserCode]
        public override object Run(string script)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            try
            {
                var handle = this.Compile(script);
                using (var result = this.Engine.Execute(handle, throwExceptionOnError: true))
                {
                    return this.GetValue(result);
                }
            }
            catch (V8ExecutionErrorException e)
            {
                throw new ScriptingException(e.Message);
            }
        }

        protected virtual InternalHandle Compile(string script)
        {
            return this.Scripts.GetOrAdd(script, key => this.Engine.Compile(script, throwExceptionOnError: true));
        }

        protected override void OnDisposing()
        {
            if (this.Scripts != null)
            {
                foreach (var pair in this.Scripts)
                {
                    pair.Value.Dispose();
                }
            }
            if (this.Context != null)
            {
                this.Context.Dispose();
            }
            if (this.Engine != null)
            {
                this.Engine.Dispose();
            }
            base.OnDisposing();
        }
    }
}