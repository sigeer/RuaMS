using Application.Core.Game.Skills;
using Google.Protobuf.Collections;

namespace Application.Core.Game.Players.PlayerProps
{
    public class PlayerSkill : PlayerPropBase
    {
        private Dictionary<Skill, SkillEntry> _dataSource;
        public PlayerSkill(IPlayer owner) : base(owner)
        {
            _dataSource = [];
        }
        public void LoadData(RepeatedField<Dto.SkillDto> skills)
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

        public Dto.SkillDto[] ToDto()
        {
            return _dataSource.Select(x => new Dto.SkillDto
            {
                Skillid = x.Key.getId(),
                Expiration = x.Value.expiration,
                Masterlevel = x.Value.masterlevel,
                Skilllevel = x.Value.skillevel,
            }).ToArray();
        }

        public override void LoadData(DBContext dbContext)
        {
            var skillInfoFromDB = dbContext.Skills.Where(x => x.Characterid == Owner.Id).ToList();
            foreach (var item in skillInfoFromDB)
            {
                var pSkill = SkillFactory.getSkill(item.Skillid);
                if (pSkill != null)
                {
                    _dataSource[pSkill] = new SkillEntry((sbyte)item.Skilllevel, item.Masterlevel, item.Expiration);
                }
            }

        }

        public override void SaveData(DBContext dbContext)
        {
            var characterSkills = dbContext.Skills.Where(x => x.Characterid == Owner.Id).ToList();
            foreach (var skill in _dataSource)
            {
                var dbSkill = characterSkills.FirstOrDefault(x => x.Skillid == skill.Key.getId());
                if (dbSkill == null)
                {
                    dbSkill = new SkillEntity(skill.Key.getId(), Owner.Id, skill.Value.skillevel, skill.Value.masterlevel, skill.Value.expiration);
                    dbContext.Skills.Add(dbSkill);
                }
                else
                {
                    dbSkill.Skilllevel = skill.Value.skillevel;
                    dbSkill.Masterlevel = skill.Value.masterlevel;
                    dbSkill.Expiration = skill.Value.expiration;
                }
            }
            dbContext.SaveChanges();
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
