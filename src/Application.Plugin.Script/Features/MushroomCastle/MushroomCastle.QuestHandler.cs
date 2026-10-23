using Application.Core.Client;
using Application.Core.scripting.quest;

namespace Application.Plugin.Script.Features.MushroomCastle
{
    internal sealed class QuestHandler : QuestScriptBase
    {
        public QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc) : base(c, quest, npc)
        {
        }

        // Quest: 2333 
        public async Task q2333s()
        {
            var eim = GetEventInstanceTrust();

            if (await SayAcceptDecline("勇士，拜托你了！请你一定要拯救蘑菇王国"))
            {
                await SayNext("#b蘑菇大臣#k是幕后策划的黑手！哦，不！他来了。。。");
                await forceStartQuest();


                var template = eim.EventManager.GetTemplate as Event;
                await template.SpawnBoss(eim);
            }
        }
        // Quest: 2333 
        public async Task q2333e()
        {
            await SayNext("天哪！ #b#h ##k 你居然打败了 #b蘑菇大臣#k.");

            await gainExp(15000);
            await forceCompleteQuest();
        }

        // Quest: 2335 
        public async Task q2335s()
        {
            await SayNext("这还不是结束，#b#h ##k。 #b蘑菇大臣#k 的手下仍然散布在城堡中。");
            if (await SayAcceptDecline("据我所知，靠近#b天空高楼 3#k附近有一群蘑菇大臣的手下。前几天我捡到了蘑菇大臣掉落的一把钥匙。拿去吧，用这把钥匙。"))
            {
                if (canHold(4032405))
                {
                    await gainItem(4032405, 1);
                    await forceStartQuest();

                    await SayNext("最后一次，祝你好运。");
                }
                else
                {
                    await SayOK("请确保你的其他栏有空位。");
                }
            }
        }

    }
}
