using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.Warrior4thJob
{
    internal sealed class Warrior4thJobNpcHandler : NpcScriptBase
    {
        public Warrior4thJobNpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        #region 寒冰掌学习
        // Npc: 2081009 
        public async Task s4blocking_enter()
        {
            if (isQuestStarted(6180) && getQuestProgressInt(6180, 9300096) < 200)
            {
                if (await AskYesNo("请注意：在你待在训练场内的时候，确保你已经装备了#t1092041#，这非常重要。你准备好去训练场了吗？"))
                {
                    if (getPlayer().haveItemEquipped(1092041))
                    {
                        await warp(924000001, 0);
                    }
                    else
                    {
                        await SayOK("请在进入训练场之前装备#r#t1092041##k。");
                        return;
                    }
                }
            }
            else
            {
                await SayOK("只有指定人员才能进入训练场地。");
            }
        }

        // Npc: 2081010 
        public async Task s4blocking()
        {
            if (await AskYesNo("你想要退出这个区域吗？如果你退出，你将需要从头开始这个任务。"))
            {
                await warp(240010400, "st00");
            }
        }
        #endregion


        #region 圣灵之剑学习
        // Npc: 2083005 
        public async Task s4holycharge()
        {
            if (isQuestStarted(6280))
            {
                if (hasItem(4031454))
                {
                    await SayOK("你从喷泉里往杯子里倒了一些水。");
                    await gainItem(4031454, -1);
                    await gainItem(4031455, 1);
                }
            }
        }
        #endregion

    }
}
