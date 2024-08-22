using scripting.npc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceTest.Scripts
{
    public class NPCScriptTests
    {
        [Test]
        public void Script_2100_Test()
        {
            NPCScriptManager.getInstance().start(TestFactory.GenerateTestClient(), 2100, null);
            Assert.Pass();
        }
    }
}
