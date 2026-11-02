using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Thief4th
{
    /// <summary>任务 6230 的副本事件。</summary>
    internal sealed class Thief4thEvent6230 : SoloQuestEventTemplate
    {
        public Thief4thEvent6230() : base(Thief4th.NinjaStorm)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 922020200;
            ExitMap = 220010001;
            MinMap = 922020200;
            MaxMap = 922020200;
        }
    }
}
