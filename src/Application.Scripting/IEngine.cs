namespace Application.Scripting
{
    public interface IEngine : IDisposable
    {
        public void AddHostedObject(string name, object obj);
        public void AddHostedType(string name, Type type);
        public ScriptResultWrapper CallFunction(string functionName, params object?[] paramsValue);
        public object Evaluate(string code);
        public object? GetValue(string variable);
        public bool IsExisted(string variable);
    }
}
