using server.life;

namespace client.command.commands.gm2;




public class MobSkillCommand : Command
{
    public MobSkillCommand()
    {
        setDescription("Apply a mob skill to all mobs on the map. Args: <mob skill id> <skill level>");
    }

    public override void execute(IClient client, string[] paramsValue)
    {
        if (paramsValue.Length < 2)
        {
            throw new ArgumentException("Mob skill command requires two args: mob skill id and level");
        }

        string skillId = paramsValue[0];
        string skillLevel = paramsValue[1];
        var possibleType = MobSkillTypeUtils.from(int.Parse(skillId));
        var possibleSkill = MobSkillFactory.getMobSkillOrThrow(possibleType, int.Parse(skillLevel));
        if (possibleSkill == null)
        {
            return;
        }

        var chr = client.OnlinedCharacter;
        MobSkill mobSkill = possibleSkill;
        chr.getMap().getAllMonsters().ForEach(
                monster => mobSkill.applyEffect(chr, monster, false, [])
        );
    }
}
