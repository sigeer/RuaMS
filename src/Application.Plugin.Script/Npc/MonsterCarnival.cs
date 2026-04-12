using Application.Core.Scripting.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {

        // Npc: 2042000 
        public Task mc_enter()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2042001, 2042006 
        public Task mc_enter1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2042002 
        public async Task mc_move()
        {
            var talkMap = getMapId();
            if (talkMap == 980000010)
            {
                await SayNext("希望你在怪物嘉年华玩得开心！");
                warp(980000000, 0);
            }
            else if (talkMap == 980030010)
            {
                await SayNext("希望你在怪物嘉年华玩得开心！");
                warp(980030000, 0);
            }
            else
            {
                var talk = $"你想做什么呢？ 如果你没有参加过怪物嘉年华, 在参加之前，你需要知道一些事情! \r\n#b" +
                    $"#L0# 前往怪物嘉年华地图 1.#l \r\n" +
                    $"#L2# 了解怪物嘉年华.#l";

                var option = await SayOption(talk);

                switch (option)
                {
                    case 0:
                        var targetEm = GetEventManager<MonsterCarnivalEventManager>("PQ_CPQ1");
                        if (getLevel() < targetEm.MinLevel)
                        {
                            await SayOK($"你必须至少达到{targetEm.MinLevel}级才能参加怪物嘉年华。当你足够强大时，和我交谈。");
                        }
                        else if (getLevel() > targetEm.MaxLevel)
                        {
                            await SayOK($"很抱歉，只有等级在${targetEm.MinLevel}到${targetEm.MaxLevel}级之间的玩家才能参加怪物嘉年华活动。");
                        }
                        else
                        {
                            getPlayer().saveLocation("MONSTER_CARNIVAL");
                            warp(980000000, 0);
                        }
                        break;
                    case 2:
                        break;
                    default:
                        break;
                }

            }
        }


        // Npc: 2042003, 2042004 
        public Task mc_roomout()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2042005 
        public Task mc2_enter()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2042007 
        public Task mc2_move()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2042008, 2042009 
        public Task mc2_roomout()
        {
            // TODO
            return Task.CompletedTask;
        }

    }
}
