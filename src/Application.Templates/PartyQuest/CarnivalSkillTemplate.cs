namespace Application.Templates.PartyQuest
{
    public class CarnivalSkillTemplate : AbstractTemplate
    {
        public CarnivalSkillTemplate(int templateId) : base(templateId)
        {
        }

        public int SpendCP { get; set; }
        public int MobSkillId { get; set; }
        public int Level { get; set; }
        public bool TargetsAll { get; set; }
    }

    public class CarnivalGuardianTemplate : AbstractTemplate
    {
        public CarnivalGuardianTemplate(int templateId) : base(templateId)
        {
        }

        public int SpendCP { get; set; }
        public int MobSkillId { get; set; }
        public int Level { get; set; }
    }
}