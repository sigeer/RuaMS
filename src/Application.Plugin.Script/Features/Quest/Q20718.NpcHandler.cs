using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.Server.life;
using System.Drawing;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q20718NpcHandler : Application.Core.scripting.npc.NpcScriptBase
    {
        public Q20718NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 1032109 
        public async Task blackShadowEli1()
        {
            if (!isQuestStarted(Q20718.QuestIdValue))
            {
                // thanks Stray, Ari
                return;
            }

            await SayOK("一个神秘的黑色身影出现并召唤了许多愤怒的怪物！");
            var player = getPlayer();
            var map = player.getMap();
            var mobId = 2220100;
            for (var i = 0; i < 10; i++)
            {
                await map.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(mobId), new Point(117, 183));
            }
            for (var i = 0; i < 10; i++)
            {
                await map.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(mobId), new Point(4, 183));
            }
            for (var i = 0; i < 10; i++)
            {
                await map.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(mobId), new Point(-109, 183));
            }

            await completeQuest(Q20718.QuestIdValue, 1103003);
            await gainExp((int)(4000 * getPlayer().getExpRate()));

        }

        // Npc: 1032110 
        public async Task blackShadowEli2()
        {
            await SayOK(GetDefaultTalk(Q20718.QuestIdValue));
        }
    }
}
