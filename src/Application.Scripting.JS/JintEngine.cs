using Application.Utility.Extensions;
using Jint;
using Jint.Runtime.Interop;
using System.Diagnostics.CodeAnalysis;

namespace Application.Scripting.JS
{
    public class JintEngine : IEngine
    {
        readonly Engine _engine;
        public JintEngine()
        {
            _engine = new Engine(o =>
            {
                o.AllowClr().AddExtensionMethods(typeof(JsEngineExtensions), typeof(PointExtensions));
                o.SetTypeConverter(o =>
                {
                    return new CustomeTypeConverter(o);
                });
                o.Strict = false;
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

        /// <summary>
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="paramsValue"></param>
        /// <returns>
        /// 不会走 <see cref="CustomeTypeConverter"/>
        /// Array -> object[]
        /// Date -> DateTime
        /// number -> double
        /// string -> string
        /// boolean -> bool
        /// Regex -> RegExp
        /// Function -> Delegate
        /// </returns>
        public ScriptResultWrapper CallFunction(string functionName, params object?[] paramsValue)
        {
            var m = _engine.Invoke(functionName, paramsValue);
            return new JintResultWrapper(m);
        }

        public void Dispose()
        {
            _engine.Dispose();
        }

        public ScriptResultWrapper EvaluateFile(string filePath)
        {
            return Evaluate(File.ReadAllText(filePath));
        }

        public ScriptResultWrapper Evaluate(string code)
        {
            return new JintResultWrapper(_engine.Evaluate(code));
        }

        public ScriptResultWrapper Evaluate(ScriptPrepareWrapper prepared)
        {
            return new JintResultWrapper(_engine.Evaluate(((JintScriptPrepareWrapper)prepared).Value));
        }

        public ScriptResultWrapper GetValue(string variable)
        {
            return new JintResultWrapper(_engine.GetValue(variable));
        }

        public bool IsExisted(string variable)
        {
            return !_engine.Evaluate(variable).IsUndefined();
        }

        public static ScriptPrepareWrapper Prepare(string code)
        {
            return new JintScriptPrepareWrapper(Engine.PrepareScript(code));
        }
    }

    public class CustomeTypeConverter : DefaultTypeConverter
    {
        public CustomeTypeConverter(Engine engine) : base(engine)
        {
        }
        public override bool TryConvert(object? value, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields)] Type type, IFormatProvider formatProvider, [NotNullWhen(true)] out object? converted)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) && value is object[] arr)
            {
                converted = arr.ToList();
                return true;
            }

            return base.TryConvert(value, type, formatProvider, out converted);
        }
    }
}
