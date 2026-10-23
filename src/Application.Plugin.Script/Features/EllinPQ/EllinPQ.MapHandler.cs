using Application.Core.Client;
using Application.Core.Game.Maps;
using scripting.map;

namespace Application.Plugin.Script.Features.EllinPQ
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class EllinPQMapHandler : MapScriptMethods
    {
        public EllinPQMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 930000000, 930000010, 930000100, 930000200, 930000300, 930000400 … (共 8 张)
        public async Task party6weatherMsg()
        {
            // TODO

        }

    }
}
