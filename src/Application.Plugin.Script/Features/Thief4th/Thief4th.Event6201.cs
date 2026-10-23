using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Thief4th
{
    /// <summary>任务 6201 的副本事件。</summary>
    internal sealed class Thief4thEvent6201 : SoloQuestEventTemplate
    {
        public Thief4thEvent6201() : base(Thief4th.Assassinate)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 910200000;
            ExitMap = 101030104;
            MinMap = 910200000;
            MaxMap = 910200000;
        }
    }
}
