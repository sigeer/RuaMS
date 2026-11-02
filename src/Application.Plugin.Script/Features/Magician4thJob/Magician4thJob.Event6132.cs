using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Magician4thJob
{
    /// <summary>任务 6132 的副本事件。</summary>
    internal sealed class Magician4thJobEvent6132 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 6132;

        public Magician4thJobEvent6132() : base(QuestIdValue)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 923000100;
            ExitMap = 230040300;
            MinMap = 923000100;
            MaxMap = 923000100;
        }
    }
}
