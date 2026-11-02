using Application.Core.Client;
using Application.Core.Game.Maps;
using Application.Core.Server.life;
using System.Drawing;
using scripting.map;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class Q21733MapHandler : MapScriptMethods
    {
        public Q21733MapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 910400000
        public async Task dangerInfo()
        {
            var questMap = 910400000;
            var normalMap = 104000004;

            if (!isQuestStarted(Q21733.QuestIdValue))
            {
                await warp(normalMap);
            }

            var questProgress = getQuestProgressInt(Q21733.QuestIdValue, 21762);
            // 已完成（未提交）
            if (questProgress == 2)
            {
                await warp(normalMap);
            }

            var mapObj = await getWarpMap(questMap);
            // mob已击杀，但是未对话2
            if (getQuestProgressInt(Q21733.QuestIdValue, 9300345) > 0)
            {
                return;
            }

            // DollMaster
            await spawnNpc(1204006, new Point(75, 0), mapObj);

            var monsterObj = LifeFactory.Instance.GetMonsterTrust(9300382);
            await mapObj.spawnFakeMonsterOnGroundBelow(monsterObj, new Point(225, 0), mob =>
            {
                mob.setStance(4);
            });

            await Pink("情报商店被人偶师占领了！确认情况后，击退人偶师！");

        }

    }
}
