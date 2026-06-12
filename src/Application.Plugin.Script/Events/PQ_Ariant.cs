using Application.Core.Channel;
using Application.Core.Game.Players;
using Application.Core.model;
using Application.Core.scripting.Events;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Scripting.Events;
using Application.Shared.Constants.Item;
using Application.Shared.Constants.Map;
using tools;

namespace Application.Plugin.Script.Events
{
    internal class PQ_Ariant : AbstractBehindPartyQuestEventTemplate
    {
        public PQ_Ariant(string name, int recruitMap) : base(name)
        {
            MinCount = 2;
            MaxCount = 7;

            MinLevel = 20;
            MaxLevel = 30;

            EventTime = 10 * 60;
            RegistrationTime = 5 * 60;

            RecruitMap = recruitMap;

            EntryMap = recruitMap + 1;
            ExitMap = 980010020;

            MinMap = recruitMap;
            MaxMap = recruitMap + 1;
            IncludedMap = [MapId.ARPQ_KINGS_ROOM];
        }

        public override AbstractEventManager GenerateEventManager(WorldChannel worldChannel)
        {
            return new AriantEventManager(worldChannel, this);
        }

        public override void OnBattleStarted(AbstractEventInstanceManager eim)
        {
            var scoreData = eim.getPlayers().ToDictionary(x => x, x => 0);
            foreach (Player mc in eim.getPlayers())
            {
                mc.sendPacket(PacketCreator.updateAriantPQRanking(scoreData));
            }
            respawnStages(eim);
        }

        protected override void respawnStages(AbstractEventInstanceManager eim)
        {
            if (eim.InstanceStatus == Core.scripting.Events.Abstraction.InstanceStatus.InProgress)
            {
                var pEim = (eim as AriantEventInstanceManager)!;

                pEim.broadcastAriantScoreUpdate();

                eim.Schedule(respawnStages, 1000);

                var leftTime = eim.getTimeLeft();
                if (leftTime == 10)
                {
                    ClearPQ(eim);
                }
            }
        }

        public override void OnPlayerUnregister(AbstractEventInstanceManager eim, Player player)
        {
            int shards = player.countItem(ItemId.ARPQ_SPIRIT_JEWEL);
            player.GainItem(ItemId.ARPQ_SPIRIT_JEWEL, (short)-shards, show: GainItemShow.ShowInChat);
            player.updateAriantScore(shards);

            base.OnPlayerUnregister(eim, player);
        }


        public override bool OnPlayerRevive(AbstractEventInstanceManager eim, Player player)
        {
            return true;
        }

        public override bool IsEventTeamLackingNow(AbstractEventInstanceManager eim, bool leavingEventMap, Player quitter)
        {
            if (eim.InstanceStatus == InstanceStatus.Recruitment)
            {
                return quitter.Id == eim.getLeaderId();
            }
            return eim.getPlayerCount() <= (eim.InstanceStatus == InstanceStatus.Cleared ? 1 : MinCount);
        }


        public override void OnTimeOut(AbstractEventInstanceManager eim)
        {
            if (eim.InstanceStatus == InstanceStatus.Recruitment)
            {
                eim.Dispose();
                return;
            }

            eim.warpEventTeam(MapId.ARPQ_KINGS_ROOM);
        }


        public override void ClearPQ(AbstractEventInstanceManager eim)
        {
            eim.setEventCleared();

            var pEim = (eim as AriantEventInstanceManager)!;

            var map = eim.getMapInstance(EntryMap);
            map.broadcastMessage(PacketCreator.showAriantScoreBoard());
            map.killAllMonsters();
            map.allowSummonState(false);

            pEim.distributeAriantPoints();
        }

        public override ClaimRewardResult GiveClearReward(AbstractEventInstanceManager eim, Player player, int point)
        {
            player.AriantPoints += point;
            player.Pink($"竞技点数(+{point})");

            return base.GiveClearReward(eim, player, point);
        }

        public override RewardOptions GetAllClearRewardOptions(Player chr, int point = 1)
        {
            return new RewardOptions(FinalExpRate: point * 92 * chr.ActualExpRate);
        }
    }
}
