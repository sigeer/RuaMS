namespace Application.Core.Gameplay.Plugins.Services
{
    /// <summary>
    /// 事件模板服务。事件模板由 <see cref="PluginManager"/> 扫描插件程序集自动发现，
    /// 本接口仅用于声明该插件提供事件模板。
    ///
    /// 与脚本不同，事件模板不允许跨插件重名——发现重名直接抛异常，
    /// 因为事件名是客户端/脚本约定的全局键，静默覆盖会让另一个功能悄悄失效。
    /// </summary>
    [PluginInvocation(PluginInvocationMode.FirstMatch)]
    public interface IScriptEventService : IPluginServiceBase
    {
    }
}
