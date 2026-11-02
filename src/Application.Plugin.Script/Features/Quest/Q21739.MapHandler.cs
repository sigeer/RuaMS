using Application.Core.Client;
using Application.Core.Game.Maps;
using System.Drawing;
using scripting.map;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class Q21739MapHandler : MapScriptMethods
    {
        public Q21739MapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 920030001
        public async Task sealGarden()
        {
            var mapObj = getMap();
            if (getQuestProgressInt(Q21739.QuestIdValue, 9300348) > 0 || mapObj.countMonster(9300348) > 0 || mapObj.containsNPC(1204010))
            {
                return;
            }

            await spawnNpc(1204010, new Point(740, 0), mapObj);

        }

    }
}
