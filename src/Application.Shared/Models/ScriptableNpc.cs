namespace Application.Shared.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="NpcId"></param>
    /// <param name="Script">使用的脚本方法， null时使用 n{NpcId}</param>
    /// <param name="ScriptInfo">当该NPC同时存在任务时，供客户端显示</param>
    public record ScriptableNpc(int NpcId, string? Script, string ScriptInfo);
}
