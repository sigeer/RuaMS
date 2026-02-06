namespace Application.Scripting
{
    /// <summary>
    /// 被脚本调用的方法，标记一下防止看到没有引用而被移除
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ScriptCallAttribute : Attribute
    {
    }
}
