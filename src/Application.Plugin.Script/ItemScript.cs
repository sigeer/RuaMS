using Application.Core.Client;
using Application.Utility.Extensions;
using scripting.npc;

namespace Application.Plugin.Script
{
    internal class ItemScript : NPCConversationManager
    {
        public ItemScript(IChannelClient c, int npcId) : base(c, npcId, -1, null)
        {
        }

        public Task killarmush()
        {
            if (getMapId() == 106020300)
            {
                var portal = getMap().getPortal("obstacle");
                if (portal != null && portal.getPosition().distance(getPlayer().getPosition()) < 240)
                {
                    if (!(isQuestStarted(100202) || isQuestCompleted(100202)))
                    {
                        startQuest(100202);
                    }
                    removeAll(2430014);
                    showInfo("Effect/OnUserEff/normalEffect/mushroomcastle/chatBalloon2");
                    Pink("好像有什么动静...嗯？是结界被消除了");
                }
                else
                {
                    message("尽可能的接近魔法结界才能将其消除");
                }
            }
            else
            {
                message("这里似乎没有需要消除的魔法结界");
            }
            return Task.CompletedTask;

        }

        public Task removethorns()
        {
            if (getMapId() == 106020500 && isQuestActive(2324))
            {
                var player = getPlayer();
                var portal = getMap().getPortal("investigate2");
                if (portal != null && portal.getPosition().distance(player.getPosition()) < 150)
                {
                    player.gainExp((int)(3300 * player.getExpRate()));
                    forceCompleteQuest(2324);
                    removeAll(2430015);
                    playerMessage(6, "使用尖刺消除剂清除道路上的荆棘。");
                    warp(106020501);
                }
                else
                {
                    playerMessage(5, "尽可能得离荆棘更近些才能完全清除");
                }
            }
            else
            {
                playerMessage(5, "这里没有荆棘需要清除");
            }
            return Task.CompletedTask;
        }
    }
}
