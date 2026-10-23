using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q542000</c>。
    /// </summary>
    internal sealed class Q2ndJobThief : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 542000;

        public Q2ndJobThief() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 108000400;
            ExitMap = 102040000;
            MinMap = 108000400;
            MaxMap = 108000400;
            MaxLobbys = 3;
            ExitPortal = 9;
        }
    }
}
