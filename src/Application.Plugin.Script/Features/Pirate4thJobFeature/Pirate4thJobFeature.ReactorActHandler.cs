using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.Pirate4thJobFeature
{
    internal sealed class Pirate4thJobFeatureReactorActHandler : ReactorActScriptBase
    {
        public Pirate4thJobFeatureReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 1209000 
        public async Task ntQuest01()
        {
            // string visibility thanks to ProXAIMeRx & Glvelturall
            if (isQuestStarted(6400))
            {
                await setQuestProgress(6400, 1, 2);
                await setQuestProgress(6400, 6401, "q3");
            }

            await Pink("Real Bart has been found. Return to Jonathan through the portal.");


        }
    }
}
