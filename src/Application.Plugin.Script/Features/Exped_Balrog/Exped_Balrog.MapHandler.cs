using Application.Core.Client;
using Application.Core.Game.Maps;
using scripting.map;

namespace Application.Plugin.Script.Features.Exped_Balrog
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class Exped_BalrogMapHandler : MapScriptMethods
    {
        public Exped_BalrogMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 105100300, 105100400
        public async Task balog_buff()
        {
            // TODO

        }

        // Map: 105100301, 105100401
        public async Task balog_dateSet()
        {
            // TODO

        }

        // Map: 105100100
        public async Task outCase()
        {
            // TODO

        }

        // Map: 105100300
        public async Task balog_summon()
        {
            // TODO

        }

        // Map: 105100400
        public async Task easy_balog_summon()
        {
            var eim = getMap().EventInstanceManager;
            if (eim == null || eim.EventName != Exped_BalrogFeature.EasyEventName)
            {
                return;
            }

            await Exped_BalrogFeature.SummonEasy(eim, getMap());
        }

        // Map: 105100301, 105100401
        public async Task balog_bonusSetting()
        {
            // TODO

        }

    }
}
