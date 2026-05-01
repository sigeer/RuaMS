using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Scripting.Events;
using server.maps;

namespace Application.Core.Gameplay.Plugins
{
    public interface IScriptService: IAsyncDisposable
    {
        #region Portal
        bool Enter(IChannelClient c, Portal p);
        #endregion


        #region NPC
        Task<bool> Start(IChannelClient c, int npcId, NPC? npcObjectId, string scriptName);
        Task Action(IChannelClient c, sbyte mode, sbyte type, int selection, string? inputText = null);
        #endregion

        Task<bool> StartQuest(IChannelClient c, server.quest.Quest questObj, int npcId);
        Task<bool> CompleteQuest(IChannelClient c, server.quest.Quest questObj, int npcId);

        Task ItemScript(IChannelClient c, int npcId, string scriptName);

        void MapFirstEnter(IChannelClient c, IMap map);
        void MapEnter(IChannelClient c, IMap map);

        Task ReactorHit(IChannelClient c, Reactor r);
        Task ReactorAct(IChannelClient c, Reactor r);

        int RegisterEvents(WorldChannel channel);
    }
}
