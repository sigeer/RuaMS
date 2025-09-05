namespace Application.Templates.Skill
{
    public sealed class MobSkillTemplate : AbstractTemplate
    {
        /// <summary>
        /// Level - Data
        /// </summary>
        private readonly Dictionary<int, MobSkillLevelData> _levelData;
        public MobSkillLevelData? GetLevelData(int nLevel) => _levelData.GetValueOrDefault(nLevel);

        public void InsertLevelData(MobSkillLevelData pData)
            => _levelData.Add(pData.nSLV, pData);

        public MobSkillTemplate(int templateId) : base(templateId)
        {
            _levelData = new Dictionary<int, MobSkillLevelData>();
        }
    }
}
