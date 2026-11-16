using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Shared.GameProps;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript : NpcScriptBase
    {
        public NpcScript(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        // Npc: 2103000
        public async Task ariant_oasis()
        {
            bool isTigunMorphed()
            {
                return getPlayer().getBuffSource(BuffStat.MORPH) == 2210005;
            }

            if (isQuestStarted(3900) && getQuestProgressInt(3900) != 5)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2103000_OasisWater));
                await setQuestProgress(3900, 5);
            }
            else if (isQuestCompleted(3938))
            {
                if (canHold(2210005))
                {
                    if (!haveItem(2210005) && !isTigunMorphed())
                    {
                        await gainItem(2210005, 1);
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc2103000_FoundHair));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc2103000_NoUseSlot));
                }
            }
            else if (isQuestStarted(3934) || (isQuestCompleted(3934) && !isQuestCompleted(3935)))
            {
                if (canHold(2210005))
                {
                    if (!haveItem(2210005) && !isTigunMorphed())
                    {
                        await gainItem(2210005, 1);
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc2103000_FoundFlask));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc2103000_IgnoredFlask));
                }
            }
        }

        // Npc: 2103001
        public async Task secret_wall()
        {
            if (isQuestStarted(3927))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc2103001_HammerAndDagger));
                await setQuestProgress(3927, 1);
            }
        }


        // Npc: 2103002
        public async Task ariant_ring()
        {
            if (isQuestStarted(3923) && !haveItem(4031578, 1))
            {
                if (canHold(4031578, 1))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc2103002_StoleRing));
                    await gainItem(4031578, 1);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc2103002_NoEtcSlot));
                }
            }
        }


        // Npc: 2103003
        public async Task ariant_house1()
        {
            await ProcessFoodDepot(0);
        }

        // Npc: 2103004
        public async Task ariant_house2()
        {
            await ProcessFoodDepot(2);
        }

        // Npc: 2103005
        public async Task ariant_house3()
        {
            await ProcessFoodDepot(1);
        }

        // Npc: 2103006
        public async Task ariant_house4()
        {
            await ProcessFoodDepot(3);
        }

        // Npc: 2103009
        public async Task ariant_gold1()
        {
            await ProcessJewelDepot(0);
        }

        // Npc: 2103010
        public async Task ariant_gold2()
        {
            await ProcessJewelDepot(2);
        }

        // Npc: 2103011
        public async Task ariant_gold3()
        {
            await ProcessJewelDepot(1);
        }

        // Npc: 2103012
        public async Task ariant_gold4()
        {
            await ProcessJewelDepot(3);
        }

        private async Task ProcessFoodDepot(int slot)
        {
            if (isQuestStarted(3929))
            {
                var progress = getQuestProgress(3929);
                var ch = progress[slot];

                if (ch == '2')
                {
                    var nextProgress = progress.Substring(0, slot) + '3' + progress.Substring(slot + 1);
                    await gainItem(4031580, -1);
                    await setQuestProgress(3929, nextProgress);
                }
            }
        }

        private async Task ProcessJewelDepot(int slot)
        {
            if (isQuestStarted(3926))
            {
                var progress = getQuestProgress(3926);
                var ch = progress[slot];

                if (ch == '2')
                {
                    var nextProgress = progress.Substring(0, slot) + '3' + progress.Substring(slot + 1);
                    await gainItem(4031579, -1);
                    await setQuestProgress(3926, nextProgress);
                }
            }
        }

    }
}
