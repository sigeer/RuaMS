using System.Collections.Concurrent;

namespace Application.Scripting.Lua
{
    internal class CLRTypeManager
    {
        public static ConcurrentDictionary<string, Type> TypeSource { get; set; } = new ();
    }
}
