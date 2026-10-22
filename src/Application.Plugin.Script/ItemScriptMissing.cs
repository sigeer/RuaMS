using Application.Core.Client;
using Application.Core.scripting.item;
using client.inventory;

namespace Application.Plugin.Script
{
    /// <summary>
    /// 补齐 WZ 中声明、但 JS 与 C# 两侧均无实现的道具脚本。
    /// 占位方法抛出 <see cref="NotImplementedException"/>，由 PluginManager 统一处理。
    /// </summary>
    internal class ItemScriptMissing : ItemScriptBase
{
        public ItemScriptMissing(IChannelClient c, Item item, int npcId) : base(c, item, npcId)
        {
        }

        private Task NotImplemented() => throw new NotImplementedException();

        [ScriptName("KeySettingByItem")]
        public Task i_KeySettingByItem() => NotImplemented();

        [ScriptName("blackBag")]
        public Task i_blackBag() => NotImplemented();

        [ScriptName("checkMiroByItem")]
        public Task i_checkMiroByItem() => NotImplemented();

        [ScriptName("compassMake")]
        public Task i_compassMake() => NotImplemented();

        [ScriptName("compassUse")]
        public Task i_compassUse() => NotImplemented();

        [ScriptName("compassUse_cash")]
        public Task i_compassUse_cash() => NotImplemented();

        [ScriptName("firstSignus")]
        public Task i_firstSignus() => NotImplemented();

        [ScriptName("icebox")]
        public Task i_icebox() => NotImplemented();

        [ScriptName("item_2430013")]
        public Task i_item_2430013() => NotImplemented();

        [ScriptName("lastSignus")]
        public Task i_lastSignus() => NotImplemented();

        [ScriptName("openTreasure")]
        public Task i_openTreasure() => NotImplemented();

        [ScriptName("perfumeUse")]
        public Task i_perfumeUse() => NotImplemented();

        [ScriptName("q7222_Start")]
        public Task i_q7222_Start() => NotImplemented();

        [ScriptName("seSignus")]
        public Task i_seSignus() => NotImplemented();

        [ScriptName("signus")]
        public Task i_signus() => NotImplemented();

        [ScriptName("snapShot")]
        public Task i_snapShot() => NotImplemented();

        [ScriptName("summonEventNpc")]
        public Task i_summonEventNpc() => NotImplemented();

        [ScriptName("vanishEventNpc")]
        public Task i_vanishEventNpc() => NotImplemented();

        [ScriptName("yellowDayBox1")]
        public Task i_yellowDayBox1() => NotImplemented();

        [ScriptName("yellowDayBox2")]
        public Task i_yellowDayBox2() => NotImplemented();

        [ScriptName("yellowDayBox3")]
        public Task i_yellowDayBox3() => NotImplemented();

        [ScriptName("yellowDayBox4")]
        public Task i_yellowDayBox4() => NotImplemented();
    }
}
