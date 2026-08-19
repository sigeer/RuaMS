namespace Application.Core.Gameplay.Plugins
{
    public interface IPluginServiceBase : IAsyncDisposable
    {
        /// <summary>
        /// 插件服务优先级，仅对 FirstMatch（首次命中）模式生效：同名脚本冲突时由优先级最高的插件处理。
        /// 默认脚本插件使用 <see cref="PluginPriority.Low"/>，第三方插件以默认 <see cref="PluginPriority.Normal"/> 即可覆盖。
        /// </summary>
        PluginPriority Priority { get; }

        Task OnMounted();
    }
}
