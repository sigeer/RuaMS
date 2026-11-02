using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{

    internal abstract class SoloQuestEventTemplate : AbstractSoloEventTemplate
    {
        /// <summary>
        /// 任务 Id，同时也是事件注册键（q{QuestId}）。
        /// </summary>
        public int QuestId { get; }

        protected SoloQuestEventTemplate(int questId) : base($"q{questId}")
        {
            QuestId = questId;
        }

        protected SoloQuestEventTemplate(string eventName) : base(eventName)
        {

        }
    }
}
