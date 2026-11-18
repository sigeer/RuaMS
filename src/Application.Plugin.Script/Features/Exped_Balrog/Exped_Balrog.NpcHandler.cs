using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Core.Scripting.Events;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Exped_Balrog
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 1061014 
        public async Task balog_accept()
        {
            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(Exped_BalrogFeature.EventName);
            var expedBoss = c.CurrentCulture.GetMobName(em.GetTemplate.BossId);


            if (player.getLevel() < em.MinLevel || player.getLevel() > em.MaxLevel)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Exped_NotEligible, expedBoss));
                return;
            }

            var expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
            if (expedition == null)
            {
                var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Exped_FormPrompt, expedBoss, em.Template.GetRequirementDescription(c), expedBoss));

                if (selection == 1)
                {
                    expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
                    if (expedition != null)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_SomeoneLeader));
                        return;
                    }

                    var r = await em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Exped_SureNotEveryone, expedBoss));
                }
            }
            else if (expedition.isLeader(player))
            {
                if (expedition.isInProgress())
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Exped_InProgress));
                }
                else
                {
                    var list = this.GetTalkMessage(ScriptMessage.Exped_Menu);
                    var selection = await AskMenu(list);

                    if (selection == 1)
                    {
                        var expedMembers = expedition.getPlayers();
                        var size = expedMembers.Count;
                        if (size == 1)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Exped_OnlyMember));
                            return;
                        }
                        var text = this.GetTalkMessage(ScriptMessage.Exped_MemberList);
                        text += "\r\n\t\t1." + expedition.getLeader().getName();
                        for (var i = 1; i < size; i++)
                        {
                            text += "\r\n#b#L" + (i + 1) + "#" + (i + 1) + ". " + expedMembers[i].Name + "#l\n";
                        }
                        var kickSelection = await AskMenu(text);
                        if (kickSelection > 0)
                        {
                            var banned = expedMembers[kickSelection - 1];
                            await expedition.ban(banned.Id);
                            await SayOK(this.GetTalkMessage(ScriptMessage.Exped_Banned, banned.Name));
                        }
                    }
                    else if (selection == 2)
                    {
                        var size = expedition.getPlayers().Count;
                        if (size < expedition.EventManager.MinCount)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Exped_NeedPlayers, expedition.EventManager.MinCount));
                            return;
                        }

                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_WillBegin, expedition.EventManager.EntryMap));

                        await expedition.StartBattle();
                    }
                    else if (selection == 3)
                    {
                        await player.getMap().LightBlue(ScriptMessageCategories.ScriptMessage, ScriptMessage.Exped_LeaderEnded, expedition.getLeader().getName());
                        await expedition.DisposeAsync();
                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_Ended));
                    }
                }
            }
            else if (expedition.isRegistering())
            {
                if (expedition.contains(player))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Exped_Registered, expedition.getLeader().getName()));
                }
                else
                {
                    await SayOK(em.GetTemplate.HandleJoinInstanceResult(await em.GetTemplate.JoinMember(expedition, getPlayer()), c));
                }
            }
            else if (expedition.isInProgress())
            {
                if (expedition.contains(player))
                {
                    if (expedition.getIntProperty("canJoin") == 1)
                    {
                        await expedition.registerPlayer(player);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_InBattle, expedBoss));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Exped_AnotherExped, expedBoss));
                }
            }
        }

        // Npc: 1061016 
        public async Task balog_scroll()
        {
            int[] items = [2040728, 2040729, 2040730, 2040731, 2040732, 2040733, 2040734, 2040735, 2040736, 2040737, 2040738, 2040739];
            await AskMenu(this.GetTalkMessage(ScriptMessage.Balrog_ExchangeBalrog));
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Balrog_Redeem, items.Select(i => $"#i{i}# #z{i}#")));
            if (!canHold(items[option], 1))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Balrog_PleaseMake));
            }
            else if (!haveItemWithId(4001261))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Balrog_DontHave));
            }
            else
            {
                await gainItem(4001261, -1);
                await gainItem(items[option], 1);
                await SayOK(this.GetTalkMessage(ScriptMessage.Balrog_ThankRedemption));
            }
        }

        // Npc: 1061018 
        public async Task balog_InOut()
        {
            var eim = GetEventInstanceTrust();
            if (eim.isEventCleared())
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Balrog_WowDefeated));

                await warp(eim.EventManager.ExitMap, 0);
            }
            else
            {
                if (await AskYesNo(getMap().getAllPlayers().Count > 1 ? this.GetTalkMessage(ScriptMessage.Balrog_ReallyGoing) : this.GetTalkMessage(ScriptMessage.Balrog_YoureCoward)))
                {
                    await eim.exitPlayer(getPlayer());
                }
            }
        }
    }
}
