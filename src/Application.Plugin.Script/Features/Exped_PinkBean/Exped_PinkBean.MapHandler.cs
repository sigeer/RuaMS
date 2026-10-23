using Application.Core.Client;
using Application.Core.Game.Maps;
using scripting.map;

namespace Application.Plugin.Script.Features.Exped_PinkBean
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class Exped_PinkBeanMapHandler : MapScriptMethods
    {
        public Exped_PinkBeanMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 270050100
        public async Task PinkBeen_before()
        {
            // TODO

        }

    }
}
