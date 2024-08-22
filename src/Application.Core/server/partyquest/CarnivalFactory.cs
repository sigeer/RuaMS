

using client;
using provider;
using provider.wz;
using server.life;
using tools;

namespace server.partyquest;

/**
 * @author Drago (Dragohe4rt)
 */
public class CarnivalFactory
{

    private static CarnivalFactory instance = new CarnivalFactory();
    private Dictionary<int, MCSkill> skills = new();
    private Dictionary<int, MCSkill> guardians = new();
    private DataProvider dataRoot = DataProviderFactory.getDataProvider(WZFiles.SKILL);

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
        foreach (Data z in dataRoot.getData("MCSkill.img"))
        {
            int id = int.Parse(z.getName());
            int spendCp = DataTool.getInt("spendCP", z, 0);
            int mobSkillId = DataTool.getInt("mobSkillID", z, 0);
            MobSkillType? mobSkillType = null;
            if (mobSkillId != 0)
            {
                mobSkillType = MobSkillTypeUtils.from(mobSkillId);
            }
            int level = DataTool.getInt("level", z, 0);
            bool isMultiTarget = DataTool.getInt("target", z, 1) > 1;
            MCSkill ms = new MCSkill(spendCp, mobSkillType, level, isMultiTarget);

            skills.AddOrUpdate(id, ms);
            if (ms.targetsAll)
            {
                multiTargetedSkills.Add(id);
            }
            else
            {
                singleTargetedSkills.Add(id);
            }
        }
        foreach (Data z in dataRoot.getData("MCGuardian.img"))
        {
            int spendCp = DataTool.getInt("spendCP", z, 0);
            int mobSkillId = DataTool.getInt("mobSkillID", z, 0);
            MobSkillType mobSkillType = MobSkillTypeUtils.from(mobSkillId);
            int level = DataTool.getInt("level", z, 0);
            guardians.AddOrUpdate(int.Parse(z.getName()), new MCSkill(spendCp, mobSkillType, level, true));
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
        if (skill != null && skill.mobSkillType == null)
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

    public record MCSkill(int cpLoss, MobSkillType? mobSkillType, int level, bool targetsAll)
    {
        public MobSkill? getSkill()
        {
            return mobSkillType == null ? null : MobSkillFactory.getMobSkillOrThrow(mobSkillType.Value, level);
        }

        public Disease? getDisease()
        {
            return Disease.getBySkill(mobSkillType);
        }
    }
}
