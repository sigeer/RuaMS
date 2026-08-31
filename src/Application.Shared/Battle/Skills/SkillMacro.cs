namespace Application.Shared.Battle.Skills
{
    public class SkillMacro
    {
        public int Skill1 { get; set; }
        public int Skill2 { get; set; }
        public int Skill3 { get; set; }
        public string Name { get; set; }
        public int Shout { get; set; }
        public int Position { get; set; }

        public SkillMacro(int skill1, int skill2, int skill3, string? name, int shout, int position)
        {
            Skill1 = skill1;
            Skill2 = skill2;
            Skill3 = skill3;
            Name = name ?? string.Empty;
            Shout = shout;
            Position = position;
        }
    }
}
