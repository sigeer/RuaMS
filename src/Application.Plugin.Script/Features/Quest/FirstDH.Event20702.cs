using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class FirstDHEvent20702 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 20702;

        public FirstDHEvent20702() : base(QuestIdValue)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 913000100;
            ExitMap = 130020000;
            MinMap = 913000100;
            MaxMap = 913000100;
        }
    }
}
