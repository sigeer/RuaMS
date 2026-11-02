using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q20718</c>。
    /// </summary>
    internal sealed class Q20718 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 20718;

        public Q20718() : base(QuestIdValue)
        {
            EventTime = 10 * 60;
            MaxLobbys = 1;

            EntryMap = 910110000;
            ExitMap = 101000000;
            MinMap = 910110000;
            MaxMap = 910110000;
            EntryPortal = 1;
            ExitPortal = 2;
        }

    }
}
