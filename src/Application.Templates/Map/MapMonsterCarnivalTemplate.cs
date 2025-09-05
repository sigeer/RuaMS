namespace Application.Templates.Map
{
    public class MapMonsterCarnivalTemplate : MapEffectTemplateBase
    {
        public int TimeDefault { get; set; }
        public int TimeExpand { get; set; }
        public int TimeFinish { get; set; } = 10;

        public int MaxMobs { get; set; } = 20;
        public int MaxReactors { get; set; } = 16;
        public int DeathCP { get; set; }

        public int RewardMapWin { get; set; }
        public int RewardMapLose { get; set; }

        public int ReactorRed { get; set; }
        public int ReactorBlue { get; set; }

        /// <summary>
        /// guardianGenPos
        /// </summary>
        public MonsterCarnivalGuardianData[] Guardians { get; set; }
        /// <summary>
        /// skill
        /// </summary>
        public int[] Skills { get; set; }
        /// <summary>
        /// mob
        /// </summary>
        public MonsterCarnivalMobData[] Mobs { get; set; }
    }

    public sealed class MonsterCarnivalGuardianData
    {
        public int Index { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Team { get; set; } = -1;
    }

    public sealed class MonsterCarnivalSkillData
    {
        public int Index { get; set; }
        public int SkillId { get; set; }
    }

    public sealed class MonsterCarnivalMobData
    {
        public int Index { get; set; }
        public int Id { get; set; }
        public int SpendCP { get; set; }
    }
}
