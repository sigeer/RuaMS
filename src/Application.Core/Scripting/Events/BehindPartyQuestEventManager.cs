using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Resources.Messages;
using Application.Templates;
using tools;

namespace Application.Core.Scripting.Events
{
    /// <summary>
    /// 在启动之后才组建团队
    /// </summary>
    public class BehindPartyQuestEventManager : PartyQuestEventManager
    {
        public int RegistrationTime => GetTemplate.RegistrationTime;
        public int PrepareTime => GetTemplate.PrepareTime;
        public override AbstractBehindPartyQuestEventTemplate GetTemplate => (Template as AbstractBehindPartyQuestEventTemplate)!;
        public BehindPartyQuestEventManager(WorldChannel cserv, AbstractBehindPartyQuestEventTemplate template) : base(cserv, template)
        {
        }

        public TEim? GetOnlyEventInstanceManager<TEim>() where TEim : BehindPartyQuestEventInstanceManager => getInstance(Name) as TEim;

        public override List<Player> GetEligibleParty(Player leader)
        {
            return [leader];
        }

        public virtual JoinInstanceResult JoinMember(BehindPartyQuestEventInstanceManager eim, Player player)
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


            eim.registerPlayer(player);

            OnPlayerJoined(eim, player);
            return JoinInstanceResult.Success;
        }

        public override AbstractEventInstanceManager Setup(int level, int lobbyId)
        {
            var eim = newInstance(Name + lobbyId);
            eim.setProperty("level", level);

            OnSetup(eim, level, lobbyId);

            respawnStages(eim);

            eim.startEventTimer(RegistrationTime * 1000);

            return eim;
        }

        public override void AfterSeup(AbstractEventInstanceManager eim)
        {
            eim.InstanceStatus = InstanceStatus.Recruitment;
        }

        public override void OnPlayerRegister(AbstractEventInstanceManager eim, Player chr)
        {
            chr.SaveLocation(SavedLocationType.EVENT);
            chr.changeMap(RecruitMap);
            chr.sendPacket(PacketCreator.getClock((int)(eim.getTimeLeft() / 1000)));
        }

        public override void OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            chr.changeMap(EntryMap == MapId.NONE ? chr.MapModel.getForcedReturnId() : EntryMap, EntryPortal);
        }

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

        public virtual void OnPlayerBanned(AbstractEventInstanceManager eim, Player chr) => GetTemplate.OnPlayerBanned(eim, chr);
        public virtual void OnPlayerJoined(AbstractEventInstanceManager eim, Player chr) => GetTemplate.OnPlayerJoined(eim, chr);
        public virtual void OnBattlePrepare(AbstractEventInstanceManager eim) => GetTemplate.OnBattlePrepare(eim);
        public virtual void OnBattleStarted(AbstractEventInstanceManager eim) 
        {
            GetTemplate.OnBattleStarted(eim);
        }

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
