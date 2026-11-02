using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

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
                var selection = await AskMenu("你有什么要说的吗...? #b\r\n#L0#我想要救出佳佳。#l\r\n");
                if (selection != 0)
                {
                    return;
                }

                await SayNext("欢迎！我从小月兔那里听说了发生的事情，很高兴你来了，因为我正打算请求一些帮助。Gaga是我的朋友，以前帮过我，经常过来打个招呼。不幸的是，他被外星人绑架了。");

                if (await AskYesNo("如果我们把佳佳留给外星人，他会遭遇可怕的事情！我会让你借用一艘月兔用来旅行的太空飞船，这样你就可以救出佳佳。虽然他有时看起来有点犹豫不决、慢吞吞和不成熟，但他其实是一个很好的年轻人。你现在想去救他吗？"))
                {
                    var em = GetEventManager(RescueGagaFeature.EventName);
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                }
            }
            else if (mapId >= 922240000 && mapId <= 922240019)
            {
                if (await AskYesNo("如果你失败了也不要担心。你还有3次机会。你还想放弃吗？"))
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
