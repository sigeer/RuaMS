using MoonSharp.Interpreter;
using System.Drawing;
using System.Xml.Linq;

namespace Application.Scripting.Lua
{
    public class LuaScriptEngine : IEngine
    {
        readonly Script _script;

        public LuaScriptEngine()
        {
            _script = new Script();

            // 设置元表，捕获未定义的全局变量
            var metatable = new Table(_script);

            metatable.Set("__index", DynValue.NewCallback((context, args) =>
            {
                string typeName = args[1].String;

                var type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.Name == typeName);
                if (type != null)
                {
                    UserData.RegisterType(type); // 自动注册类型
                    if (type.IsAbstract && type.IsSealed)
                    {
                        return UserData.CreateStatic(type);
                    }
                    return UserData.Create(type);
                }

                throw new ScriptRuntimeException($"Type '{typeName}' not found.");
            }));

            // 将元表设置到全局环境
            _script.Globals.MetaTable = metatable;
        }

        public void AddHostedObject(string name, object obj)
        {
            UserData.RegisterType(obj.GetType());
            _script.Globals[name] = obj;
        }

        public void AddHostedType(string name, Type type)
        {
            UserData.RegisterType(type);
            if (type.IsAbstract && type.IsSealed)
                _script.Globals[name] = UserData.CreateStatic(type);
            if (type.IsEnum)
                _script.Globals[name] = UserData.CreateStatic(type);
        }

        public ScriptResultWrapper CallFunction(string functionName, params object?[] paramsValue)
        {
            foreach (var item in paramsValue)
            {
                if (item != null)
                {
                    UserData.RegisterType(item.GetType());
                }
            }
            var value = _script.Call(_script.Globals[functionName], paramsValue);
            return new LuaResultWrapper(value);
        }

        public object Evaluate(string code)
        {
            return _script.DoString(code).ToObject();
        }

        public object? GetValue(string variable)
        {
            return _script.DoString(variable).ToObject();
        }

        public bool IsExisted(string variable)
        {
            var value = _script.Globals.RawGet(variable);
            return value != null && value.IsNotNil();
        }

        public void Dispose()
        {
        }

        public void RegisterPoint()
        {
            UserData.RegisterType(typeof(Point));
            _script.Globals["Point"] = (Func<int, int, Point>)((x, y) =>
            { return new Point(x, y); });
        }
    }
}
