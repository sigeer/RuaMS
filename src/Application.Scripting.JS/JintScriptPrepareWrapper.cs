using Acornima.Ast;
using Jint;

namespace Application.Scripting.JS
{
    public class JintScriptPrepareWrapper : ScriptPrepareWrapper
    {
        public JintScriptPrepareWrapper(Prepared<Script> value)
        {
            Value = value;
        }

        public Prepared<Script> Value { get; set; }
    }
}
