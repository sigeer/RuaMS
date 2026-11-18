using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Infrastructure;
using Application.Core.scripting.npc;
using Application.Shared.Constants;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Aran
{
    internal sealed class AranNpcHandler : NpcScriptBase
    {
        public AranNpcHandler(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        // Npc: 1209000 
        public async Task talkHelena()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.Aran_Npc_AranYoure),
                this.GetTalkMessage(ScriptMessage.Aran_Npc_WereAlmost),
                this.GetTalkMessage(ScriptMessage.Aran_Npc_OtherHeroes)
                ]);
            await setQuestProgress(21000, 21002, 1);
            await ShowDirectionEffect("Effect/Direction1.img/aranTutorial/Trio");
        }

        // Npc: 1202000 
        public async Task awake()
        {
            if (getPlayer().getMapId() == 140090000)
            {
                if (!containsAreaInfo(21019, "helper=clear"))
                {
                    await SaySpeech([
                        new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_YouveFinally), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_AndYouAre), NpcTalkSpeaker.PlayerRight),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_HeroFought), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_TalkingAbout), NpcTalkSpeaker.PlayerRight),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_CantRemember), NpcTalkSpeaker.PlayerRight)
                        ]);
                    await ShowDirectionEffect("Effect/Direction1.img/aranTutorial/face");
                    await ShowDirectionEffect("Effect/Direction1.img/aranTutorial/ClickLilin");
                    await updateAreaInfo(21019, "helper=clear");
                }
                else
                {
                    await SaySpeech([
                        new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_Alright), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_CantRememberAnything), NpcTalkSpeaker.PlayerRight),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_StayCalm), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_YoureHero), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_IslandCalled), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_NameLilin), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                        new SpeechText(this.GetTalkMessage(ScriptMessage.Aran_Npc_IveSaid), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face)
                        ]);
                    await spawnGuide();
                    await warp(140090100, 0);
                }
            }
            else
            {
                var option = await AskMenu(
                    this.GetTalkMessage(ScriptMessage.Aran_Npc_AnythingYoure), [
                        this.GetTalkMessage(ScriptMessage.Aran_Npc_Text),
                        this.GetTalkMessage(ScriptMessage.Aran_Npc_WhereAmI),
                        this.GetTalkMessage(ScriptMessage.Aran_Npc_WhoAreYou),
                        this.GetTalkMessage(ScriptMessage.Aran_Npc_TellHave),
                        this.GetTalkMessage(ScriptMessage.Aran_Npc_TellAbout),
                        this.GetTalkMessage(ScriptMessage.Aran_Npc_AdvanceSkills),
                        this.GetTalkMessage(ScriptMessage.Aran_Npc_WantKnow),
                        this.GetTalkMessage(ScriptMessage.Aran_Npc_UseQuick),
                        this.GetTalkMessage(ScriptMessage.Aran_Npc_OpenBreakable),
                        this.GetTalkMessage(ScriptMessage.Aran_Npc_WantSit)
                        ]);
                switch (option)
                {
                    case 0:
                        await SayNext(this.GetTalkMessage(ScriptMessage.Aran_Npc_OneHeroes));
                        break;
                    case 1:
                        await SayNext(this.GetTalkMessage(ScriptMessage.Aran_Npc_IslandCalledRien));
                        break;
                    case 2:
                        await SayNext(this.GetTalkMessage(ScriptMessage.Aran_Npc_ImLilin, npc));
                        break;
                    case 3:
                        await SayNext(this.GetTalkMessage(ScriptMessage.Aran_Npc_LetsWaste));
                        break;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        await guideHint(option + 10);
                        break;
                    default:
                        break;
                }

            }
        }
    }
}
