using Application.Core.Scripting.Infrastructure;
using JavaScriptEngineSwitcher.Core;
using Jint;
using Jint.Native;
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
                // o.AddObjectConverter(new UndefinedConverter());
                o.AllowClr().AddExtensionMethods(typeof(ListExtensions), typeof(Enumerable));

                // o.SetReferencesResolver(new NullPropagationReferenceResolver());
                o.Strict = false;

                //o.Interop.AllowGetType = true;
                //o.Interop.AllowSystemReflection = true;
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

        public object CallFunction(string functionName, params object[] paramsValue)
        {
            var m = _engine.Invoke(functionName, paramsValue);
            if (m is ObjectWrapper wrapper)
            {
                return wrapper.ToObject();
            }
            return m;
        }

        public object Evaluate(string code)
        {
            return _engine.Evaluate(code);
        }
    }

    public sealed class UndefinedConverter : IObjectConverter
    {
        public bool TryConvert(Engine engine, object value, out JsValue result)
        {
            if (value is Undefined)
            {
                result = JsValue.Undefined;
                return true;
            }

            result = JsValue.Null;
            return false;
        }
    }

    public class NullPropagationReferenceResolver : IReferenceResolver
    {
        public bool TryUnresolvableReference(Engine engine, Reference reference, out JsValue value)
        {
            value = reference.Base;
            return true;
        }

        public bool TryPropertyReference(Engine engine, Reference reference, ref JsValue value)
        {
            return value.IsNull() || value.IsUndefined();
        }

        public bool TryGetCallable(Engine engine, object callee, out JsValue value)
        {
            if (callee is Reference reference)
            {
                var name = reference.ReferencedName.AsString();
                if (name == "filter")
                {
                    value = new ClrFunction(engine, "map", (thisObj, values) => engine.Intrinsics.Array.AsArray());
                    return true;
                }
            }

            value = new ClrFunction(engine, "anonymous", (thisObj, values) => thisObj);
            return true;
        }

        public bool CheckCoercible(JsValue value)
        {
            return true;
        }
    }
}
