using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script
{
    /// <summary>
    /// Reactor.wz 中没有 <c>action</c> 字段、但 scripts/reactor 下仍有 <c>touch()</c> 实现的反应堆。
    /// 脚本名用 <c>r{ReactorId}</c> 约定。
    /// </summary>
    internal partial class ReactorTouchScript
    {
        // Reactor: 2408003 — 神木村 龙头：先出假怪物，再按地图刷出真正的龙头
        public async Task r2408003()
        {
            var eim = getEventInstance();
            if (eim != null)
            {
                eim.setProperty("summoned", "true");
                eim.setProperty("canEnter", "false");
            }

            await spawnFakeMonster(8800000);
            await mapMessage(6, "A gigantic creature is approaching from the deep cave.");

            switch (getMapId())
            {
                case 240060000:
                    await spawnMonster(8810000, 1, 960, 0);
                    break;
                case 240060100:
                    await spawnMonster(8810001, 1, 0, 0);
                    break;
            }
        }
    }
}
