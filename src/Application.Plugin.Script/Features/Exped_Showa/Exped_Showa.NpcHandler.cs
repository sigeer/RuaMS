using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Core.Scripting.Events;
using Application.Shared.Events;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Exped_Showa
{
    /// <summary>昭和村远征相关 NPC。</summary>
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, Application.Core.Game.Life.NPC? npcObject)
            : base(c, npc, npcObject)
        {
        }

        // Npc: 9120015 
        [ScriptName("To the Showa manor...")]
        public async Task s_To_the_Showa_manor()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Showa_WantGather));
            switch (option)
            {
                case 0:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Showa_TakeHideout));
                    break;
                case 1:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Showa_BraveOne));
                    break;

                case 2:
                    await SayOK(this.GetTalkMessage(ScriptMessage.Showa_ImBusy));
                    break;
                default:
                    break;
            }
        }

        // Npc: 9120200 
        public async Task con2()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Showa_RightFront)))
            {
                await warp(801000000);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Showa_WantReturn));
            }
        }

        // Npc: 9120202 
        public async Task con3()
        {
            var eim = getEventInstance();
            if (eim == null || !eim.isEventCleared())
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Showa_LeaveWont)))
                {
                    await warp(801040004, 1);
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Showa_GuysGreat));

                var result = await eim!.GiveClearReward(getPlayer());
                if (result != ClaimRewardResult.Success)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Showa_PleaseMake));
                }
                else
                {
                    await warp(801040101);
                }
            }
        }

        // Npc: 9120203 
        public async Task con4()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Showa_AhBoss));
            await warp(801000000);
        }

        // Npc: 9120201 
        public async Task s_dungeon()
        {
            var exped = ExpeditionType.SHOWA;
            var expedName = "Showa Gang";
            var expedMap = "Nightmarish Last Days";
            var expedItem = 4000138;

            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(Exped_ShowaFeature.EventName);
            var expedBoss = c.CurrentCulture.GetMobName(em.GetTemplate.BossId);

            if (player.getLevel() < em.MinLevel || player.getLevel() > em.MaxLevel)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Exped_NotEligible, expedBoss));
                return;
            }

            var expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();

            if (expedition == null)
            {
                var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Exped_FormPrompt, expedName, em.Template?.GetRequirementDescription(c) ?? "", expedBoss));
                if (sel == 1)
                {
                    expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
                    if (expedition != null)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_SomeoneLeader));
                        return;
                    }

                    if (!haveItem(expedItem))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_LeaderNeedsItem, expedItem, expedBoss));
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
                    var sel = await AskMenu(list);

                    if (sel == 1)
                    {
                        var expedMembers = expedition.GetPlayerSortList();
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

                        var kickSel = await AskMenu(text);
                        if (kickSel > 0)
                        {
                            var banned = expedMembers[kickSel - 1];
                            await expedition.ban(banned.Id);
                            await SayOK(this.GetTalkMessage(ScriptMessage.Exped_Banned, banned.Name));
                        }
                    }
                    else if (sel == 2)
                    {
                        if (!haveItem(expedItem))
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Exped_LeaderNeedsItem, expedItem, expedBoss));
                            return;
                        }

                        var size = expedition.getPlayers().Count;
                        if (size < expedition.EventManager.MinCount)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Exped_NeedPlayers, expedition.EventManager.MinCount));
                            return;
                        }

                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_WillBeginNamed, expedMap));

                        await expedition.StartBattle();
                    }
                    else if (sel == 3)
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
                    if (em != null)
                    {
                        var eim = em.getInstance(expedName + player.getClient().getChannel());
                        if (eim != null && eim.getIntProperty("canJoin") == 1)
                        {
                            await eim.registerPlayer(player);
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Exped_InBattle, expedBoss));
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_CannotLoad));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Exped_AnotherExped, expedBoss));
                }
            }
        }
    }
}
