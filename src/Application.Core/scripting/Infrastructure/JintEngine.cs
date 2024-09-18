using Application.Core.Scripting.Infrastructure;
using Jint;
using Jint.Runtime;
using Jint.Runtime.Interop;

namespace Application.Core.scripting.Infrastructure
{
    public class JintEngine : IEngine
    {
        readonly Engine _engine;
        public JintEngine()
        {
            _engine = new Engine(o =>
            {
                o.AllowClr().AddExtensionMethods(typeof(ListExtensions), typeof(Enumerable));
                o.Strict = false;
                o.Interop.Enabled = true;
            });
        }
        public void AddHostedObject(string name, object obj)
        {
            _engine.SetValue(name, obj);
        }

        public void AddHostedType(string name, Type type)
        {
            _engine.SetValue(name, type);
        }

        public object CallFunction(string functionName, params object?[] paramsValue)
        {
            var m = _engine.Invoke(functionName, paramsValue);
            if (m is ObjectWrapper wrapper)
            {
                return wrapper.ToObject();
            }
            return m;
        }

        public void Dispose()
        {
            _engine.Dispose();
        }

        public object Evaluate(string code)
        {
            return _engine.Evaluate(code);
        }
    }
}
