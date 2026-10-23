using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q513000</c>。
    /// </summary>
    internal sealed class Q3rdJobWarrior : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 513000;

        public Q3rdJobWarrior() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 108010300;
            ExitMap = 105070001;
            MinMap = 108010300;
            MaxMap = 108010301;
        }
    }
}
