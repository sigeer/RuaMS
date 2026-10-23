using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Infrastructure;
using Application.Core.scripting.npc;
using Application.Core.Server.life;
using Application.Shared.Constants;
using System.Drawing;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21733NpcHandler : NpcScriptBase
    {
        public Q21733NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 1104000, 1204006 
        public async Task DollMaster()
        {
            if (getMapId() == 910400000)
            {
                await SaySpeech([
                    new SpeechText("哼……真的找来了，太容易了？看来你的变身术还是有点用处的嘛。巴洛克，我们走。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd),
                    new SpeechText("切……这笔帐将来再算。", NpcTalkSpeaker.ExtraNpc | NpcTalkSpeaker.NoEnd, 1204004),
                    new SpeechText("来得正好。上一次因为刚和冒险骑士团的骑士们战斗完，已经没什么余力了，才会让你得逞。这次我可没那么好惹了！碍眼的家伙，消失掉吧！",NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd)
                    ]);
                await setQuestProgress(Q21733.QuestIdValue, 21762, 1);

                var mapObj = getMap();
                if (mapObj.countMonster(9300382) > 0)
                {
                    await mapObj.killMonster(9300382);
                }

                var npcPos = mapObj.getMapObject(getNpcObjectId())!.getPosition();
                await mapObj.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(9300345), new Point(npcPos.X + 50, npcPos.Y), mob =>
                {
                    mob.setSpawnEffect(12);
                });
                await mapObj.destroyNPC(getNpc());
            }
            else
            {
                // 910510001, 1104000
                await SayNext("什么……你不属于这里！");
                var puppet = GetEventManager(PuppeteerFeature.EventName);
                var r = await puppet.StartInstance(getPlayer());
                await SayOK(puppet.HandleCreateInstanceResult(r, c));

            }
        }

        // Npc: 1204005 
        public async Task downTrue()
        {
            if (getQuestProgressInt(Q21733.QuestIdValue, 9300345) < 1)
            {
                await SayOK(GetDefaultTalk(Q21733.QuestIdValue));
                return;
            }

            await SayNext("啊……那些家伙全都消灭了？哈哈……真不愧是英雄大人！呃，先整理整理再说。");
            await setQuestProgress(Q21733.QuestIdValue, 21762, 2);
            await warp(104000004);
        }
    }
}
