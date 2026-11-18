using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.RescueGaga
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        /// <summary>兔子爷爷：开启挑战、中途放弃、失败后发放飞船技能。</summary>
        public async Task spaceGaGa_papa()
        {
            var mapId = getPlayer().getMapId();
            if (mapId == RescueGagaFeature.EXIT_MAP)
            {
                var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.RescueGaga_Head_SomethingWant));
                if (selection != 0)
                {
                    return;
                }

                await SayNext(this.GetTalkMessage(ScriptMessage.RescueGaga_Head_WelcomeHeard));

                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.RescueGaga_Head_LeaveGaga)))
                {
                    var em = GetEventManager(RescueGagaFeature.EventName);
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                }
            }
            else if (mapId >= 922240000 && mapId <= 922240019)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.RescueGaga_Head_DontWorry)))
                {
                    await warp(RescueGagaFeature.EXIT_MAP, 0);
                }
            }
            else if (mapId >= 922240100 && mapId <= 922240119)
            {
                var eim = getEventInstance();
                string text = "You went through so much trouble to rescue Gaga, but it looks like we're back to square one. ";
                if (getPlayer().Events.GetValueOrDefault(RescueGagaFeature.EventName) > 10 && eim?.EventManager.Template is Event template)
                {
                    text += "Please don't give up until Gaga is rescued. To show you my appreciation for what you've accomplished thus far, I've given you a Spaceship. It's rather worn out, but it should still be operational. Check your #bSkill Window#k.";
                    await template.GiveSkill(getPlayer());
                }
                else
                {
                    text += "Let's go back now.";
                }

                await SayNext(text);
                await warp(RescueGagaFeature.EXIT_MAP, 0);
            }
        }
    }
}
