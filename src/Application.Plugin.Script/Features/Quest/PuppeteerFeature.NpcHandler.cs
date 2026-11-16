using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

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
                await SayOK(this.GetTalkMessage(ScriptMessage.Puppet_BlockedPath));
                await setQuestProgress(21728, 21761, 0);
                return;
            }

            var pwd = await AskText(this.GetTalkMessage(ScriptMessage.Puppet_PasswordPrompt));
            if (pwd == this.GetTalkMessage(ScriptMessage.Puppet_Password))
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
                    await this.Pink(this.GetTalkMessage(ScriptMessage.Puppet_RightAnswerBlocked));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2103008_Error));
            }
        }

        // Npc: 1063016 
        public async Task DollWayKeeper1()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Puppet_ExitTrial)))
            {
                await warp(105040201, 2);
            }
        }

        // Npc: 1063017 
        public async Task DollWayKeeper2()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Puppet_AheadAwaits)))
            {
                // 925020010？
                if ((await getMap(910510202)).getAllPlayers().Count > 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Puppet_SomeoneAlready));
                }
                else
                {
                    await warp(910510202, 0);
                }
            }
        }

    }
}
