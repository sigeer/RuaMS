using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Warrior4thJob
{
    internal sealed class Warrior4thJobEvent6192 : SoloQuestEventTemplate
    {
        const int ENTRY_MAP = 921100300;
        const int EXIT_MAP = 211040100;

        public Warrior4thJobEvent6192() : base(Warrior4thJob.PowerStance)
        {
            MinCount = 1;
            MaxCount = 4;
            MinLevel = 80;
            MaxLevel = 255;

            RecruitMap = 211000001;
            EntryMap = ENTRY_MAP;
            ExitMap = EXIT_MAP;
            ClearMap = EXIT_MAP;

            MinMap = ENTRY_MAP;
            MaxMap = ENTRY_MAP;

            EventTime = 10 * 60;
        }

    }
}
