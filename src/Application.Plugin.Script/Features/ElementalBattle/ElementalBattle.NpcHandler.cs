using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.ElementalBattle
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 2041023 
        public async Task s4efreet()
        {
            if (!(isQuestCompleted(6316) && (isQuestStarted(6225) || isQuestStarted(6315))))
            {
                await SayOK("你似乎没有理由去遇见基于元素的萨那托斯。");
                return;
            }

            var em = GetEventManager(ElementalBattleFeature.EventName);
            var option = await AskMenu($"#e#b<组队任务：元素塔纳托斯>\r\n#k#n{em.Template.GetRequirementDescription(c)}\r\n\r\n你正在寻找元素塔纳托斯，对吗？如果你和另一个法师组队，而且他的元素属性与你相反，你们就能够克服它们。作为队长，当你准备好出发时，和我交谈。", [
                "我想参加组队任务。",
            "我想了解更多细节。"
            ]);

            if (option == 0)
            {
                var r = await em.StartInstance(getPlayer());
                await SayOK(em.HandleCreateInstanceResult(r, c));
            }
            else
            {
                await SayOK("#e#b<组队任务：元素死神>#k#n\r\n在进入舞台之前，与另一位具有#rdifferent elemental affinity#k的法师组队。这个团队合作对于克服内部的元素非常关键。");
            }
        }
    }
}
