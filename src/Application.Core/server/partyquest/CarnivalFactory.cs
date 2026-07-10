using Application.Templates.PartyQuest;
using Application.Templates.Reader;
using server.life;

namespace server.partyquest;

/**
 * @author Drago (Dragohe4rt)
 */
public class CarnivalFactory
{

    private static CarnivalFactory instance = new CarnivalFactory();
    private Dictionary<int, MCSkill> skills = new();
    private Dictionary<int, MCSkill> guardians = new();

    private List<int> singleTargetedSkills = new();
    private List<int> multiTargetedSkills = new();

    public CarnivalFactory()
    {
        //whoosh
        initialize();
    }

    public static CarnivalFactory getInstance()
    {
        return instance;
    }

    private void initialize()
    {
        if (skills.Count != 0)
        {
            return;
        }
        var skillProvider = ProviderSource.Instance.GetProvider<IProvider<CarnivalSkillTemplate>>(ProviderType.CarnivalSkill);
        foreach (var template in skillProvider.LoadAll())
        {
            var ms = new MCSkill(template.SpendCP, template.MobSkillId, template.Level, template.TargetsAll);
            skills.AddOrUpdate(template.TemplateId, ms);
            if (ms.targetsAll)
            {
                multiTargetedSkills.Add(template.TemplateId);
            }
            else
            {
                singleTargetedSkills.Add(template.TemplateId);
            }
        }
        var guardianProvider = ProviderSource.Instance.GetProvider<IProvider<CarnivalGuardianTemplate>>(ProviderType.CarnivalGuardian);
        foreach (var template in guardianProvider.LoadAll())
        {
            guardians.AddOrUpdate(template.TemplateId, new MCSkill(template.SpendCP, template.MobSkillId, template.Level, true));
        }
    }

    private MCSkill? randomizeSkill(bool multi)
    {
        if (multi)
        {
            return skills.GetValueOrDefault(multiTargetedSkills.ElementAtOrDefault((int)(Randomizer.nextDouble() * multiTargetedSkills.Count)));
        }
        else
        {
            return skills.GetValueOrDefault(singleTargetedSkills.ElementAtOrDefault((int)(Randomizer.nextDouble() * singleTargetedSkills.Count)));
        }
    }

    public MCSkill? getSkill(int id)
    {
        var skill = skills.GetValueOrDefault(id);
        if (skill != null && skill.MobSkillId == 0)
        {
            return randomizeSkill(skill.targetsAll);
        }
        else
        {
            return skill;
        }
    }

    public MCSkill? getGuardian(int id)
    {
        return guardians.GetValueOrDefault(id);
    }

    public record MCSkill(int cpLoss, int MobSkillId, int level, bool targetsAll)
    {
        public MobSkill getSkill()
        {
            return MobSkillFactory.getMobSkillOrThrow((MobSkillType)MobSkillId, level);
        }

        public Disease? getDisease()
        {
            return Disease.getBySkill((MobSkillType)MobSkillId);
        }
    }
}
