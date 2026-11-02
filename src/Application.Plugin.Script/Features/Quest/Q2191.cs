using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q2191</c>。
    /// </summary>
    internal sealed class Q2191 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 2191;

        public Q2191() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 108000502;
            ExitMap = 120000101;
            MinMap = 108000502;
            MaxMap = 108000502;
            MaxLobbys = 2;
        }

    }
}
