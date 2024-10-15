using Application.Core.Game.Skills;
using Application.Core.Game.TheWorld;
using Application.Core.Managers;
using client;
using constants.skills;
using net.server;
using server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceTest.Games
{
    internal class SkillTests: TestBase
    {
        [TestCase(2290000)]
        [Test]
        public void GetSkillStatsTest(int skillBookItemId)
        {
            var d = ItemInformationProvider.getInstance().getSkillStats(skillBookItemId, 2000);
            Assert.Pass();
        }

        [Test]
        public void Buff_ApplyMagicianGuard()
        {
            var player = GetOnlinedTestClient().OnlinedCharacter;

            SkillFactory.LoadAllSkills();
            var skill = SkillFactory.GetSkillTrust(Magician.MAGIC_GUARD);

            skill.getEffect(20).applyTo(player);
            Assert.That(player.getBuffedValue(BuffStat.SLOW) == null);
        }
    }
}
