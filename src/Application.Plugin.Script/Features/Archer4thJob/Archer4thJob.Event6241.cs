using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Archer4thJob
{
    /// <summary>任务 6241 的副本事件。</summary>
    internal sealed class Archer4thJobEvent6241 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 6241;

        public Archer4thJobEvent6241() : base(QuestIdValue)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 924000100;
            ExitMap = 240010700;
            MinMap = 924000100;
            MaxMap = 924000100;
        }
    }
}
