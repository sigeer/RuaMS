using Application.Core.Client;
using Application.Core.Game.Maps;
using scripting.map;

namespace Application.Plugin.Script.Features.RescueGaga
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class RescueGagaMapHandler : MapScriptMethods
    {
        public RescueGagaMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 922240200
        public async Task space_first()
        {
            // TODO

        }

        // Map: 922240000, 922240001, 922240002, 922240003, 922240004, 922240005 … (共 20 张)
        public async Task spaceGaGa_start()
        {
            // TODO

        }

        // Map: 922240100, 922240101, 922240102, 922240103, 922240104, 922240105 … (共 20 张)
        public async Task spaceGaGa_sMap()
        {
            await spawnMonster(9300331, -28, 0);
        }

    }
}
