using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q2192</c>。
    /// </summary>
    internal sealed class Q2192 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 2192;

        public Q2192() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 108000501;
            ExitMap = 120000101;
            MinMap = 108000501;
            MaxMap = 108000501;
            MaxLobbys = 2;
        }

    }
}
