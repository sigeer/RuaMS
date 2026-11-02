using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q543000</c>。
    /// </summary>
    internal sealed class Q3rdJobThief : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 543000;

        public Q3rdJobThief() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 108010400;
            ExitMap = 107000402;
            MinMap = 108010400;
            MaxMap = 108010401;
        }
    }
}
