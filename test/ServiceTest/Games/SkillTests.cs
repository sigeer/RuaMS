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
    //internal class SkillTests: TestBase
    //{
    //    public SkillTests()
    //    {
    //        SkillFactory.LoadAllSkills();
    //    }

    //    [TestCase(2290000)]
    //    [Test]
    //    public void GetSkillStatsTest(int skillBookItemId)
    //    {
    //        var d = ItemInformationProvider.getInstance().getSkillStats(skillBookItemId, 2000);
    //        Assert.Pass();
    //    }

    //    [Test]
    //    public void Buff_ApplyMAGIC_ARMOR()
    //    {
    //        var player = MockClient.OnlinedCharacter;
    //        var skill = SkillFactory.GetSkillTrust(Magician.MAGIC_ARMOR);

    //        skill.getEffect(20).applyTo(player);
    //        Assert.That(player.getBuffedValue(BuffStat.SLOW) == null);
    //    }
    //}
}
