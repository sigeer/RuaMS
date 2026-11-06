using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script
{
    /// <summary>
    /// Reactor.wz 中没有 <c>action</c> 字段、但 scripts/reactor 下仍有 <c>hit()</c> 实现的反应堆。
    /// 脚本名用 <c>r{ReactorId}</c> 约定。
    /// </summary>
    internal partial class ReactorHitScript
    {
        // Reactor: 2008007 — 奥比斯 PQ 监狱机关：移动对应编号的陷阱
        public async Task r2008007()
        {
            // 机关名形如 lever1 / stone6，第 6 个字符即编号；
            // 抽奖地图里也有直接叫 "1"~"5" 的同类反应堆，取末位字符兜底。
            var name = getReactor().getName();
            var index = name.Length > 5 ? name[5] : name[^1];
            await getMap().moveEnvironment("trap" + index, 1);
        }

        // Reactor: 2618000 — 玛加提亚 PQ 烧瓶：三只烧瓶都完成后开门并发放阶段奖励
        public async Task r2618000()
        {
            if (getReactor().getState() != 6)
                return;

            var eim = GetEventInstanceTrust();

            var done = eim.getIntProperty("statusStg3") + 1;
            eim.setIntProperty("statusStg3", done);

            if (done != 3)
                return;

            await eim.showClearEffect();
            await eim.GiveStageClearRewardAll(getMapId());
            await getMap().killAllMonsters();

            var reactName = eim.getIntProperty("isAlcadno") == 0 ? "rnj2_door" : "jnr2_door";
            await getMap().getReactorByName(reactName)!.hitReactor(getClient());
        }
    }
}
