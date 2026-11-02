using System.Reflection;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 一条脚本注册：某个脚本名由哪个类型的方法实现，以及它由哪个插件提供。
    ///
    /// 插件通过 <c>IScript*Service.Load*Scripts()</c> 返回自己声明的脚本时，
    /// <see cref="Source"/> 为 null；<see cref="PluginManager"/> 合并时用 <see cref="WithSource"/>
    /// 补上来源插件，因此生效表中的每条描述符都能直接回答「这是谁的脚本」。
    /// </summary>
    public sealed class ScriptHandlerDescriptor
    {
        public ScriptHandlerDescriptor(ScriptKind kind, string scriptName, Type handlerType, MethodInfo method, PluginServiceBase source)
        {
            Kind = kind;
            ScriptName = scriptName;
            HandlerType = handlerType;
            Method = method;
            Source = source;
        }

        public ScriptKind Kind { get; }
        public string ScriptName { get; }
        public Type HandlerType { get; }
        public MethodInfo Method { get; }

        public PluginServiceBase Source { get; }

        public string FullName => $"{HandlerType.FullName}.{Method.Name}";


        public override string ToString()
        {
            return $"{Kind}:{ScriptName} -> {FullName}";
        }
    }
}
