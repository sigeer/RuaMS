using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using static Application.Core.Channel.Internal.Handlers.PlayerFieldHandlers;

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

        public override JoinInstanceResult JoinMember(BehindPartyQuestEventInstanceManager eim, Player player)
        {

            int channel = eim.ChannelServer.getId();

            if (!eim.ChannelServer.NodeService.ExpeditionService.CanStartExpedition(player.getId(), channel, Name))
            {
                // thanks Conrad, Cato for noticing some expeditions have entry limit
                return JoinInstanceResult.ChannelFull;
            }

            return base.JoinMember(eim, player);
        }

        // 队伍变动不影响远征
        public override void OnPlayerLeftParty(AbstractEventInstanceManager eim, Player player)
        {

        }

        public override void OnPartyDisband(AbstractEventInstanceManager eim)
        {

        }

        public override void OnLeaderChanged(AbstractEventInstanceManager eim, Player leader)
        {

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
        public override void OnPlayerBanned(AbstractEventInstanceManager eim, Player chr)
        {
            chr.Notice("[Expedition] You have been banned from this expedition.");

            eim.Notice("[Expedition] " + chr.Name + " has been banned from the expedition.");
        }

        public override void OnPlayerJoined(AbstractEventInstanceManager eim, Player chr)
        {
            eim.LightBlue(nameof(ClientMessage.Expedition_Join), chr.Name);
        }


        public override void OnBattleStarted(AbstractEventInstanceManager eim)
        {
            eim.LightBlue(nameof(ClientMessage.Expedition_Start));
            eim.ChannelServer.NodeActor
                .Send(s =>
                {
                    s.SendDropMessage(6, "[Expedition] " + Name + " Expedition started with leader: " + eim.getLeader().getName(), true);
                });
        }

        public override void OnPlayerExit(AbstractEventInstanceManager eim, Player player)
        {
            eim.LightBlue(nameof(ClientMessage.Expedition_Left), player.Name);
            player.LightBlue(nameof(ClientMessage.Expedition_ChrLeft));

            base.OnPlayerExit(eim, player);
        }

        public override void OnTimeOut(AbstractEventInstanceManager eim)
        {
            if (eim.InstanceStatus == InstanceStatus.Recruitment)
            {
                eim.LightBlue(nameof(ClientMessage.Expedition_Timeout_Disband));
            }

            base.OnTimeOut(eim);
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
        public override void AfterSeup(AbstractEventInstanceManager eim)
        {
            base.AfterSeup(eim);

            eim.getLeader().getMap().BroadcastAll(e =>
            {
                if (e != eim.getLeader())
                    e.LightBlue(nameof(ClientMessage.Expedition_Captain_NoticeMap));
            });
            eim.getLeader()?.LightBlue(nameof(ClientMessage.Expedition_Captain_Notice));
        }
    }
}
