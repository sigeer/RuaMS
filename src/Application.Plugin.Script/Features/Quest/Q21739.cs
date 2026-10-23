using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q21739</c>。
    /// </summary>
    internal sealed class Q21739 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 21739;

        public Q21739() : base(QuestIdValue)
        {
            EventTime = 10 * 60;
            MaxLobbys = 1;

            EntryMap = 920030000;
            ExitMap = 200060000;
            MinMap = 920030000;
            MaxMap = 920030001;
            EntryPortal = 2;
        }

    }
}
