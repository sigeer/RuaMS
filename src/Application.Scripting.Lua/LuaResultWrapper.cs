using MoonSharp.Interpreter;

namespace Application.Scripting.Lua
{
    public class LuaResultWrapper : ScriptResultWrapper
    {
        readonly DynValue? _value;

        public LuaResultWrapper(DynValue? value)
        {
            _value = value;
        }

        public override TObject ToObject<TObject>()
        {
            if (_value == null)
                return default;

            return _value.ToObject<TObject>();
        }

        public override object? ToObject()
        {
            return _value?.ToObject();
        }
    }
}
