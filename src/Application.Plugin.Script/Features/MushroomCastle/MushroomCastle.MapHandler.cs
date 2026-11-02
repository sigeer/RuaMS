using Application.Core.Client;
using Application.Core.Game.Maps;
using scripting.map;

namespace Application.Plugin.Script.Features.MushroomCastle
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class MushroomCastleMapHandler : MapScriptMethods
    {
        public MushroomCastleMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 106021500, 106021501, 106021502, 106021503, 106021504, 106021505 … (共 10 张)
        public async Task pepeking_effect()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != MushroomCastleFeature.PepeKingEventName)
            {
                return;
            }

            var bossId = eim.getIntProperty(MushroomCastleFeature.PepeKingBossIdProperty);
            var type = bossId == MushroomCastleFeature.PepeKingFirstSubBossId
                ? "B"
                : (bossId == MushroomCastleFeature.PepeKingFirstSubBossId + 1 ? "G" : "W");

            await mapEffect("pepeKing/frame/W");
            await mapEffect("pepeKing/pepe/pepe" + type);
            await mapEffect("pepeKing/frame/B");
            await mapEffect("pepeKing/chat/nugu");
        }

        // Map: 106021001, 106021002, 106021003, 106021004, 106021005, 106021006 … (共 10 张)
        public async Task in_secretroom()
        {
            // 在Portal触发了

        }

        // Map: 106020000
        public Task TD_MC_title()
        {
            return mapEffect("temaD/enter/mushCatle");
        }

        // Map: 106020001
        public async Task TD_MC_Openning()
        {
            await lockUI();
            await ShowDirectionEffect("Effect/Direction2.img/open/back0");
            await ShowDirectionEffect("Effect/Direction2.img/open/back1");
            await ShowDirectionEffect("Effect/Direction2.img/open/light");
            await ShowDirectionEffect("Effect/Direction2.img/open/pepeKing");
            await ShowDirectionEffect("Effect/Direction2.img/open/line");
            await ShowDirectionEffect("Effect/Direction2.img/open/violeta0");
            await ShowDirectionEffect("Effect/Direction2.img/open/violeta1");
            await ShowDirectionEffect("Effect/Direction2.img/open/frame");
            await ShowDirectionEffect("Effect/Direction2.img/open/chat");
            await ShowDirectionEffect("Effect/Direction2.img/open/out");
            // Effect/Direction2.img/open/out/0 = type=2, field=106020000
        }

        // Map: 106020502
        public async Task TD_MC_gasi()
        {
            await lockUI();
            await ShowDirectionEffect("Effect/Direction2.img/gasi/gasi1");
            await ShowDirectionEffect("Effect/Direction2.img/gasi/gasi2");
            await ShowDirectionEffect("Effect/Direction2.img/gasi/gasi22");
            await ShowDirectionEffect("Effect/Direction2.img/gasi/gasi3");
            await ShowDirectionEffect("Effect/Direction2.img/gasi/gasi4");
            await ShowDirectionEffect("Effect/Direction2.img/gasi/gasi5");
            await ShowDirectionEffect("Effect/Direction2.img/gasi/gasi6");
            await ShowDirectionEffect("Effect/Direction2.img/gasi/gasi7");
            await ShowDirectionEffect("Effect/Direction2.img/gasi/gasi8");
            // 最终会跳转 106020501
            // Effect/Direction2.img/gasi/gasi6/0 = type=2, field=106020501
        }

        // Map: 106020501
        public async Task TD_MC_gasi2()
        {
            // TODO

        }

        // Map: 106021400
        public async Task TD_MC_keycheck()
        {
            // TODO

        }

        // Map: 106021600, 106021601, 106021602, 106021603, 106021604, 106021605 … (共 10 张)
        public async Task findvioleta()
        {
            if (isQuestStarted(2332))
            {
                await getPlayer().ForceCompleteQuest(2332, 1300002);
                // QuestInfo中描述了奖励，但是相关的Act.img没有数据，不能使用completeQuest
                await giveCharacterExp(4400, getPlayer());
            }
        }

        // Map: 106021500, 106021501, 106021502, 106021503, 106021504, 106021505 … (共 10 张)
        public async Task summon_pepeking()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != MushroomCastleFeature.PepeKingEventName)
            {
                return;
            }

            var subBossId = MushroomCastleFeature.PepeKingFirstSubBossId + Random.Shared.Next(3);
            eim.setIntProperty(MushroomCastleFeature.PepeKingBossIdProperty, subBossId);

            await getMap().spawnMonsterOnGroundBelow(subBossId, -28, -68);
        }

    }
}
