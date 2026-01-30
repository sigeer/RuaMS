using Application.Core.Game.Skills;
using client.autoban;
using constants.game;
using server;

namespace Application.Core.Game.Gameplay
{
    public class AttackInfo
    {
        public int numAttacked, numDamage, numAttackedAndDamage, skill, skilllevel, stance, direction, rangedirection, charge, display;
        public Dictionary<int, AttackTarget?> targets = new();
        public bool ranged, magic;
        public int speed = 4;
        public Point position = new Point();
        public List<int> explodedMesos = [];
        public short attackDelay;

        public StatEffect? getAttackEffect(Player chr, Skill? theSkill)
        {
            var mySkill = theSkill ?? SkillFactory.getSkill(skill);
            if (mySkill == null)
            {
                return null;
            }

            int skillLevel = chr.getSkillLevel(mySkill);
            if (skillLevel == 0 && GameConstants.isPqSkillMap(chr.getMapId()) && GameConstants.isPqSkill(mySkill.getId()))
            {
                skillLevel = 1;
            }

            if (skillLevel == 0)
            {
                return null;
            }
            if (display > 80)
            {
                //Hmm
                if (!mySkill.getAction())
                {
                    chr.Client.CurrentServer.NodeService.AutoBanManager.Autoban(AutobanFactory.GACHA_EXP, chr, "WZ Edit; adding action to a skill: " + display);
                    return null;
                }
            }
            return mySkill.getEffect(skillLevel);
        }
    }
}
