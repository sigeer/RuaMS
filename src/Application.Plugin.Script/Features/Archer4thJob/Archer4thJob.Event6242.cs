using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Archer4thJob
{
    /// <summary>任务 6242 的副本事件。</summary>
    internal sealed class Archer4thJobEvent6242 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 6242;

        public Archer4thJobEvent6242() : base(QuestIdValue)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 921100210;
            ExitMap = 211040700;
            MinMap = 921100210;
            MaxMap = 921100210;
        }
    }
}
