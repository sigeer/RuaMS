using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Core.Scripting.Events;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Zakum
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }


        // Npc: 2030013
        public async Task zakum_accept()
        {
            var expedItem = 4001017;
            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(ZakumFeature.ExpeditionEventName);
            var expedBoss = c.CurrentCulture.GetMobName(em.GetTemplate.BossId);


            if (player.getLevel() < em.MinLevel || player.getLevel() > em.MaxLevel)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_NotEligible, expedBoss));
                return;
            }

            var expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
            if (expedition == null)
            {
                var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Zakum_ExpeditionMenu, expedBoss, em.Template.GetRequirementDescription(c)));

                if (selection == 1)
                {
                    if (!haveItem(expedItem))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_NeedItemOnLeader, expedItem, expedBoss));
                        return;
                    }

                    expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
                    if (expedition != null)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_SomeoneIsLeader));
                        return;
                    }

                    var r = await em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_NotForEveryone, expedBoss));
                }
            }
            else if (expedition.isLeader(player))
            {
                if (expedition.isInProgress())
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_AlreadyInProgress));
                }
                else
                {
                    var list = this.GetTalkMessage(ScriptMessage.Exped_Menu);
                    var selection = await AskMenu(list);

                    if (selection == 1)
                    {
                        var expedMembers = expedition.GetPlayerSortList();
                        var size = expedMembers.Count;
                        if (size == 1)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_OnlyMember));
                            return;
                        }
                        var text = this.GetTalkMessage(ScriptMessage.Exped_MemberList);
                        text += "\r\n\t\t1." + expedMembers[0].Name;
                        for (var i = 1; i < size; i++)
                        {
                            text += "\r\n#b#L" + (i + 1) + "#" + (i + 1) + ". " + expedMembers[i].Name + "#l\n";
                        }
                        var kickSelection = await AskMenu(text);
                        if (kickSelection > 0)
                        {
                            var banned = expedMembers[kickSelection - 1];
                            await expedition.ban(banned.Id);
                            await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_Banned));
                        }
                    }
                    else if (selection == 2)
                    {
                        var size = expedition.getPlayers().Count;
                        if (size < expedition.EventManager.MinCount)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_NeedMinPlayers));
                            return;
                        }

                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_WillBegin, expedition.EventManager.EntryMap));

                        await expedition.StartBattle();
                    }
                    else if (selection == 3)
                    {
                        await player.getMap().LightBlue(ScriptMessageCategories.ScriptMessage, ScriptMessage.Zakum_ExpeditionOver);
                        await expedition.DisposeAsync();
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_ExpeditionEnded));
                    }
                }
            }
            else if (expedition.isRegistering())
            {
                if (expedition.contains(player))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_AlreadyRegistered));
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
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_BattleStarted, expedBoss));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_AnotherExpedition, expedBoss));
                }
            }
        }

    }
}
