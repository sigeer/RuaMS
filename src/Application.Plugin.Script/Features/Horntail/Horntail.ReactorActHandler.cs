using Application.Core.Channel.DataProviders;
using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;
using System.Drawing;

namespace Application.Plugin.Script.Features.Horntail
{
    internal sealed class HorntailReactorActHandler : ReactorActScriptBase
    {
        public HorntailReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        // Reactor: 2408002 PQ
        public async Task r2408002()
        {
            var eim = GetEventInstanceTrust();
            var vvpMap = getMapId();
            var vvpOrig = 4001088;
            var vvpStage = -1;

            await eim.showClearEffect(false, vvpMap);
            await mapMessage(6, "The key has been teleported somewhere...");

            int vvpKey;
            switch (vvpMap)
            {
                case 240050101:
                    vvpKey = vvpOrig;
                    vvpStage = 1;
                    break;
                case 240050102:
                    vvpKey = vvpOrig + 1;
                    vvpStage = 2;
                    break;
                case 240050103:
                    vvpKey = vvpOrig + 2;
                    vvpStage = 3;
                    break;
                case 240050104:
                    vvpKey = vvpOrig + 3;
                    vvpStage = 4;
                    break;
                default:
                    vvpKey = -1;
                    break;
            }

            eim.setIntProperty(vvpStage + "stageclear", 1);

            var womanfred = await eim.getMapInstance(240050100);
            var theWomanfred = womanfred.getReactorByName("keyDrop" + vvpStage);
            var dropper = eim.getPlayers().FirstOrDefault();
            if (dropper == null || theWomanfred == null)
                return;

            var key = ItemInformationProvider.getInstance().GenerateVirtualItemById(vvpKey, 1);
            await womanfred.spawnItemDrop(theWomanfred, dropper, key, theWomanfred.getPosition(), true, true);
            await eim.dropMessage(6, "A bright flash of light, then a key suddenly appears somewhere in the map.");
        }

        // Reactor: 2401000 
        public async Task hontaleBoss()
        {
            await changeMusic("Bgm14/HonTale");
            if (getMap().getMonsterById(8810026) == null)
            {
                await getMap().spawnHorntailOnGroundBelow(new Point(71, 260));

                var eim = GetEventInstanceTrust();
                await eim.restartEventTimer(60 * 60000);
            }
            await mapMessage(6, "From the depths of his cave, here comes Horntail!");
        }

        // Reactor: 2402002 
        public async Task hontaleItem0()
        {
            // TODO
            // 2402002



        }


        // Reactor: 2402003 
        public async Task hontaleItem1()
        {
            // TODO
            // 2402003



        }


        // Reactor: 2402004 
        public async Task hontaleItem2()
        {
            // TODO
            // 2402004



        }


        // Reactor: 2402005 
        public async Task hontaleItem3()
        {
            // TODO
            // 2402005



        }


        // Reactor: 2402006 
        public async Task hontaleItem4()
        {
            await dropItems();


        }
    }
}
