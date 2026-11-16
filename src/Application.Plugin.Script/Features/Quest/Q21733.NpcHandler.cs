using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Infrastructure;
using Application.Core.scripting.npc;
using Application.Core.Server.life;
using Application.Shared.Constants;
using System.Drawing;
using Application.Plugin.Script;
using Application.Resources.Messages;

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
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21733Npc_HmphReally), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21733Npc_TchIll), NpcTalkSpeaker.ExtraNpc | NpcTalkSpeaker.NoEnd, 1204004),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21733Npc_GoodTiming),NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd)
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
                await SayNext(this.GetTalkMessage(ScriptMessage.Q21733Npc_DontBelong));
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

            await SayNext(this.GetTalkMessage(ScriptMessage.Q21733Npc_AhAll));
            await setQuestProgress(Q21733.QuestIdValue, 21762, 2);
            await warp(104000004);
        }
    }
}
