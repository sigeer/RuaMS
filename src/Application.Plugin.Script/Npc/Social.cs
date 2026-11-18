using Application.Core.Managers;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 2010007 
        public async Task guild_proc()
        {
            var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Social_WelcomeGuild), [
                this.GetTalkMessage(ScriptMessage.Social_CreateGuild),
                this.GetTalkMessage(ScriptMessage.Social_DisbandGuild),
                this.GetTalkMessage(ScriptMessage.Social_IncreaseGuilds)
            ]);

            switch (selection)
            {
                case 0:
                    if (getPlayer().getGuildId() > 0)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Social_CreateNew));
                        return;
                    }
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Social_CreatingGuild)))
                    {
                        await getPlayer().genericGuildMessage(1);
                    }
                    break;
                case 1:
                    if (getPlayer().getGuildId() < 1 || getPlayer().getGuildRank() != 1)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Social_OnlyDisband));
                        return;
                    }
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Social_SureWant)))
                    {
                        await disbandGuild();
                    }
                    break;
                case 2:
                    if (getPlayer().getGuildId() < 1 || getPlayer().getGuildRank() != 1)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Social_OnlyIncrease));
                        return;
                    }
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Social_IncreasingGuild)))
                    {
                        await increaseGuildCapacity();
                    }
                    break;
            }
        }


        // Npc: 2010008 
        public async Task guild_mark()
        {
            var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.A30_WantHmmm, [this.GetTalkMessage(ScriptMessage.Social_CreateChange)]));

            if (selection == 0)
            {
                if (getPlayer().getGuildRank() == 1)
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Social_CreatingChanging)))
                    {
                        await getPlayer().genericGuildMessage(17);
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Social_GuildLeader));
                }
            }
        }


        // Npc: 2010009 
        public async Task guild_union()
        {
            if (getPlayer().getGuildId() < 1 || getPlayer().getGuildRank() != 1)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Social_ImLenario, npc));
                return;
            }

            var choice = await AskMenu(this.GetTalkMessage(ScriptMessage.Social_ImLenario, npc), [
                this.GetTalkMessage(ScriptMessage.Social_PleaseTell),
                this.GetTalkMessage(ScriptMessage.Social_MakeGuild),
                this.GetTalkMessage(ScriptMessage.Social_WantMake),
                this.GetTalkMessage(ScriptMessage.Social_WantAdd),
                this.GetTalkMessage(ScriptMessage.Social_WantBreak)
            ]);

            switch (choice)
            {
                case 0:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Social_GuildUnionJust));
                    break;
                case 1:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Social_MakeGuildUnion));
                    break;
                case 2:
                    if (!isLeader())
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Social_WantForm));
                        return;
                    }
                    if (GetGuild()!.AllianceId > 0)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Social_CreateGuildUnion));
                        return;
                    }

                    var price = 2000000;
                    if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Social_InterestedForming)))
                        return;
                    if (getMeso() < price)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Social_DontHave));
                        return;
                    }

                    var guildName = await AskText(this.GetTalkMessage(ScriptMessage.GuildUnion_NamePrompt));
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Social_NameGuild)))
                    {
                        if (!canBeUsedAllianceName(guildName))
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Social_NameUnavailable));
                            return;
                        }
                        await CreateAllianceAysnc(guildName, price);
                    }
                    break;
                case 3:
                    if (GetGuild() == null)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Social_ExpandGuild));
                        return;
                    }
                    if (getPlayer().AllianceRank == 1)
                    {
                        if (getAllianceCapacity() >= 5)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Social_AllianceAlready));
                            return;
                        }
                        var increaseCapacityPrice = 1000000;
                        if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Social_WantIncrease)))
                        {
                            if (getMeso() < increaseCapacityPrice)
                            {
                                await SayOK(this.GetTalkMessage(ScriptMessage.Social_DontHave));
                                return;
                            }
                            await upgradeAlliance();
                            await gainMeso(-increaseCapacityPrice);
                            await SayOK(this.GetTalkMessage(ScriptMessage.Social_AllianceAccept));
                        }
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Social_OnlyGuild));
                    }
                    break;
                case 4:
                    var alliance = GetAlliance();
                    if (alliance == null)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Social_DisbandGuildUnion));
                        return;
                    }
                    if (getPlayer().AllianceRank == 1)
                    {
                        if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Social_SureWantDisband)))
                        {
                            await disbandAlliance(c, alliance.AllianceId);
                            await SayOK(this.GetTalkMessage(ScriptMessage.Social_GuildUnion));
                        }
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Social_OnlyGuildUnion));
                    }
                    break;
            }
        }
    }
}
