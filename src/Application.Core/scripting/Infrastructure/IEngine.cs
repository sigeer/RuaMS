namespace Application.Core.Scripting.Infrastructure
{
    public interface IEngine : IDisposable
    {
        public void AddHostedObject(string name, object obj);
        public void AddHostedType(string name, Type type);
        public object? CallFunction(string functionName, params object?[] paramsValue);
        public object Evaluate(string code);
        public object? GetValue(string variable);
    }
}
