using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q532000</c>。
    /// </summary>
    internal sealed class Q2ndJobBowman : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 532000;

        public Q2ndJobBowman() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 108000100;
            ExitMap = 106010000;
            MinMap = 108000100;
            MaxMap = 108000100;
            MaxLobbys = 3;
            ExitPortal = 9;
        }
    }
}
