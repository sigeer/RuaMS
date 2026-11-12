using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.MagatiaPQ
{
    /// <summary>
    /// 玛加提亚组队任务的反应堆 <c>act</c> 脚本。
    /// </summary>
    internal sealed class MagatiaPQReactorActHandler : ReactorActScriptBase
    {
        public MagatiaPQReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 2618001 / 2618002 — 玛加提亚 PQ 门：触发另一张地图的出口反应堆
        public Task r2618001() => OpenDoor(201, "rnj31_out", "jnr31_out", 1);

        public Task r2618002() => OpenDoor(202, "rnj32_out", "jnr32_out", 2);

        async Task OpenDoor(int mapOffset, string zenumistReactor, string alcadnoReactor, int portalId)
        {
            var eim = GetEventInstanceTrust();
            var isAlcadno = eim.getIntProperty("isAlcadno");

            var map = await eim.getMapInstance(
                (isAlcadno == 0 ? MagatiaPQFeature.ZenumistMapBase : MagatiaPQFeature.AlcadnoMapBase) + mapOffset);
            await map.getReactorByName(isAlcadno == 0 ? zenumistReactor : alcadnoReactor)!.hitReactor(getClient());
        }

        // Reactor: 2612001 ~ 2612003 — 箱子
        public async Task rnjItem0() => await dropItems();

        public async Task rnjItem1() => await dropItems();

        public async Task rnjItem2() => await dropItems();

        // Reactor: 2619001 / 2619002 — 训练塔入口的魔法装置。
        // 原始 JS 没有 2619001.js / 2619002.js，掉落由 2619000 的 hit 处理，这里保持空实现。
        public Task rnjScript0() => Task.CompletedTask;

        public Task rnjScript1() => Task.CompletedTask;
    }
}
