using Application.Core.Managers;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 2010007 
        public async Task guild_proc()
        {
            var selection = await AskMenu("欢迎来到家族公馆，你现在想做什么呢?", [
                "创建家族",
                "解散家族",
                "增加家族成员人数上限"
            ]);

            switch (selection)
            {
                case 0:
                    if (getPlayer().getGuildId() > 0)
                    {
                        await SayOK("你已经拥有家族了，不能再创建家族。");
                        return;
                    }
                    if (await AskYesNo("创建一个新的家族需要 #b 1500000 金币#k，你确定继续创建一个新的家族吗？"))
                    {
                        await getPlayer().genericGuildMessage(1);
                    }
                    break;
                case 1:
                    if (getPlayer().getGuildId() < 1 || getPlayer().getGuildRank() != 1)
                    {
                        await SayOK("你还没有家族！或者\r 你不是族长，因此你不能解散该家族.");
                        return;
                    }
                    if (await AskYesNo("你确定真的要解散你的家族？当解散后你将不能恢复所有家族相关资料以及GP的数值，是否继续？"))
                    {
                        disbandGuild();
                    }
                    break;
                case 2:
                    if (getPlayer().getGuildId() < 1 || getPlayer().getGuildRank() != 1)
                    {
                        await SayOK("你不是族长，因此你将不能增加家族成员的人数上限.");
                        return;
                    }
                    if (await AskYesNo($"家族成员人数每增加 #b5#k 位需要支付#b {GuildManager.getIncreaseGuildCost(getPlayer().GetGuild()!.Capacity)}金币#k，你确定要继续吗？"))
                    {
                        await increaseGuildCapacity();
                    }
                    break;
            }
        }


        // Npc: 2010008 
        public async Task guild_mark()
        {
            var selection = await AskMenu("你想做什么？", ["创建/更改你的家族徽标"]);

            if (selection == 0)
            {
                if (getPlayer().getGuildRank() == 1)
                {
                    if (await AskYesNo("创建或更改家族徽标需要 #b 5000000 金币#k，您确定要继续吗？"))
                    {
                        await getPlayer().genericGuildMessage(17);
                    }
                }
                else
                {
                    await SayOK("你必须是家族族长才能更改徽标。请告诉你的族长与我交谈。");
                }
            }
        }


        // Npc: 2010009 
        public async Task guild_union()
        {
            if (getPlayer().getGuildId() < 1 || getPlayer().getGuildRank() != 1)
            {
                await SayNext("你好！我是#b#p2010009##k。只有家族会长才能尝试组建家族联盟。");
                return;
            }

            var choice = await AskMenu("你好！我是#bLenario#k。", [
                "你能告诉我家族联盟是什么吗？",
                "我如何创建家族联盟？",
                "我想创建一个家族联盟。",
                "我想为家族联盟添加更多公会。",
                "我想解散家族联盟。"
            ]);

            switch (choice)
            {
                case 0:
                    await SayNext("家族联盟就像它的名字一样，是由多个公会组成的超级团体。我负责管理这些家族联盟。");
                    break;
                case 1:
                    await SayNext("要创建家族联盟，需要两位且仅限两位公会会长组成一个队伍，并且必须在同一个频道的房间内。该队伍的队长将被指定为家族联盟的主人。\r\n最初，只有两个公会可以加入新的联盟，但随着时间的推移，你可以通过在特定时机与我交谈并投资一定费用来扩大联盟的容量。");
                    break;
                case 2:
                    if (!isLeader())
                    {
                        await SayNext("如果你想组建家族联盟，请告诉你的队伍领袖与我交谈。他/她将被指定为家族联盟的领袖。");
                        return;
                    }
                    if (GetGuild()!.AllianceId > 0)
                    {
                        await SayOK("当你的公会已经注册在其他家族联盟中时，你无法创建家族联盟。");
                        return;
                    }
                    if (!await AskYesNo($"哦，你对组建家族联盟感兴趣吗？目前这项操作的费用是 #b2000000 冒险币#k。"))
                        return;
                    if (getMeso() < 2000000)
                    {
                        await SayOK("你没有足够的金币来完成这个请求。");
                        return;
                    }

                    var guildName = await AskText("Now please enter the name of your new Guild Union. (max. 12 letters)");
                    if (await AskYesNo($"'{guildName}'会成为你的家族联盟的名字吗？"))
                    {
                        if (!canBeUsedAllianceName(guildName))
                        {
                            await SayNext("此名称不可用，请选择另一个。");
                            return;
                        }
                        CreateAllianceAysnc(guildName, 2000000);
                    }
                    break;
                case 3:
                    if (GetGuild() == null)
                    {
                        await SayOK("如果你没有公会，就无法扩展家族联盟。");
                        return;
                    }
                    if (getPlayer().AllianceRank == 1)
                    {
                        if (getAllianceCapacity() >= 5)
                        {
                            await SayOK("你的联盟已经达到了公会的最大容量。");
                            return;
                        }
                        if (await AskYesNo($"你想要增加你的家族联盟 #rone guild#k 的位置吗？这个手续的费用是 #b1000000 冒险币#k。"))
                        {
                            if (getMeso() < 1000000)
                            {
                                await SayOK("你没有足够的金币来完成这个请求。");
                                return;
                            }
                            upgradeAlliance();
                            await gainMeso(-1000000);
                            await SayOK("你的联盟现在可以接受一个额外的公会。");
                        }
                    }
                    else
                    {
                        await SayNext("只有家族联盟会长才能扩大家族联盟中的公会数量。");
                    }
                    break;
                case 4:
                    var alliance = GetAlliance();
                    if (alliance == null)
                    {
                        await SayOK("如果你没有家族联盟，你就无法解散家族联盟。");
                        return;
                    }
                    if (getPlayer().AllianceRank == 1)
                    {
                        if (await AskYesNo("你确定要解散你的家族联盟吗？"))
                        {
                            disbandAlliance(c, alliance.AllianceId);
                            await SayOK("你的家族联盟已经解散。");
                        }
                    }
                    else
                    {
                        await SayNext("只有家族联盟的会长才能解散家族联盟。");
                    }
                    break;
            }
        }
    }
}
