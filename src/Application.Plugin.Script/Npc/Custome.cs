namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 9010000 
        public async Task n9010000()
        {
            var res = await c.CurrentServer.Node.Transport.GetActiveRewards(new ItemProto.GetRewardsRequestProto { PlayerId = getPlayer().Id });
            if (res.Rewards.Count == 0)
            {
                await SayNext("旅行愉快。");
                return;
            }
            var idx = await AskMenu("", res.Rewards.Select(x => x.Title));
            await SayNext(res.Rewards[idx].Description);
            var takeRes = await c.CurrentServer.Node.Transport.TakeReward(new ItemProto.UseIdRequest { Id = res.Rewards[idx].Id, MasterId = getPlayer().Id });
            if (takeRes.Code == 0)
            {
                foreach (var item in takeRes.Items)
                {
                    await getPlayer().GainItem(item.ItemId, item.Quantity);
                }
            }
            else
            {
                await SayOK("你已经领取过了，或者已经过期了");
            }
        }
    }
}
