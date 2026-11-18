using Application.Resources.Messages;
using Application.Core.Client;
using Application.Core.Game.Players;
using Application.Core.Server.maps;
using Application.Shared.GameProps;
using scripting.portal;

namespace Application.Plugin.Script.Features.PQ_Ariant
{
    internal sealed class PQ_AriantPortalHandler : PortalPlayerInteraction
    {
        public PQ_AriantPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 980010101 | pt: 9
        public Task<bool> aM_start()  => Task.FromResult(true);


        // Map: 980010000 | pt: 7
        public async Task<bool> aMatchMove2()
        {

            await playPortalSound();
            await warp(getPlayer().getSavedLocation("MIRROR"));
            return true;
        }

        // Map: 980010010 | pt: 7
        public async Task<bool> ariantMout()
        {

            await playPortalSound();
            await warp(980010020, 0);
            return true;
        }

        // Map: 980010020 | pt: 7
        public async Task<bool> ariantMout2()
        {

            await playPortalSound();
            await warp(980010000, 0);
            return true;
        }

        // Map: 260000200 | pt: 8
        public async Task<bool> ariant_Agit()
        {

            if (isQuestCompleted(3928) && isQuestCompleted(3931) && isQuestCompleted(3934))
            {
                await playPortalSound();
                await warp(260000201, 1);
                return true;
            }
            else
            {
                await this.Pink(this.GetTalkMessage(ScriptMessage.PQAriantPortalHandler_OnlyMembers));
                return false;
            }
        }

        // Map: 260000300 | pt: 7
        public async Task<bool> ariant_castle()
        {

            if (getPlayer().haveItem(4031582) == true)
            {
                await playPortalSound();
                await warp(260000301, 5);
                return true;
            }
            else
            {
                await this.Pink(this.GetTalkMessage(ScriptMessage.PQAriantPortalHandler_OnlyHolding));
                return false;
            }
        }

        static bool isTigunMorphed(Player chr)
        {
            return chr.getBuffSource(BuffStat.MORPH) == 2210005;
        }
        // Map: 260000301, 260000302, 260000303 | pt: 9
        public async Task<bool> ariant_queens()
        {

            if (isTigunMorphed(getPlayer()))
            {
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(260000300, 7);
                await this.Pink(this.GetTalkMessage(ScriptMessage.PQAriantPortalHandler_IntruderHave));
                return true;
            }
        }

    }
}
