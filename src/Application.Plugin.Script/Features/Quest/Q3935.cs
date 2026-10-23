using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q3935</c>。
    /// </summary>
    internal sealed class Q3935 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 3935;

        public Q3935() : base(QuestIdValue)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 926000010;
            ExitMap = 260000302;
            MinMap = 926000010;
            MaxMap = 926000010;
        }

    }
}
