using Application.Core.Client;
using Application.Core.Game.Maps;
using scripting.map;

namespace Application.Plugin.Script
{
    internal class MapFirstEnterScript : MapScriptMethods
    {
        public MapFirstEnterScript(IChannelClient c, IMap map) : base(c, map)
        {
        }

        public Task dojang_1st()
        {
            getPlayer().resetEnteredScript();
            var stage = (int)Math.Floor(getMapId() / 100.0) % 100;
            var callBoss = false;

            if (stage % 6 == 0)
            {
                getClient().getChannelServer().dismissDojoSchedule(getMapId(), getParty());
                getClient().getChannelServer().setDojoProgress(getMapId());
            }
            else
            {
                callBoss = getClient().getChannelServer().setDojoProgress(getMapId());

                var realstage = stage - ((stage / 6) | 0);
                var mob = getMonsterLifeFactory(9300183 + realstage);
                if (callBoss && mob != null && getPlayer().getMap().getMonsterById(9300216) == null)
                {
                    mob.setBoss(false);
                    getPlayer().getMap().spawnDojoMonster(mob);
                }
            }
            return Task.CompletedTask;
        }

        public Task spaceGaGa_sMap()
        {
            getPlayer().resetEnteredScript();
            spawnMonster(9300331, -28, 0);
            return Task.CompletedTask;
        }

    }
}
