using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q2291</c>。
    /// </summary>
    internal sealed class Q2291 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 2291;

        public Q2291() : base(QuestIdValue)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 103040440;
            ExitMap = 103040400;
            MinMap = 103040410;
            MaxMap = 103040460;
        }

    }
}
