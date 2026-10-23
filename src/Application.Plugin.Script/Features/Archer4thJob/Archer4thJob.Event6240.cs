using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Archer4thJob
{
    /// <summary>任务 6240 的副本事件。</summary>
    internal sealed class Archer4thJobEvent6240 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 6240;

        public Archer4thJobEvent6240() : base(QuestIdValue)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 921100200;
            ExitMap = 211042200;
            MinMap = 921100200;
            MaxMap = 921100200;
        }
    }
}
