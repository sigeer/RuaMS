using Application.Shared.Constants.Job;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceTest.Games.Gameplay
{
    internal class JobTests
    {
        [Test]
        public void TestJobBranch()
        {
            Assert.That(Job.IL_WIZARD.IsSameJobGroup(Job.MAGICIAN));
            Assert.That(!Job.IL_WIZARD.IsSameJobGroup(Job.IL_MAGE));
            Assert.That(Job.IL_MAGE.IsSameJobGroup(Job.IL_WIZARD));
        }
    }
}
