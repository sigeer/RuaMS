using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class FirstDHEvent20701 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 20701;

        public FirstDHEvent20701() : base(QuestIdValue)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 913000000;
            ExitMap = 130020000;
            MinMap = 913000000;
            MaxMap = 913000000;
        }
    }
}
