using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Shared.Constants;

namespace Application.Plugin.Script.Features.MushroomCastle
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 1300014 
        public async Task forself()
        {
            var MapId = getMapId();

            if (MapId == 106020300)
            {
                //蘑菇森林深处
                if (isQuestActive(2314) && getQuestProgressInt(2314) == 0)
                {
                    // i sense an overwhelming force of magic here
                    await ShowEffect("Effect/OnUserEff/normalEffect/mushroomcastle/chatBalloon1");
                    await SayNext("这里...似乎有点奇怪...？！", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd);
                    await SayNext("嗯...似乎有一种无形的力量在阻止我通过入口。", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd);
                    await SayNext("显然这不是普通的障碍，否则我不可能过不去，也许...这应该是 #e#b#p1300003##k#n 提到的结界了。", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd);
                    await SayNext("看来还是得先回去跟 #e#b#p1300003##k#n 报告一下了。", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd);
                    await setQuestProgress(2314, 1);
                }
                else if (haveItem(2430014))
                {
                    await SayNext("在这附近使用#e#b#v2430014##t2430014##n#k应该就能消除魔法结界了吧。", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd);
                }
            }
            else if (MapId == 106020500)
            {
                //城墙中央
                if (isQuestActive(2322) && getQuestProgressInt(2322) == 0)
                {
                    await SayOK("嗯...城墙爬满了#r长着尖刺得藤蔓#k，确实有点棘手，看来是过不去了。\r\n\r\n还是先回去跟 #e#b#p1300003##k#n 报告一下吧。", 3);
                    await setQuestProgress(2322, 1);
                }
            }
        }

        // Npc: 1300012 
        public async Task TD_MC_bossEnter()
        {
            // TD_MC_enterboss1
            if (IsQuestNotStarted(2330))
            {
                await SayOK("……");
                return;
            }

            Dictionary<int, string> allOptions = new() {
                    { 0, "挑战 企鹅王 和 雪人。" },
                };

            if (haveItem(4032388) && !isQuestCompleted(2332))
            {
                allOptions[1] = "拯救 #p1300002#。";
            }
            else if (isQuestCompleted(2332))
            {
                allOptions[2] = "挑战 #b蘑菇大臣#k。";
            }

            var option = await AskMenu("你要去哪？", allOptions);
            if (option == 0)
            {
                var pepe = GetEventManager(MushroomCastleFeature.PepeKingEventName);
                var r = await pepe.StartInstance(getPlayer());
                await SayOK(pepe.HandleCreateInstanceResult(r, c));
            }
            else
            {
                await getPlayer().PortalSound();
                await warp(106021401, "out00");
            }
        }



        // Npc: 1300013 
        public async Task TD_MC_violetaEnter()
        {
            if (IsQuestNotStarted(2332))
            {
                // if (!haveItem(4032388))
                await SayOK("门似乎已经被锁住了，需要找到开启门的钥匙……");
                return;
            }

            var em = GetEventManager(MushroomCastleFeature.EventName);
            await TryStartEvent(em);
        }
    }
}
