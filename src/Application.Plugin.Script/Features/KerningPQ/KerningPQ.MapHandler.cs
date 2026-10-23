using Application.Core.Client;
using Application.Core.Game.Maps;
using scripting.map;

namespace Application.Plugin.Script.Features.KerningPQ
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class KerningPQMapHandler : MapScriptMethods
    {
        public KerningPQMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 103000800, 103000801, 103000802, 103000803, 103000804
        public async Task StageMsg_together()
        {
            // TODO

        }

    }
}
