using Application.Core.Client;
using Application.Core.Game.Maps;
using Application.Core.Server.maps;
using Application.Shared.Constants.Map;
using scripting.portal;

namespace Application.Plugin.Script.Features.Dojo
{
    /// <summary>
    /// 武陵道场：dojang_exit、dojang_next、dojang_tuto、dojang_up。
    /// </summary>
    internal sealed class DojoPortalHandler : PortalPlayerInteraction
    {
        public DojoPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 925020000 | pt: 7
        public async Task<bool> dojang_exit()
        {

            var map = getPlayer().getSavedLocation("MIRROR");
            if (map == -1)
            {
                map = 100000000;
            }

            await playPortalSound();
            await warp(map);
            return true;
        }
        // Map: 925020100, 925020200, 925020300, 925020400, 925020500, 925020600 … (共 39 张) | pt: 8
        public async Task<bool> dojang_next()
        {

            if (getPlayer().canClickNPC(3000))
            {
                return false;
            }
            // this script can be ran twice when passing the dojo portal... strange.
            getPlayer().setClickedNPC();

            var gate = getPlayer().getMap().getReactorByName("door");
            if (gate != null)
            {
                if (gate.getState() == 1 || getMap().countMonsters() == 0)
                {
                    if (Math.Floor(getPlayer().getMapId() / 100.0) % 100 < 38)
                    {
                        if (((Math.Floor((getPlayer().getMap().getId() + 100) / 100.0)) % 100) % 6 == 0)
                        {
                            if (Math.Floor(getPlayer().getMapId() / 10000.0) == 92503)
                            {
                                var restMapId = getPlayer().getMap().getId() + 100;
                                var mapId = getPlayer().getMap().getId();

                                for (var i = 0; i < 5; i++)
                                {
                                    var chrlist = (await getMap(mapId - 100 * i)).getAllPlayers();

                                    foreach (var chr in chrlist)
                                    {
                                        for (var j = i; j >= 0; j--)
                                        {
                                            await chr.Pink("You received " + chr.addDojoPointsByMap(mapId - 100 * j) + " training points. Your total training points score is now " + chr.getDojoPoints() + ".");
                                        }

                                        await chr.changeMap(restMapId, 0);
                                    }
                                }
                            }
                            else
                            {
                                await Pink("You received " + getPlayer().addDojoPointsByMap(getMapId()) + " training points. Your total training points score is now " + getPlayer().getDojoPoints() + ".");
                                await playPortalSound();
                                await warp(getPlayer().getMap().getId() + 100, 0);
                            }
                        }
                        else
                        {
                            await Pink("You received " + getPlayer().addDojoPointsByMap(getMapId()) + " training points. Your total training points score is now " + getPlayer().getDojoPoints() + ".");
                            await playPortalSound();
                            await warp(getPlayer().getMap().getId() + 100, 0);
                        }
                    }
                    else
                    {
                        await playPortalSound();
                        await warp(925020003, 0);
                        await getPlayer().gainExp(2000 * getPlayer().getDojoPoints(), true, true, true);
                    }
                    return true;
                }
                else
                {
                    await Pink("The door is not open yet.");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        // Map: 925020010 | pt: 7
        public async Task<bool> dojang_tuto()
        {

            if (getPlayer().getMap().getMonsterById(9300216) != null)
            {
                getPlayer().FinishedDojoTutorial = true;
                getClient().getChannelServer().resetDojo(getPlayer().getMap().getId());
                getClient().getChannelServer().dismissDojoSchedule(getPlayer().getMap().getId(), getParty());
                await playPortalSound();
                await warp(925020001, 0);
                return true;
            }
            else
            {
                await Pink("So Gong: Haha! You're going to run away like a coward? I won't let you get away that easily!");
                return false;
            }
        }
        // Map: 925020100, 925020200, 925020300, 925020400, 925020500, 925020610 … (共 33 张) | pt: 7
        public async Task<bool> dojang_up()
        {

            try
            {
                if (getPlayer().getMap().getMonsterById(9300216) != null)
                {
                    await goDojoUp();
                    await getPlayer().getMap().setReactorState();
                    var stage = (int)Math.Floor(getPlayer().getMapId() / 100.0) % 100;
                    if ((stage - (stage / 6) | 0) == getPlayer().getVanquisherStage() && !MapId.isPartyDojo(getPlayer().getMapId())) // we can also try 5 * stage / 6 | 0 + 1
                    {
                        getPlayer().setVanquisherKills(getPlayer().getVanquisherKills() + 1);
                    }
                }
                else
                {
                    await Pink("There are still some monsters remaining.");
                }
                await enableActions();
                return true;
            }
            catch (Exception err)
            {
                await Notice(err.Message);
                return false;
            }
        }
    }
}
