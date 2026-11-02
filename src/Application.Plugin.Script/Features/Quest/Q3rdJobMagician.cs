using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q523000</c>。
    /// </summary>
    internal sealed class Q3rdJobMagician : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 523000;

        public Q3rdJobMagician() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 108010200;
            ExitMap = 100040106;
            MinMap = 108010200;
            MaxMap = 108010201;
        }
    }
}
