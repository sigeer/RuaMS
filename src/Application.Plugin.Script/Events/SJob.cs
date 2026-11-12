using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{

    internal abstract class SoloQuestEventTemplate : AbstractSoloEventTemplate
    {
        protected SoloQuestEventTemplate(int questId) : base($"q{questId}")
        {
            QuestId = (short)questId;
        }

        protected SoloQuestEventTemplate(string eventName) : base(eventName)
        {

        }
    }
}
