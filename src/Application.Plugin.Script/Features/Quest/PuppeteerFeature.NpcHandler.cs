using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class PuppeteerFeatureNpcHandler : NpcScriptBase
    {
        public PuppeteerFeatureNpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }




        // Npc: 1063011 
        /// <summary>
        /// <see cref="PortalScript.enterDollcave"/>
        /// </summary>
        /// <returns></returns>
        public async Task Dollcave()
        {
            if (isQuestStarted(21728))
            {
                await SayOK("你寻找着线索，试图找到操纵者的蛛丝马迹，但似乎有一股强大的力量挡住了你的去路……最好返回找 #b#p1061019##k。");
                await setQuestProgress(21728, 21761, 0);
                return;
            }

            var pwd = await AskText("一个可疑的声音打破了沉默。 #b暗号#k ！");
            if (pwd == "弗朗西斯是天才傀儡师！")
            {
                if (isQuestStarted(PuppeteerFeature.PuppeteerCygnus) && getQuestProgressInt(PuppeteerFeature.PuppeteerCygnus, 9300285) == 0)
                {
                    await warp(910510001, 1);
                }
                else if (isQuestStarted(PuppeteerFeature.PuppeteerAran) && getQuestProgressInt(PuppeteerFeature.PuppeteerAran, 9300346) == 0)
                {
                    await warp(910510001, 1);
                }
                else
                {
                    await Pink("尽管你说出了正确的答案，但一些神秘的力量正在阻挡进入的道路。");
                }
            }
            else
            {
                await SayOK("#r错误！");
            }
        }

        // Npc: 1063016 
        public async Task DollWayKeeper1()
        {
            if (await AskYesNo("你要退出这个试炼吗？"))
            {
                await warp(105040201, 2);
            }
        }

        // Npc: 1063017 
        public async Task DollWayKeeper2()
        {
            if (await AskYesNo("欢迎回来，弗朗西斯主人。要进入主人的洞窟吗？"))
            {
                // 925020010？
                if ((await getMap(910510202)).getAllPlayers().Count > 0)
                {
                    await SayOK("有人在里面了。请稍后再进入。");
                }
                else
                {
                    await warp(910510202, 0);
                }
            }
        }

    }
}
