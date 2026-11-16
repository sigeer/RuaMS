using Application.Core.Client;
using Application.Core.scripting.item;
using Application.Utility.Extensions;
using client.inventory;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script
{
    internal partial class ItemScript : ItemScriptBase
    {
        public ItemScript(IChannelClient c, Item item, int npcId) : base(c, item, npcId)
        {
        }

        public async Task killarmush()
        {
            if (getMapId() == 106020300)
            {
                var portal = getMap().getPortal("obstacle");
                if (portal != null && portal.getPosition().distance(getPlayer().getPosition()) < 240)
                {
                    if (!(isQuestStarted(100202) || isQuestCompleted(100202)))
                    {
                        await startQuest(100202);
                    }
                    await removeAll(2430014);
                    await ShowEffect("Effect/OnUserEff/normalEffect/mushroomcastle/chatBalloon3");
                    await this.Pink(this.GetTalkMessage(ScriptMessage.Item_BarrierGone));
                }
                else
                {
                    await this.Pink(this.GetTalkMessage(ScriptMessage.Item_NeedCloserToBarrier));
                }
            }
            else
            {
                await this.Pink(this.GetTalkMessage(ScriptMessage.Item_NoBarrierHere));
            }

        }

        public async Task removethorns()
        {
            if (getMapId() == 106020500 && isQuestActive(2324))
            {
                var player = getPlayer();
                var portal = getMap().getPortal("investigate2");
                if (portal != null && portal.getPosition().distance(player.getPosition()) < 150)
                {
                    await player.gainExp((int)(3300 * player.getExpRate()));
                    await forceCompleteQuest(2324);
                    await removeAll(2430015);
                    await this.LightBlue(this.GetTalkMessage(ScriptMessage.Item_ThornsCleared));
                    await warp(106020502);
                }
                else
                {
                    await this.Pink(this.GetTalkMessage(ScriptMessage.Item_NeedCloserToThorns));
                }
            }
            else
            {
                await TypedMessage(NoticeType.Pink, this.GetTalkMessage(ScriptMessage.Item_NoThornsHere));
            }
        }

        /// <summary>
        /// 北斗卫星手册（2430033）：集齐 7 本可兑换 100 万冒险币。
        /// </summary>
        public async Task BeiDouSatelliteManual()
        {
            if (haveItem(2430033, 7))
            {
                await gainMeso(1000000);
                await SayOK(this.GetTalkMessage(ScriptMessage.Item_BeiDouSatellite_Reward));
                await gainItem(2430033, -7);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Item_BeiDouSatellite_NeedMore));
            }
        }
    }
}
