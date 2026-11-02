using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 单人任务副本事件，注册键为 <c>q21746</c>。
    /// </summary>
    internal sealed class Q21746 : SoloQuestEventTemplate
    {
        internal const int QuestIdValue = 21746;

        public Q21746() : base(QuestIdValue)
        {
            EventTime = 30 * 60;
            MaxLobbys = 1;

            EntryMap = 925040001;
            ExitMap = 925040000;
            MinMap = 925040001;
            MaxMap = 925040001;
        }

    }
}
