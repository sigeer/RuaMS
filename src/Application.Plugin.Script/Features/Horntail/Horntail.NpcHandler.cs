using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Core.Scripting.Events;
using Application.Shared.GameProps;
using Application.Utility.Configs;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Horntail
{
    /// <summary>
    /// 火焰龙相关 NPC：洞穴入口（Keroben）、试炼场报名（Mark of the Squad）、
    /// 远征报名（Mark of the Squad / Expedition）以及出口水晶。
    /// </summary>
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 2081005 — Keroben (Horntail Cave Entrance)
        public async Task hontale_keroben()
        {
            var isTransformed = getPlayer().getBuffSource(BuffStat.MORPH) == 2210003;
            if (!isTransformed && !haveItem(4001086))
            {
                await SayOK(GetDefault0());
                return;
            }

            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Horntail_WelcomeCave));

            if (sel == 1)
            {
                if (getMeso() < 100000)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_SorryDont));
                    return;
                }

                if (!canHold(2000005, 10))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_SorryDontHave));
                    return;
                }

                await gainMeso(-100000);
                await gainItem(2000005, 10);
                await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_ThankBuying));
            }
            else if (sel == 2)
            {
                if (getLevel() > 99)
                {
                    await warp(240050000, 0);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_ImSorry));
                }
            }
        }


        // Npc: 2083000 — The Encrypted Slate
        public async Task hontale_enterToE()
        {
            if (haveItem(4001086))
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Horntail_WantAccess)))
                    await warp(240050400);
            }
            else if (YamlConfig.config.server.USE_ENABLE_SOLO_EXPEDITIONS && canBypassHTPQ())
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Horntail_WantAccess)))
                    await warp(240050400);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_DontHave));
            }
        }

        private bool canBypassHTPQ()
        {
            return haveItem(4001083) && haveItem(4001084) && haveItem(4001085);
        }


        // Npc: 2083001 — Mark of the Squad (Horntail PQ)
        public async Task hontale_enter1()
        {
            if (getMapId() == 240050000)
            {
                var em = GetEventManager(HorntailFeature.PartyQuestEventName);
                if (em == null)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_HorntailPq));
                    return;
                }

                if (isUsingOldPqNpcStyle())
                {
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                    return;
                }

                var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Horntail_PartyQuest) + em.Template.GetRequirementDescription(c) + this.GetTalkMessage(ScriptMessage.Horntail_PathHorntails));

                if (sel == 0)
                {
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_PartyQuestHorntails));
                }
            }
            else
            {
                await hontale_enter1_inquest();
            }
        }

        private async Task hontale_enter1_inquest()
        {
            if (!isEventLeader())
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_OnlyParty));
                return;
            }

            var mapId = getMapId();
            if (mapId == 240050100)
            {
                if (haveItem(4001087) && haveItem(4001088) && haveItem(4001089) && haveItem(4001090) && haveItem(4001091))
                {
                    await gainItem(4001087, -1);
                    await gainItem(4001088, -1);
                    await gainItem(4001089, -1);
                    await gainItem(4001090, -1);
                    await gainItem(4001091, -1);

                    await getEventInstance()!.warpEventTeam(240050200);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_DontHaveAll));
                }
            }
            else if (mapId == 240050300 || mapId == 240050310)
            {
                if (haveItem(4001092, 1) && haveItem(4001093, 6))
                {
                    await gainItem(4001092, -1);
                    await gainItem(4001093, -6);
                    await getEventInstance()!.clearPQ();
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_CheckHave) + (mapId == 240050300 ? "" : this.GetTalkMessage(ScriptMessage.Horntail_All)) + this.GetTalkMessage(ScriptMessage.Horntail_RedKeys));
                }
            }
        }


        // Npc: 2083002 — Crystal of Roots (Horntail Exit)
        public async Task hontale_out()
        {
            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Ticket_ConfirmLeave)))
                return;

            if (getMapId() > 240050400)
                await warp(240050600);
            else
                await warp(240040700, "out00");
        }


        // Npc: 2083004 — Mark of the Squad (Horntail Expedition)
        public async Task hontale_accept()
        {
            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(HorntailFeature.ExpeditionEventName);
            var expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
            var expedBoss = c.CurrentCulture.GetMobName(em.GetTemplate.BossId);

            if (player.getLevel() < em.MinLevel || player.getLevel() > em.MaxLevel)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_MeetCriteria, expedBoss) + expedBoss + this.GetTalkMessage(ScriptMessage.Horntail_Text, expedBoss));
                return;
            }

            if (expedition == null)
            {
                var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Horntail_ExpeditionHorntail) + (em.Template.GetRequirementDescription(c) ?? "") + this.GetTalkMessage(ScriptMessage.Horntail_LikeAssemble) + expedBoss + this.GetTalkMessage(ScriptMessage.Horntail_LetsGet));

                if (sel == 1)
                {
                    expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
                    if (expedition != null)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_SomeoneAlready));
                        return;
                    }

                    var r = await em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_SureEveryones, expedBoss));
                }
            }
            else if (expedition.isLeader(player))
            {
                if (expedition.isInProgress())
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_ExpeditionAlreadyProgress));
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
                            await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_OnlyMember));
                            return;
                        }

                        var text = this.GetTalkMessage(ScriptMessage.Horntail_FollowingMembers);
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
                            await SayOK(this.GetTalkMessage(ScriptMessage.Exped_Banned, banned.Name));
                        }
                    }
                    else if (sel == 2)
                    {
                        var size = expedition.getPlayers().Count;
                        if (size < expedition.EventManager.MinCount)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_NeedLeast, expedition.EventManager.MinCount) + expedition.EventManager.MinCount + this.GetTalkMessage(ScriptMessage.Horntail_PlayersRegistered));
                            return;
                        }

                        await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_GoodLuck));

                        await expedition.StartBattle();
                    }
                    else if (sel == 3)
                    {
                        await player.getMap().LightBlue(chr => expedition.getLeader().getName() + chr.GetTalkMessage(ScriptMessage.Zakum_ExpeditionOver));
                        await expedition.DisposeAsync();
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_ExpeditionEnded));
                    }
                }
            }
            else if (expedition.isRegistering())
            {
                if (expedition.contains(player))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_HaveAlready, expedition.getLeader().getName()) + expedition.getLeader().getName() + this.GetTalkMessage(ScriptMessage.Horntail_Begin));
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
                        await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_ExpeditionAlready, expedBoss) + expedBoss + this.GetTalkMessage(ScriptMessage.Horntail_LetsPray));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Horntail_AnotherExpedition, expedBoss) + expedBoss + this.GetTalkMessage(ScriptMessage.Horntail_LetsPrayBrave));
                }
            }
        }
    }
}
