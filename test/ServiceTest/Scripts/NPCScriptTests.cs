using scripting.npc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceTest.Scripts
{
    public class NPCScriptTests: TestBase
    {
        [Test]
        public void Script_2100_Test()
        {
            NPCScriptManager.getInstance().start(GenerateTestClient(), 2100, null);
            Assert.Pass();
        }
    }
}
