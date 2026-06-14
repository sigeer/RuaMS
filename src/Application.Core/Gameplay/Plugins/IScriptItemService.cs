using Application.Core.Channel;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 物品脚本服务
    /// </summary>
    public interface IScriptItemService : IPluginServiceBase
    {
        Task ItemScript(IChannelClient c, int npcId, string scriptName);
    }
}
