using Application.Core.Client;
using Application.Core.scripting.quest;
using scripting.quest;

namespace Application.Plugin.Script.Quest
{
    /// <summary>
    /// 根据 area 拆分, Etc/QuestCategory
    /// </summary>
    internal partial class QuestScript : QuestScriptBase
    {
        public QuestScript(IChannelClient c, server.quest.Quest quest, int npc) : base(c, quest, npc)
        {
        }
    }
}
