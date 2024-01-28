using FoxTunes.Interfaces;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using V8.Net;

namespace FoxTunes
{
    public class JSScriptingContext : ScriptingContext
    {
        public JSScriptingContext(V8Engine engine)
        {
            this.Engine = engine;
            this.Context = this.Engine.CreateContext();
            this.Engine.SetContext(this.Context);
        }

        public V8Engine Engine { get; private set; }

        public Context Context { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Engine.Execute(Resources.utils, throwExceptionOnError: true);
            base.InitializeComponent(core);
        }

        public override void SetValue(string name, object value)
        {
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
            this.SetValue(name, value);
        }

        public override object GetValue(string name)
        {
            return this.Engine.GlobalObject.GetProperty(name);
        }

        [DebuggerNonUserCode]
        public override object Run(string script)
        {
            try
            {
                return this.Engine.Execute(script, throwExceptionOnError: true);
            }
            catch (V8ExecutionErrorException e)
            {
                throw;
            }
        }

        protected override void OnDisposing()
        {
            this.Context.Dispose();
            this.Engine.Dispose();
            base.OnDisposing();
        }
    }
}
