using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.HolidayPQ
{
    internal sealed class HolidayPQReactorActHandler : ReactorActScriptBase
    {
        public HolidayPQReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }
        // Reactor: 9400300 — 假日 PQ：雪人吸收真实的雪之力量
        public async Task r9400300()
        {
            await HolidayPQEventBase.FeedSnowman(GetEventInstanceTrust());
        }

        // Reactor: 9400301 — 假日 PQ：雪人吸收假的雪之力量
        public async Task r9400301()
        {
            await HolidayPQEventBase.FeedFakeSnowman(GetEventInstanceTrust());
        }
    }
}
