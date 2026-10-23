using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q553000</c>。
    /// </summary>
    internal sealed class Q3rdJobPirate : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 553000;

        public Q3rdJobPirate() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 108010500;
            ExitMap = 105070200;
            MinMap = 108010500;
            MaxMap = 108010501;
        }
    }
}
