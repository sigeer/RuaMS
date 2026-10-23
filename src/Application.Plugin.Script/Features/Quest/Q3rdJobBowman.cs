using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q533000</c>。
    /// </summary>
    internal sealed class Q3rdJobBowman : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 533000;

        public Q3rdJobBowman() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 108010100;
            ExitMap = 105040305;
            MinMap = 108010100;
            MaxMap = 108010101;
        }
    }
}
