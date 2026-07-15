using System.Reflection;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 物品脚本服务
    /// </summary>
    public interface IScriptItemService : IPluginServiceBase
    {
        Dictionary<string, (Type ObjType, MethodInfo Method)> ItemScripts { get; }
    }
}
