using Application.Core.Client;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Core.Scripting.Events;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Exped_PinkBean
{
    /// <summary>粉豆远征相关 NPC（时间神殿 - 遗忘的黄昏）。对应 JS scripts/npc/2141000~2141002.js。</summary>
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, Application.Core.Game.Life.NPC? npcObject)
            : base(c, npc, npcObject)
        {
        }

        const int PINKBEAN_TEMPLE_MAP = 270050000;

        // Npc: 2141000 — Kirston：接受后召唤粉豆
        public async Task PinkBeen_Summon()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.PinkBean_OnlyHad)))
                return;

            var map = getMap();
            var npc = map.getNPCById(2141000);
            if (npc != null)
            {
                await map.RemoveMapObject(npc, mapChr => npc.sendDestroyData(mapChr.Client));
            }

            var reactor = map.getReactorById(2709000);
            if (reactor != null)
            {
                await reactor.hitReactor(c);
            }
        }

        // Npc: 2141001 — 遗忘神殿管理员：远征队管理
        public async Task PinkBeen_accept()
        {
            const string expedName = "Twilight of the Gods";
            var expedMap = "Twilight of Gods";

            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(Exped_PinkBeanFeature.EventName);
            var expedBoss = c.CurrentCulture.GetMobName(em.GetTemplate.BossId);

            if (player.getLevel() < em.MinLevel || player.getLevel() > em.MaxLevel)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Exped_NotEligible, expedBoss));
                return;
            }

            var expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();

            if (expedition == null)
            {
                var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Exped_FormPrompt, expedName,
                    em?.Template?.GetRequirementDescription(c) ?? "", expedBoss));
                if (sel == 1)
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
                    return;
                }

                var list = this.GetTalkMessage(ScriptMessage.Exped_Menu);
                var sel = await AskMenu(list);

                if (sel == 1)
                {
                    var expedMembers = expedition.GetPlayerSortList();
                    if (expedMembers.Count == 1)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Exped_OnlyMember));
                        return;
                    }

                    var text = this.GetTalkMessage(ScriptMessage.Exped_MemberList);
                    text += "\r\n\t\t1." + expedMembers[0].Name;
                    for (var i = 1; i < expedMembers.Count; i++)
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

        // Npc: 2141002 — 战斗结束后离开
        public async Task PinkBeen_Out()
        {
            var eim = GetEventInstanceTrust();

            if (!eim.isEventCleared())
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.PinkBean_WantGet)))
                {
                    await warp(PINKBEAN_TEMPLE_MAP);
                }
                return;
            }

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.PinkBean_PinkBean)))
                return;

            if (await eim.GiveClearReward(getPlayer()) == Core.scripting.Events.Abstraction.ClaimRewardResult.Success)
            {
                await warp(PINKBEAN_TEMPLE_MAP);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.PinkBean_CannotReceive));
            }
        }
    }
}
