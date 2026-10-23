using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q3239</c>。
    /// </summary>
    internal sealed class Q3239 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 3239;

        public Q3239() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 922000000;
            ExitMap = 922000009;
            MinMap = 922000000;
            MaxMap = 922000000;
        }

    }
}
