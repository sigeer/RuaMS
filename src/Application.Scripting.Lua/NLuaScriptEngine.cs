using System.Text;

namespace Application.Scripting.Lua
{
    public class NLuaScriptEngine : IEngine
    {
        readonly NLua.Lua _engine;

        public NLuaScriptEngine()
        {
            _engine = new NLua.Lua();
            _engine.LoadCLRPackage();
            _engine.State.Encoding = Encoding.UTF8;
            AddHostedType("LuaTableUtils", typeof(LuaTableUtils));
        }

        public void AddHostedObject(string name, object obj)
        {
            _engine[name] = obj;
        }

        public void AddHostedType(string name, Type type)
        {
            var fullName = type.Assembly.FullName;
            if (fullName != null)
            {
                CLRTypeManager.TypeSource[fullName] = type;
                _engine.DoString($"""
                import ('{type.Assembly.FullName}', '{type.Namespace}') 
                """);
            }


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
            return new NLuaResultWrapper(_engine.DoString(Encoding.UTF8.GetBytes(code)));
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
            return new NLuaResultWrapper(new object[] { _engine.GetObjectFromPath(variable)  });
        }

        public bool IsExisted(string variable)
        {
            return _engine.GetObjectFromPath(variable) != null;
        }

        public static ScriptPrepareWrapper Prepare(string code)
        {
            return new NLuaScriptPrepareWrapper(code);
        }
    }
}
