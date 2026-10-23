using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q3933</c>。
    /// </summary>
    internal sealed class Q3933 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 3933;

        public Q3933() : base(QuestIdValue)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 926000000;
            ExitMap = 260000200;
            MinMap = 926000000;
            MaxMap = 926000000;
            EntryPortal = 1;
        }

    }
}
