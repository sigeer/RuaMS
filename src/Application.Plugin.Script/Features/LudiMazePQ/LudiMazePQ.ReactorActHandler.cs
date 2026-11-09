using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.LudiMazePQ
{
    internal sealed class LudiMazePQReactorActHandler : ReactorActScriptBase
    {
        public LudiMazePQReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 8098000 — JP PQ：按地图生成不同的怪物
        public async Task r8098000()
        {
            var map = getMapId();
            var b = Math.Abs(map - 809050005);

            if (map != 809050000 && map != 809050010 && map != 809050014)
            {
                await spawnMonster(9400217 - b, 2);
                await spawnMonster(9400218 - b, 3);
            }
            else
            {
                await spawnMonster(9400209, 6);
                await spawnMonster(9400210, 9);
            }

            await Pink("Some monsters are summoned.");
        }
    }
}
