using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.LatanicaBattle
{
    internal sealed class LatanicaBattleReactorActHandler : ReactorActScriptBase
    {
        public LatanicaBattleReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 5411000 
        public async Task sgboss0()
        {
            await changeMusic("Bgm09/TimeAttack");
            await spawnMonster(9420513, -146, 225);
            GetEventInstanceTrust().setIntProperty("boss", 1);
            await Pink("As you wish, here comes Capt Latanica.");
        }
    }
}
