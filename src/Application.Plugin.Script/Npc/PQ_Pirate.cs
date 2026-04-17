using Application.Core.scripting.Events.Abstraction;
using Application.Plugin.Script.Events;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {

        // Npc: 2094000 
        public async Task davyJohn_enter()
        {
            var em = GetEventManager<PQ_Pirate>(nameof(PQ_Pirate));

            var option = await SayOption($"#e#b<组队任务：海盗船>\r\n#k#n{em.GetRequirementDescription(c)}\r\n\r\n救命啊！我的儿子被绑在可怕的#r海盗领主#k手中。我需要你的帮助... 你能组建或加入一个队伍来救他吗？请让你的#b队长#k与我交谈或者组建一个队伍。",
                ["我想参加组队任务。", "我想了解更多详情。"]
            );
            switch (option)
            {
                case 0:
                    await SayOK(em.HandleCreateInstanceResult(em.StartInstance(getPlayer()), c));
                    break;
                case 1:
                    await SayOK("#e#b<组队任务：海盗船>#k#n\r\n在这个组队任务中，你的任务是逐步穿过船舱，与途中的所有海盗和坏蛋战斗。当你到达#r海盗领主#k时，根据之前阶段打开的宝箱数量，boss会变得更加强大，所以要保持警惕。如果打开了这些宝箱，将会给你的船员带来许多额外的奖励，值得一试！祝你好运。");
                    break;
                default:
                    break;
            }
        }


        // Npc: 2094001 
        public Task davy_clear()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2094002 
        public async Task davyJohn_play()
        {
            var eim = getEventInstance();
            var status = eim.ClearedMaps.GetValueOrDefault(getMapId(), Core.scripting.Events.Abstraction.StageStatus.NotStarted);
            switch (getMapId())
            {
                case 925100000:
                    await SayNext("我们现在要前往海盗船！要进入船内，我们必须消灭所有守卫的怪物。");
                    break;
                case 925100100:
                    var emp = eim.getProperty("stage2");
                    if (emp == "0")
                    {
                        if (haveItem(4001120, 20))
                        {
                            gainItem(4001120, -20);
                            getMap().killAllMonsters();
                            eim.setProperty("stage2", "1");

                            await SayNext("太棒了！现在去收集20个#i4001121# #t4001121#给我。");
                        }
                        else
                        {
                            await SayNext("我们现在要前往海盗船！为了进入，我们必须证明自己是高贵的海盗。给我猎取20个#i4001120# #t4001120#。");
                        }
                    }
                    else if (emp == "1")
                    {
                        if (haveItem(4001121, 20))
                        {
                            gainItem(4001121, -20);
                            getMap().killAllMonsters();
                            eim.setProperty("stage2", "2");

                            await SayNext("太棒了！现在去收集20个#i4001122# #t4001122#给我。");
                        }
                        else
                        {
                            await SayNext("我们现在要前往海盗船！为了进入，我们必须证明自己是高贵的海盗。给我猎取20个#i4001121# #t4001121#。");
                        }
                    }
                    else if (emp == "2")
                    {
                        if (haveItem(4001122, 20))
                        {
                            gainItem(4001122, -20);
                            getMap().killAllMonsters();
                            eim.setProperty("stage2", "3");

                            eim.ClearedMaps[getMapId()] = StageStatus.Completed;
                            eim.showClearEffect(getMapId());

                            await SayNext("太棒了！现在让我们走吧。");
                        }
                        else
                        {
                            await SayNext("我们现在要前往海盗船！为了进入，我们必须证明自己是高贵的海盗。给我猎取20个#i4001122# #t4001122#。");
                        }
                    }
                    else
                    {
                        await SayNext("下一个阶段已经开放。前进！");
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
