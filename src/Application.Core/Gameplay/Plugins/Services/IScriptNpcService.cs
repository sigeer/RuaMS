namespace Application.Core.Gameplay.Plugins.Services
{
    /// <summary>
    /// NPC 脚本服务。插件在此返回自己声明了的脚本，同名脚本由 <see cref="PluginManager"/> 按优先级裁决。
    /// </summary>
    [PluginInvocation(PluginInvocationMode.FirstMatch)]
    public interface IScriptNpcService : IPluginServiceBase
    {
    }
}
