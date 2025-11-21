using Application.Core.Game.Relation;
using Application.Core.Scripting.Events;
using tools;

namespace Application.Core.Game.GameEvents.CPQ
{
    public class MonsterCarnivalTeam
    {
        /// <summary>
        /// 红队0  蓝队1
        /// </summary>
        public sbyte TeamFlag { get; set; }

        public int AvailableCP { get; set; }
        public int TotalCP { get; set; }
        public int SummonedMonster { get; set; }
        public int SummonedReactor { get; set; }
        public bool IsWinner { get; private set; }
        public Team Team { get; }
        public MonsterCarnivalEventInstanceManager Event { get; }
        public MonsterCarnivalTeam? Enemy { get; set; }
        public List<IPlayer> EligibleMembers { get; }

        public MonsterCarnivalTeam(MonsterCarnivalEventInstanceManager @event, Team team, sbyte team1)
        {
            Event = @event;
            Team = team;
            EligibleMembers = Team.getEligibleMembers();

            TeamFlag = team1;
        }

        public void SetEnemy(MonsterCarnivalTeam party)
        {
            this.Enemy = party;
        }

        public void AddCP(IPlayer player, int amount)
        {
            TotalCP += amount;
            AvailableCP += amount;
            player.addCP(amount);
        }
        public void UseCP(IPlayer player, int amount)
        {
            AvailableCP -= amount;
            player.useCP(amount);
        }

        public void Summon()
        {
            this.SummonedMonster++;
        }

        public bool CanSummon()
        {
            return this.SummonedMonster < Event.EventMap.MaxMobs;
        }

        public bool CanGuardian()
        {
            var strFlag = TeamFlag.ToString();
            return Event.EventMap.getAllReactors().Count(x => x.getName().Substring(0, 1) == strFlag) < Event.EventMap.MaxReactors;
        }

        public void Complete(bool isWinner)
        {
            IsWinner = isWinner;

            ShowEffect();
        }

        void ShowEffect()
        {
            var map = Event.EventMap;
            var effect = IsWinner ? map.EffectWin : map.EffectLose;
            var sound = IsWinner ? map.SoundWin : map.SoundLose;
            foreach (var mc in EligibleMembers)
            {
                if (mc.IsOnlined)
                {
                    mc.sendPacket(PacketCreator.showEffect(effect));
                    mc.sendPacket(PacketCreator.playSound(sound));
                    mc.dispelDebuffs();
                }
            }
        }

        /// <summary>
        /// 前往结算奖励
        /// </summary>
        public void MoveToReward()
        {
            var mapFactory = Event.EventMap.getChannelServer().getMapFactory();
            var map = Event.EventMap;

            var rewardMap = mapFactory.getMap(IsWinner ? map.RewardMapWin : map.RewardMapLose);
            foreach (var mc in EligibleMembers)
            {
                if (mc.IsOnlined)
                {
                    mc.gainFestivalPoints(this.TotalCP);
                    mc.setMonsterCarnival(null);
                    mc.changeMap(rewardMap);
                    mc.setTeam(-1);
                    mc.dispelDebuffs();
                }
            }
        }

        public void Dispose()
        {
            Enemy = null;
            TotalCP = 0;
            AvailableCP = 0;

            Team.setEligibleMembers([]);
            Team.MCTeam = null;
        }
    }
}
