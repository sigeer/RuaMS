namespace Application.Templates.Skill
{
    [GenerateTag]
    public sealed class MobSkillTemplate : AbstractTemplate
    {
        /// <summary>
        /// Level - Data
        /// </summary>
        private Dictionary<int, MobSkillLevelData>? _levelData;
        public Dictionary<int, MobSkillLevelData> DicLevelData
        {
            get
            {
                if (_levelData == null)
                {
                    _levelData = LevelData.ToDictionary(x => x.nSLV);
                }
                return _levelData;
            }
        }
        public MobSkillLevelData? GetLevelData(int nLevel) => DicLevelData.GetValueOrDefault(nLevel);

        public void InsertLevelData(MobSkillLevelData pData)
            => DicLevelData.Add(pData.nSLV, pData);

        public MobSkillTemplate(int templateId) : base(templateId)
        {
            LevelData = Array.Empty<MobSkillLevelData>();
        }

        [WZPath("level/-")]
        public MobSkillLevelData[] LevelData { get; set; }
    }
}
