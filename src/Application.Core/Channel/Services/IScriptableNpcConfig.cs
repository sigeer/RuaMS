using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Services
{
    public interface IScriptableNpcConfig
    {
        void RegisterScriptableNpc(ScriptableNpc data);
        void RemoveScriptableNpc(int npcId);
        Task FlushScriptableNpc();
    }

    public interface IScriptableNpcData
    {
        string? GetNpcScript(int npcId);
        IEnumerable<ScriptableNpc> GetAllScriptableNpcs();
    }
}
