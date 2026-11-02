using Application.Core.Client;
using Application.Core.Game.Maps;
using scripting.map;

namespace Application.Plugin.Script.Features.DavyJohn
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class DavyJohnMapHandler : MapScriptMethods
    {
        public DavyJohnMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 925100000, 925100100, 925100200, 925100300, 925100400, 925100500
        public async Task StageMsg_davy()
        {
            // TODO

        }

    }
}
