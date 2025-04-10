namespace Application.Scripting.Lua
{
    public class NLuaScriptEngine : IEngine
    {
        readonly NLua.Lua _engine;

        public NLuaScriptEngine()
        {
            _engine = new NLua.Lua();
            _engine.LoadCLRPackage();
        }

        public void AddHostedObject(string name, object obj)
        {
            _engine[name] = obj;
        }

        public void AddHostedType(string name, Type type)
        {
            _engine.DoString($"""
                import ('{type.Assembly.FullName}', '{type.Namespace}') 
                """);
        }

        public ScriptResultWrapper CallFunction(string functionName, params object?[] paramsValue)
        {
            var function = _engine.GetFunction(functionName);
            if (function == null)
                throw new Exception($"方法{functionName}不存在");
            return new NLuaResultWrapper(function.Call(paramsValue));
        }

        public void Dispose()
        {
            _engine.Dispose();
        }

        public ScriptResultWrapper Evaluate(string code)
        {
            return new NLuaResultWrapper(_engine.DoString(code));
        }

        public ScriptResultWrapper Evaluate(ScriptPrepareWrapper prepared)
        {
            if (prepared is NLuaScriptPrepareWrapper d)
                return Evaluate(d.Content);
            return new NLuaResultWrapper([]);
        }

        public ScriptResultWrapper EvaluateFile(string filePath)
        {
            return new NLuaResultWrapper(_engine.DoFile(filePath));
        }

        public ScriptResultWrapper GetValue(string variable)
        {
            return new NLuaResultWrapper(_engine.DoString(variable));
        }

        public bool IsExisted(string variable)
        {
            return _engine.GetObjectFromPath(variable) != null;
        }

        public ScriptPrepareWrapper Prepare(string code)
        {
            return new NLuaScriptPrepareWrapper(code);
        }
    }
}
