using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q3335</c>。
    /// </summary>
    internal sealed class Q3335 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 3335;

        public Q3335() : base(QuestIdValue)
        {
            EventTime = 10 * 60;
            MaxLobbys = 1;

            EntryMap = 926120300;
            ExitMap = 261000000;
            MinMap = 926120300;
            MaxMap = 926120300;
            EntryPortal = 4;
        }

    }
}
