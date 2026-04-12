using Application.Core.Game.Maps;
using server.maps;

namespace Application.Core.Gameplay.Plugins
{
    public interface IScriptService
    {
        #region Portal
        Task<bool> Enter(IChannelClient c, Portal p);
        #endregion


        #region NPC
        Task Start(IChannelClient c, int npcId, int npcObjectId, string scriptName);
        Task Action(IChannelClient c, sbyte mode, sbyte type, int selection, string? inputText = null);
        #endregion

        Task ItemScript(IChannelClient c, int npcId, string scriptName);

        Task MapFirstEnter(IChannelClient c, IMap map);
        Task MapEnter(IChannelClient c, IMap map);

        Task ReactorHit(IChannelClient c, Reactor r);
        Task ReactorAct(IChannelClient c, Reactor r);

    }
}
