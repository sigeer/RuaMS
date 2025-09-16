namespace Application.Templates.Map
{
    public class MapMonsterCarnivalTemplate 
    {
        public string? EffectWin { get; set; }
        public string? EffectLose { get; set; }
        public string? SoundWin { get; set; }
        public string? SoundLose { get; set; }

        public int TimeDefault { get; set; }
        public int TimeExpand { get; set; }
        public int TimeFinish { get; set; } = 10;
        [WZPath("monsterCarnival/mobGenMax")]
        public int MaxMobs { get; set; } = 20;
        [WZPath("monsterCarnival/guardianGenMax")]
        public int MaxReactors { get; set; } = 16;
        public int DeathCP { get; set; }

        public int RewardMapWin { get; set; }
        public int RewardMapLose { get; set; }

        public int ReactorRed { get; set; }
        public int ReactorBlue { get; set; }

        /// <summary>
        /// guardianGenPos
        /// </summary>
        [WZPath("monsterCarnival/guardianGenPos/-")]
        public MonsterCarnivalGuardianData[] Guardians { get; set; }
        /// <summary>
        /// skill
        /// </summary>
        [WZPath("monsterCarnival/skill/-")]
        public int[] Skills { get; set; }
        /// <summary>
        /// mob
        /// </summary>
        [WZPath("monsterCarnival/mob/-")]
        public MonsterCarnivalMobData[] Mobs { get; set; }

        public MapMonsterCarnivalTemplate()
        {
            Guardians = Array.Empty<MonsterCarnivalGuardianData>();
            Skills = Array.Empty<int>();
            Mobs = Array.Empty<MonsterCarnivalMobData>();
        }
    }

    public sealed class MonsterCarnivalGuardianData
    {
        [WZPath("monsterCarnival/guardianGenPos/-/$name")]
        public int Index { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Team { get; set; } = -1;
    }


    public sealed class MonsterCarnivalMobData
    {
        [WZPath("monsterCarnival/mob/-/$name")]
        public int Index { get; set; }
        public int Id { get; set; }
        public int SpendCP { get; set; }
    }
}
