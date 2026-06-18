using Application.Core.Game.Life;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// NPC 脚本服务
    /// </summary>
    public interface IScriptNpcService : IPluginServiceBase
    {
        Task<bool> Start(IChannelClient c, int npcId, NPC? npcObjectId, string? scriptName);
        Task Action(IChannelClient c, sbyte mode, sbyte type, int selection, string? inputText = null);
    }
}
