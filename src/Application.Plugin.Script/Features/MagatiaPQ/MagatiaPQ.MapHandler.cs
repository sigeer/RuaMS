using Application.Core.Client;
using Application.Core.Game.Maps;
using scripting.map;

namespace Application.Plugin.Script.Features.MagatiaPQ
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class MagatiaPQMapHandler : MapScriptMethods
    {
        public MagatiaPQMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 926100000, 926100001, 926100100, 926100200, 926100300, 926100301 … (共 9 张)
        public async Task StageMsg_romio()
        {
            // TODO

        }

        // Map: 926110000, 926110001, 926110100, 926110200, 926110300, 926110301 … (共 9 张)
        public async Task StageMsg_juliet()
        {
            // TODO

        }

    }
}
