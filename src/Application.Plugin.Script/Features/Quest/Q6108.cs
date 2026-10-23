using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q6108</c>。
    /// </summary>
    internal sealed class Q6108 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 6108;

        public Q6108() : base(QuestIdValue)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 910500000;
            ExitMap = 105090200;
            MinMap = 910500000;
            MaxMap = 910500000;
            EntryPortal = 1;
        }

    }
}
