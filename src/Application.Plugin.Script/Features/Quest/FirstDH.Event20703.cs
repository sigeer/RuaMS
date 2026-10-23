using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class FirstDHEvent20703 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 20703;

        public FirstDHEvent20703() : base(QuestIdValue)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 913000200;
            ExitMap = 130020000;
            MinMap = 913000200;
            MaxMap = 913000200;
        }
    }
}
