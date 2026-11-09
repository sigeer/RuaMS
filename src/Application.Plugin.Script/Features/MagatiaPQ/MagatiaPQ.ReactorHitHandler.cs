using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.MagatiaPQ
{
    /// <summary>
    /// 玛加提亚组队任务的反应堆 <c>hit</c> 脚本。
    /// </summary>
    internal sealed class MagatiaPQReactorHitHandler : ReactorHitScriptBase
    {
        public MagatiaPQReactorHitHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 2618000（beaker1~3）— 三只烧瓶都完成第 6 状态后开门并发放阶段奖励
        public async Task r2618000()
        {
            var eim = GetEventInstanceTrust();

            if (getReactor().getState() != 6)
                return;

            var done = eim.getIntProperty("statusStg3") + 1;
            eim.setIntProperty("statusStg3", done);

            if (done != 3)
                return;

            await eim.showClearEffect();
            await eim.GiveStageClearRewardAll(3);

            await getMap().killAllMonsters();

            // JS 按名字取 rnj2_door / jnr2_door，两者 WZ id 都是 2618006
            await getMap().getReactorById(2618006)!.hitReactor(getClient());
        }
    }
}
