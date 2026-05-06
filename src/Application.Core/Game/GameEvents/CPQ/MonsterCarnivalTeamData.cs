namespace Application.Core.Game.GameEvents.CPQ
{
    public class MonsterCarnivalData
    {
        public MonsterCarnivalData(sbyte teamFlag)
        {
            TeamFlag = teamFlag;
        }

        public sbyte TeamFlag { get; set; } = -1;
        public int AvailableCP { get; set; }
        public int TotalCP { get; set; }
    }
    public class MonsterCarnivalTeamData
    {
        /// <summary>
        /// 红队0  蓝队1
        /// </summary>
        public sbyte TeamFlag { get; set; }

        public int AvailableCP { get; set; }
        public int TotalCP { get; set; }
        public int SummonedMonster { get; set; }

        public List<Player> EligibleMembers { get; }

        public MonsterCarnivalTeamData(sbyte team1, List<Player> members)
        {
            TeamFlag = team1;

            EligibleMembers = members;
        }
    }
}
