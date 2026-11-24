
using Application.Plugin.Script;
using Application.Resources.Messages;
using Application.Shared.Items;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 9010000
        public async Task n9010000()
        {
            var res = await c.CurrentServer.Node.Transport.GetActiveRewards(new ProtoModel.GetRewardsRequest { PlayerId = getPlayer().Id });
            if (res.Rewards.Count == 0)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc9010000_HaveNiceTrip));
                return;
            }
            var idx = await AskMenu("", res.Rewards.Select(x => x.Title));
            await SayNext(res.Rewards[idx].Description);

            // 中心服核销 + 频道发奖是一个整体：背包放不下就撤销核销，不会出现「奖励没了但道具没到手」
            var code = await c.CurrentServer.NodeService.ItemService.ClaimReward(getPlayer(), res.Rewards[idx].Id);
            if (code != UseCdkResponseCode.Success)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9010000_AlreadyClaimed));
            }
        }
    }
}
