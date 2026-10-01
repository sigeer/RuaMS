using Application.Shared.GameProps;

namespace Application.Shared.Battle.Skills
{
    public interface IBuffSource
    {
        BuffSourceType Type { get; }
        public int GetEncodeId();
    }

    public enum BuffSourceType
    {
        Skill,
        Item,
        MobSkill
    }
}
