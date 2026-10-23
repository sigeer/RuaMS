using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q522000</c>。
    /// </summary>
    internal sealed class Q2ndJobMagician : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 522000;

        public Q2ndJobMagician() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 108000200;
            ExitMap = 101020000;
            MinMap = 108000200;
            MaxMap = 108000200;
            MaxLobbys = 3;
            ExitPortal = 9;
        }
    }
}
