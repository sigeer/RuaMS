using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q21301</c>。
    /// </summary>
    internal sealed class Q21301 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 21301;

        public Q21301() : base(QuestIdValue)
        {
            EventTime = 10 * 60;
            MaxLobbys = 1;

            EntryMap = 108010700;
            ExitMap = 140020200;
            MinMap = 108010700;
            MaxMap = 108010700;
            EntryPortal = 1;
        }

    }
}
