using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Warrior4thJob
{
    /// <summary>任务 6153 的副本事件。</summary>
    internal sealed class Event6153 : SoloQuestEventTemplate
    {
        public Event6153() : base(Warrior4thJob.Berserk)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 910500200;
            ExitMap = 105090800;
            MinMap = 910500200;
            MaxMap = 910500200;
            EntryPortal = 2;
        }
    }
}
