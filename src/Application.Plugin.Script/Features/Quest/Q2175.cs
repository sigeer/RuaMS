using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q2175</c>。
    /// </summary>
    internal sealed class Q2175 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 2175;

        public Q2175() : base(QuestIdValue)
        {
            EventTime = 20 * 60;
            MaxLobbys = 1;

            EntryMap = 912000000;
            ExitMap = 120000100;
            MinMap = 912000000;
            MaxMap = 912000000;
        }

    }
}
