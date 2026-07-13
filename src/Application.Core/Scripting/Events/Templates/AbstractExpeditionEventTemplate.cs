using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using System.Runtime.ConstrainedExecution;

namespace Application.Core.scripting.Events.Templates
{
    public abstract class AbstractExpeditionEventTemplate : AbstractBehindPartyQuestEventTemplate
    {
        public int BossId { get; }
        public AbstractExpeditionEventTemplate(string name, int bossId) : base(name)
        {
            BossId = bossId;
        }

        public override AbstractEventManager GenerateEventManager(WorldChannel worldChannel)
        {
            return new ExpeditionEventManager(worldChannel, this);
        }

        public override async Task<JoinInstanceResult> JoinMember(BehindPartyQuestEventInstanceManager eim, Player player)
        {

            int channel = eim.ChannelServer.getId();

            if (!eim.ChannelServer.NodeService.ExpeditionService.CanStartExpedition(player.getId(), channel, Name))
            {
                // thanks Conrad, Cato for noticing some expeditions have entry limit
                return JoinInstanceResult.ChannelFull;
            }

            return await base.JoinMember(eim, player);
        }

        // 队伍变动不影响远征
        public override Task OnPlayerLeftParty(AbstractEventInstanceManager eim, Player player)
        {
            return Task.CompletedTask;
        }

        public override Task OnPartyDisband(AbstractEventInstanceManager eim)
        {
            return Task.CompletedTask;
        }

        public override Task OnLeaderChanged(AbstractEventInstanceManager eim, Player leader)
        {
            return Task.CompletedTask;
        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    return "#r" + c.CurrentCulture.GetMobName(BossId) + " 远征#k 已经创建。\r\n\r\n再次与我交谈，查看当前队伍，或开始战斗！";
                case CreateInstanceResult.LobbyLimited:
                    return "抱歉，您已经达到了此次远征的尝试配额！请另选他日再试……";
                default:
                    return "在开始远征时发生了意外错误，请稍后重试。";
            }
        }

        protected override Task End(AbstractEventInstanceManager eim, TerminationReason reason)
        {
            if (reason == TerminationReason.MemberCount)
            {
                eim.Pink(nameof(ClientMessage.Expedition_MemberCountChanged_Abort));
            }
            return base.End(eim, reason);
        }

        public override async Task OnPlayerBanned(AbstractEventInstanceManager eim, Player chr)
        {
            await chr.Notice("[Expedition] You have been banned from this expedition.");

            await eim.Notice("[Expedition] " + chr.Name + " has been banned from the expedition.");

            await base.OnPlayerBanned(eim, chr);
        }

        public override async Task OnPlayerJoined(AbstractEventInstanceManager eim, Player chr)
        {
            await eim.LightBlue(nameof(ClientMessage.Expedition_Join), chr.Name);
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            await eim.LightBlue(nameof(ClientMessage.Expedition_EnterMap), chr.Name);
            await base.OnPlayerEntry(eim, chr);
        }


        public override async Task OnBattleStarted(AbstractEventInstanceManager eim)
        {
            await eim.LightBlue(nameof(ClientMessage.Expedition_Start));
            await eim.ChannelServer.NodeActor
                .Send(s =>
                {
                    s.SendDropMessage(6, "[Expedition] " + Name + " Expedition started with leader: " + eim.getLeader().getName(), true);
                });
            await base.OnBattleStarted(eim);
        }

        public override async Task OnPlayerExit(AbstractEventInstanceManager eim, Player player)
        {
            await eim.LightBlue(nameof(ClientMessage.Expedition_Left), player.Name);
            await player.LightBlue(nameof(ClientMessage.Expedition_ChrLeft));

            await base.OnPlayerExit(eim, player);
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            if (eim.InstanceStatus == InstanceStatus.Recruitment)
            {
                await eim.LightBlue(nameof(ClientMessage.Expedition_Timeout_Disband));
            }

            await base.OnTimeOut(eim);
        }

        public override bool IsEventTeamLackingNow(AbstractEventInstanceManager eim, bool leavingEventMap, Player quitter)
        {
            if (eim.InstanceStatus == InstanceStatus.Recruitment)
            {
                return leavingEventMap && quitter.Id == eim.getLeaderId();
            }
            else
            {
                return leavingEventMap && eim.getPlayerCount() <= 1;
            }
        }
        public override async Task AfterSeup(AbstractEventInstanceManager eim)
        {
            await base.AfterSeup(eim);

            await eim.getLeader().getMap().BroadcastAll(async e =>
            {
                if (e != eim.getLeader())
                    await e.LightBlue(nameof(ClientMessage.Expedition_Captain_NoticeMap));
            });
            await eim.getLeader().LightBlue(nameof(ClientMessage.Expedition_Captain_Notice));
        }
    }
}
