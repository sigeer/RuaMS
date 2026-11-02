using Application.Core.Client;
using Application.Core.Game.Maps;
using scripting.map;

namespace Application.Plugin.Script.Features.RockSpirit
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class RockSpiritMapHandler : MapScriptMethods
    {
        public RockSpiritMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 103040430, 103040431, 103040432, 103040433, 103040434, 103040435 … (共 10 张)
        public async Task Depart_BossEnter()
        {
            // TODO

        }

        // Map: 103040410, 103040411, 103040412, 103040413, 103040414, 103040415 … (共 10 张)
        public async Task Depart_topFloorEnter()
        {
            // TODO

        }

        // Map: 103040430, 103040431, 103040432, 103040433, 103040434, 103040435 … (共 10 张)
        public async Task Depart_Boss_F_Enter()
        {
            // TODO

        }

    }
}
