using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Shared.Constants;
using Application.Plugin.Script;
using Application.Resources.Messages;

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
                    await SayNext(this.GetTalkMessage(ScriptMessage.MushNpc_PlaceSeems, NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd));
                    await SayNext(this.GetTalkMessage(ScriptMessage.MushNpc_HmmSeems, NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd));
                    await SayNext(this.GetTalkMessage(ScriptMessage.MushNpc_ClearlyIsnt, NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd));
                    await SayNext(this.GetTalkMessage(ScriptMessage.MushNpc_SupposeGo, NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd));
                    await setQuestProgress(2314, 1);
                }
                else if (haveItem(2430014))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.MushNpc_UsingAround, NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd));
                }
            }
            else if (MapId == 106020500)
            {
                //城墙中央
                if (isQuestActive(2322) && getQuestProgressInt(2322) == 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.MushNpc_HmmCastle, 3));
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
                    { 0, this.GetTalkMessage(ScriptMessage.MushNpc_ChallengeKing) },
                };

            if (haveItem(4032388) && !isQuestCompleted(2332))
            {
                allOptions[1] = this.GetTalkMessage(ScriptMessage.MushNpc_Save);
            }
            else if (isQuestCompleted(2332))
            {
                allOptions[2] = this.GetTalkMessage(ScriptMessage.MushNpc_ChallengePrime);
            }

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.MushNpc_Going, allOptions));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.MushNpc_DoorSeems));
                return;
            }

            var em = GetEventManager(MushroomCastleFeature.EventName);
            await TryStartEvent(em);
        }
    }
}
