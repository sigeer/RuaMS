using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using System.Xml.Linq;
using tools;

namespace Application.Core.scripting.Events.Instances
{
    public class ExpeditionEventInstanceManager : BehindPartyQuestEventInstanceManager
    {
        public ExpeditionEventInstanceManager(ExpeditionEventManager em, string name) : base(em, name)
        {
        }
    }
}
