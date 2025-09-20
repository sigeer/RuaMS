using System.Drawing;

namespace Application.Templates.Reactor
{
    /// <summary>
    /// This contains the data that is shared between all reactors of the same ID.
    /// </summary>
    public sealed class ReactorTemplate : AbstractTemplate, ILinkTemplate<ReactorTemplate>
    {

        [WZPath("info/activateByTouch")]
        public bool ActivateByTouch { get; set; }

        public StateInfo[] StateInfoList { get; set; }
        public ReactorTemplate(int templateId)
            : base(templateId)
        {
            StateInfoList = Array.Empty<StateInfo>();
        }


        public class StateInfo
        {
            public StateInfo(int state)
            {
                State = state;
                EventInfos = Array.Empty<EventInfo>();
            }

            public int State { get; set; }
            [WZPath("x/event/timeOut")]
            public int TimeOut { get; set; } = -1;
            public EventInfo[] EventInfos { get; set; }


            public sealed class EventInfo
            {
                [WZPath("x/event/x/type")]
                public int EventType { get; set; }
                [WZPath("x/event/x/state")]
                public int NextState { get; set; }
                [WZPath("x/event/x/activeSkillID")]
                public int[]? ActiveSkillId { get; set; }

                public int Int0Value { get; set; }
                public int Int1Value { get; set; } = 1;
                public int Int2Value { get; set; }
                public Point Lt { get; set; }
                public Point Rb { get; set; }
            }
        }

        public void CloneLink(ReactorTemplate sourceTemplate)
        {
            sourceTemplate.StateInfoList = StateInfoList;
        }
    }
}
