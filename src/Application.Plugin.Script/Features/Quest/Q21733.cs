using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q21733</c>。
    /// </summary>
    internal sealed class Q21733 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 21733;

        public Q21733() : base(QuestIdValue)
        {
            EventTime = 10 * 60;
            MaxLobbys = 1;

            EntryMap = 910400000;
            ExitMap = 104000004;
            MinMap = 910400000;
            MaxMap = 910400000;
        }

    }
}
