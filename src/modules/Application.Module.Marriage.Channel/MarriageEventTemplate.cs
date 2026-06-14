using Application.Core.scripting.Events.Templates;

namespace Application.Module.Marriage.Channel
{
    public class MarriageEventTemplate : AbstractEventTemplate
    {
        public MarriageEventTemplate(string name) : base(name)
        {
            MinCount = 2;
            MaxCount = 2;
        }

    }
}