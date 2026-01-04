using Application.Core.Game.Relation;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using Application.Shared.MapObjects;
using AutoMapper.Execution;
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
        public bool IsWinner { get; private set; }
        public MonsterCarnivalEventInstanceManager Event { get; }
        public MonsterCarnivalTeam? Enemy { get; set; }
        public TeamRegistry Team { get; }

        public MonsterCarnivalTeam(MonsterCarnivalEventInstanceManager @event, TeamRegistry team, sbyte team1)
        {
            Event = @event;
            Team = team;

            TeamFlag = team1;
        }

        public void Initialize(MonsterCarnivalTeam enemy)
        {
            Enemy = enemy;
            foreach (var mc in Team.EligibleMembers)
            {
                if (mc.IsOnlined)
                {
                    mc.MCTeam = this;
                    mc.resetCP();
                    mc.setTeam(TeamFlag);
                    mc.setFestivalPoints(0);
                    mc.changeMap(Event.EventMap, Event.EventMap.GetInitPortal(TeamFlag));
                    mc.sendPacket(PacketCreator.startMonsterCarnival(mc));
                    mc.LightBlue(nameof(ClientMessage.CPQ_Entry));
                }
            }
        }

        public void AddCP(Player player, int amount)
        {
            TotalCP += amount;
            AvailableCP += amount;
            player.addCP(amount);
        }
        public void UseCP(Player player, int amount)
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
            foreach (var mc in Team.EligibleMembers)
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
            var mapFactory = Event.getMapFactory();
            var map = Event.EventMap;

            var rewardMap = mapFactory.getMap(IsWinner ? map.RewardMapWin : map.RewardMapLose);
            foreach (var mc in Team.EligibleMembers)
            {
                if (mc.IsOnlined)
                {
                    mc.gainFestivalPoints(this.TotalCP);
                    mc.changeMap(rewardMap);
                    mc.setTeam(-1);
                    mc.dispelDebuffs();
                }
            }
        }
        public int GetRandomMemberId()
        {
            return Randomizer.Select(Team.EligibleMembers).Id;
        }

        public void Dispose()
        {
            Enemy = null;
            TotalCP = 0;
            AvailableCP = 0;
        }
    }
}
