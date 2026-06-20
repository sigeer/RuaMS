using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Resources.Messages;
using tools;

namespace Application.Core.scripting.Events.Templates
{
    public abstract class AbstractBehindPartyQuestEventTemplate : AbstractPartyQuestEventTemplate
    {
        protected AbstractBehindPartyQuestEventTemplate(string name) : base(name)
        {
            PartyLeaderRequired = false;
            MaxLobbys = 1;
        }

        public int RegistrationTime { get; init; }
        public int PrepareTime { get; init; }

        public virtual async Task<JoinInstanceResult> JoinMember(BehindPartyQuestEventInstanceManager eim, Player player)
        {
            if (eim.InstanceStatus != InstanceStatus.Recruitment)
            {
                return JoinInstanceResult.StopRecruitment;
            }

            if (eim.Banned.ContainsKey(player.getId()))
            {
                return JoinInstanceResult.Banned;
            }

            if (eim.getPlayerCount() >= MaxCount)
            {
                return JoinInstanceResult.RoomFull;
            }

            if (player.Level < MinLevel || player.Level > MaxLevel)
            {
                return JoinInstanceResult.LevelOutOfRange;
            }


            await eim.registerPlayer(player);

            await OnPlayerJoined(eim, player);
            return JoinInstanceResult.Success;
        }

        public override List<Player> GetEligibleParty(Player leader)
        {
            return [leader];
        }

        public override Task AfterSeup(AbstractEventInstanceManager eim)
        {
            eim.InstanceStatus = InstanceStatus.Recruitment;
            return Task.CompletedTask;
        }

        public override async Task OnPlayerRegister(AbstractEventInstanceManager eim, Player chr)
        {
            chr.SaveLocation(SavedLocationType.EVENT);
            await chr.changeMap(RecruitMap);
            await chr.SendPacket(PacketCreator.getClock((int)(eim.getTimeLeft() / 1000)));
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            await chr.changeMap(EntryMap == MapId.NONE ? chr.MapModel.getForcedReturnId() : EntryMap, EntryPortal);
        }

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
                    return null;
                case CreateInstanceResult.LobbyLimited:
                    return "抱歉，您已经达到了此次远征的尝试配额！请另选他日再试……";
                default:
                    return "在开始远征时发生了意外错误，请稍后重试。";
            }
        }

        public virtual string? HandleJoinInstanceResult(JoinInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case JoinInstanceResult.Success:
                    return "You have registered for the expedition successfully!";
                case JoinInstanceResult.StopRecruitment:
                    return "Sorry, this expedition is already underway. Registration is closed!";
                case JoinInstanceResult.Banned:
                    return "Sorry, you've been banned from this expedition.";
                case JoinInstanceResult.RoomFull:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Expedition_MemberFull));
                case JoinInstanceResult.ChannelFull:
                    return "Sorry, you've already reached the quota of attempts for this expedition! Try again another day...";
                case JoinInstanceResult.LevelOutOfRange:
                    return $"只有等级#r{MinLevel}~{MaxLevel}#k才能参与。";
                case JoinInstanceResult.Unknown:
                default:
                    return "unknown error";
            }
        }

        public virtual Task OnPlayerBanned(AbstractEventInstanceManager eim, Player chr) => Task.CompletedTask;
        public virtual Task OnPlayerJoined(AbstractEventInstanceManager eim, Player chr) => Task.CompletedTask;
        public virtual Task OnBattlePrepare(AbstractEventInstanceManager eim) => Task.CompletedTask;
        public virtual Task OnBattleStarted(AbstractEventInstanceManager eim) => Task.CompletedTask;

        public override bool IsEventTeamLackingNow(AbstractEventInstanceManager eim, bool leavingEventMap, Player quitter)
        {
            if (eim.InstanceStatus == InstanceStatus.Recruitment)
            {
                return leavingEventMap && quitter.Id == eim.getLeaderId();
            }
            return base.IsEventTeamLackingNow(eim, leavingEventMap, quitter);
        }
    }
}
