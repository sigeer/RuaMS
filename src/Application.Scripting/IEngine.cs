namespace Application.Scripting
{
    public interface IEngine : IDisposable
    {
        public void AddHostedObject(string name, object obj);
        public void AddHostedType(string name, Type type);
        public ScriptResultWrapper CallFunction(string functionName, params object?[] paramsValue);
        public ScriptResultWrapper EvaluateFile(string filePath);
        public ScriptResultWrapper Evaluate(string code);
        public ScriptResultWrapper Evaluate(ScriptPrepareWrapper prepared);
        public ScriptResultWrapper GetValue(string variable);
        public bool IsExisted(string variable);
    }
}
