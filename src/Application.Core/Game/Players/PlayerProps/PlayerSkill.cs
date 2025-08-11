using Application.Core.Game.Skills;
using Google.Protobuf.Collections;

namespace Application.Core.Game.Players.PlayerProps
{
    public class PlayerSkill : PlayerPropBase<Dto.SkillDto>
    {
        private Dictionary<Skill, SkillEntry> _dataSource;
        public PlayerSkill(IPlayer owner) : base(owner)
        {
            _dataSource = [];
        }
        public override void LoadData(RepeatedField<Dto.SkillDto> skills)
        {
            foreach (var item in skills)
            {
                var pSkill = SkillFactory.getSkill(item.Skillid);
                if (pSkill != null)
                {
                    _dataSource[pSkill] = new SkillEntry((sbyte)item.Skilllevel, item.Masterlevel, item.Expiration);
                }
            }

        }

        public override Dto.SkillDto[] ToDto()
        {
            return _dataSource.Select(x => new Dto.SkillDto
            {
                Skillid = x.Key.getId(),
                Expiration = x.Value.expiration,
                Masterlevel = x.Value.masterlevel,
                Skilllevel = x.Value.skillevel,
            }).ToArray();
        }

        public SkillEntry? GetSkill(Skill skill) => _dataSource.GetValueOrDefault(skill);
        public void AddOrUpdate(Skill key, SkillEntry value)
        {
            _dataSource[key] = value;
        }

        public void Remove(Skill key)
        {
            _dataSource.Remove(key);
        }

        internal Dictionary<Skill, SkillEntry> GetDataSource()
        {
            return _dataSource;
        }
    }
}
