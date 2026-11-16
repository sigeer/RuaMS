using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

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
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Warrior4th_PayAttention)))
                {
                    if (getPlayer().haveItemEquipped(1092041))
                    {
                        await warp(924000001, 0);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Warrior4th_PleaseEquip));
                        return;
                    }
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Warrior4th_OnlyAssigned));
            }
        }

        // Npc: 2081010 
        public async Task s4blocking()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Warrior4th_WantExit)))
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.Warrior4th_PouredSome));
                    await gainItem(4031454, -1);
                    await gainItem(4031455, 1);
                }
            }
        }
        #endregion

    }
}
