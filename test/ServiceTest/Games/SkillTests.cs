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
    }
}
