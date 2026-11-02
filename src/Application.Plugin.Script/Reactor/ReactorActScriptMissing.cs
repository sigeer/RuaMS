namespace Application.Plugin.Script
{
    /// <summary>
    /// 补齐 Reactor.wz 中声明、但 JS 与 C# 两侧均无实现的反应堆脚本（act 阶段）。
    /// 占位方法抛出 <see cref="NotImplementedException"/>，由 PluginManager 统一处理。
    /// </summary>
    internal partial class ReactorActScript
    {
        private Task NotImplemented() => throw new NotImplementedException();

        [ScriptName("08_Cross0")]
        public Task r_s_08_Cross0() => NotImplemented();
        [ScriptName("08_Cross1")]
        public Task r_s_08_Cross1() => NotImplemented();

        [ScriptName("08_Cross2")]
        public Task r_s_08_Cross2() => NotImplemented();

        [ScriptName("08_Snowman0")]
        public Task r_s_08_Snowman0() => NotImplemented();

        [ScriptName("08_Snowman1")]
        public Task r_s_08_Snowman1() => NotImplemented();

        [ScriptName("08_Snowman2")]
        public Task r_s_08_Snowman2() => NotImplemented();

        [ScriptName("6th_mob0")]
        public Task r_s_6th_mob0() => NotImplemented();
    }
}
