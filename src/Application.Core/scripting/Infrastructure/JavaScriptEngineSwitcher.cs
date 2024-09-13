using Application.Core.Scripting.Infrastructure;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;

namespace Application.Core.scripting.Infrastructure
{
    public class JavaScriptEngineSwitcherEngine : IEngine
    {
        readonly IJsEngine _engine;

        public JavaScriptEngineSwitcherEngine()
        {
            _engine = new JintJsEngine();
        }

        public void AddHostedObject(string name, object obj)
        {
            _engine.EmbedHostObject(name, obj);
        }

        public void AddHostedType(string name, Type type)
        {
            _engine.EmbedHostType(name, type);
        }

        public object CallFunction(string functionName, params object[] paramsValue)
        {
            return _engine.CallFunction(functionName, paramsValue);
        }

        public object Evaluate(string code)
        {
            return _engine.Evaluate(code);
        }
    }
}
