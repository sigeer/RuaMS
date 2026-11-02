using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q512000</c>。
    /// </summary>
    internal sealed class Q2ndJobWarrior : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 512000;

        public Q2ndJobWarrior() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 108000300;
            ExitMap = 102020300;
            MinMap = 108000300;
            MaxMap = 108000300;
            MaxLobbys = 3;
            ExitPortal = 9;
        }
    }
}
