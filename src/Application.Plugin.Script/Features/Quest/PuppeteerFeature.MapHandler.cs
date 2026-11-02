using Application.Core.Client;
using Application.Core.Game.Maps;
using scripting.map;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class PuppeteerFeatureMapHandler : MapScriptMethods
    {
        public PuppeteerFeatureMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 910510200
        public async Task dollCave00()
        {
            // TODO

        }

        // Map: 910510201
        public async Task dollCave01()
        {
            // TODO

        }

        // Map: 910510202
        public async Task dollCave02()
        {
            if (Map.getMonsterById(9300346) == null)
            {
                await Map.spawnMonsterOnGroundBelow(9300346, 95, 200);
            }
        }

    }
}
