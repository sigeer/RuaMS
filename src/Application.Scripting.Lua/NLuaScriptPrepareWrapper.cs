using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Scripting.Lua
{
    public class NLuaScriptPrepareWrapper : ScriptPrepareWrapper
    {
        public NLuaScriptPrepareWrapper(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }
}
