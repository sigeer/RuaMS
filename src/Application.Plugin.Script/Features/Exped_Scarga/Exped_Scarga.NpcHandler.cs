using Application.Core.Client;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using Application.Plugin.Script;

namespace Application.Plugin.Script.Features.Exped_Scarga
{
    /// <summary>马来西亚远征队（Scarga）相关 NPC。</summary>
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, Application.Core.Game.Life.NPC? npcObject)
            : base(c, npc, npcObject)
        {
        }

        // Npc: 9270047 — 马来西亚远征队（Scarga）入口
        public async Task MalaysiaBoss_GL()
        {
            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(Exped_ScargaFeature.EventName);
            var expedBoss = c.CurrentCulture.GetMobName(em.GetTemplate.BossId);

            if (player.getLevel() < em.MinLevel || player.getLevel() > em.MaxLevel)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Exped_NotEligible, expedBoss));
                return;
            }

            var expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();

            if (expedition == null)
            {
                var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Exped_FormPrompt, "Scarga Expedition",
                    em?.Template?.GetRequirementDescription(c) ?? "", expedBoss));
                if (sel == 1)
                {
                    expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
                    if (expedition != null)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_SomeoneLeader));
                        return;
                    }

                    var r = await em.StartInstance(player);
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Exped_SureNotEveryone, expedBoss));
                }
                return;
            }

            if (expedition.isLeader(player))
            {
                if (expedition.isInProgress())
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Exped_InProgress));
                    return;
                }

                var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Exped_Menu));
                if (sel == 1)
                {
                    var members = expedition.GetPlayerSortList();
                    if (members.Count == 1)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_OnlyMember));
                        return;
                    }

                    var text = this.GetTalkMessage(ScriptMessage.Exped_MemberList);
                    text += "\r\n\t\t1." + members[0].Name;
                    for (var i = 1; i < members.Count; i++)
                    {
                        text += $"\r\n#b#L{i + 1}#{i + 1}. {members[i].Name}#l\n";
                    }

                    var kick = await AskMenu(text);
                    if (kick > 0)
                    {
                        var banned = members[kick - 1];
                        await expedition.ban(banned.Id);
                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_Banned, banned.Name));
                    }
                }
                else if (sel == 2)
                {
                    if (expedition.getPlayers().Count < expedition.EventManager.MinCount)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_NeedPlayers, expedition.EventManager.MinCount));
                        return;
                    }

                    await SayOK(this.GetTalkMessage(ScriptMessage.Scarga_GoodLuck));
                    await expedition.StartBattle();
                }
                else if (sel == 3)
                {
                    await player.getMap().LightBlue(ScriptMessageCategories.ScriptMessage, ScriptMessage.Exped_LeaderEnded, expedition.getLeader().getName());
                    await expedition.DisposeAsync();
                    await SayOK(this.GetTalkMessage(ScriptMessage.Exped_Ended));
                }
                return;
            }

            if (expedition.isRegistering())
            {
                if (expedition.contains(player))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Exped_Registered, expedition.getLeader().getName()));
                }
                else
                {
                    await SayOK(em.GetTemplate.HandleJoinInstanceResult(await em.GetTemplate.JoinMember(expedition, player), c));
                }
                return;
            }

            if (expedition.isInProgress())
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

        // Npc: 9201134 
        public async Task Malay_Warp()
        {
            var eim = GetEventInstanceTrust();
            if (!eim.isEventCleared())
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Scarga_LeaveWont)))
                {
                    await warp(551030100, 2);
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Scarga_GuysDefeated));

                if (await eim.GiveClearReward(getPlayer()) == Core.scripting.Events.Abstraction.ClaimRewardResult.Success)
                {
                    await warp(551030100, 2);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Redeem_InventoryFull));
                    await SayOK(this.GetTalkMessage(ScriptMessage.Redeem_InventoryFull));
                }
            }
        }
    }
}
