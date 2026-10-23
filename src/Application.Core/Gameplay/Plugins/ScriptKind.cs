namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 脚本种类。与 <see cref="Services.IScriptNpcService"/> 等注册接口一一对应。
    /// </summary>
    public enum ScriptKind : byte
    {
        Npc,
        Quest,
        Item,
        Portal,
        Map,

        ReactorHit,
        ReactorAct,
        ReactorTouch,
        ReactorUntouch
    }
}
