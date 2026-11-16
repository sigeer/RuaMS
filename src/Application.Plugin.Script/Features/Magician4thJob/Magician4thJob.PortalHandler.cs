using Application.Resources.Messages;
using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Magician4thJob
{
    internal sealed class Magician4thJobPortalHandler : PortalPlayerInteraction
    {
        public Magician4thJobPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 230040300 | pt: 8
        public async Task<bool> s4resurrection()
        {

            if (haveItem(4001108))
            {
                return await TryStartSoloQuest(Magician4thJobEvent6132.QuestIdValue);
            }

            await this.Pink(this.GetTalkMessage(ScriptMessage.PortalScript_MysteriousForce));
            return false;
        }



        // Map: 220070400 | pt: 11
        public async Task<bool> s4resur_enter()
        {

            if (isQuestStarted(6134))
            {
                await playPortalSound();
                await warp(922020000, 0);
                return true;
            }

            await this.Pink(this.GetTalkMessage(ScriptMessage.PortalScript_MysteriousForce));
            return false;
        }

        // Map: 922020000 | pt: 8
        public async Task<bool> s4resur_out()
        {

            if (isQuestStarted(6134))
            {
                if (canHold(4031448))
                {
                    await gainItem(4031448, 1);
                    await playPortalSound();
                    await warp(220070400, 3);

                    return true;
                }
                else
                {
                    await this.Pink(this.GetTalkMessage(ScriptMessage.Magician4thJobPortalHandler_MakeRoom));
                    return false;
                }
            }
            else
            {
                await playPortalSound();
                await warp(220070400, 3);
                return true;
            }
        }


    }
}
