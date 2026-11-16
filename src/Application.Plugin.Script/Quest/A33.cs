
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{
    // 冰峰雪域 + 水下世界
    internal partial class QuestScript
    {
        // Quest: 3108 
        public async Task q3108s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A33_PeekIntoShattered));
            await forceCompleteQuest();
        }

    }
}