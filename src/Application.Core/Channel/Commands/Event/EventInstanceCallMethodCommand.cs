using Application.Core.Scripting.Events;
using System.Xml.Linq;

namespace Application.Core.Channel.Commands
{
    public class EventCallMethodCommand : IWorldChannelCommand
    {
        EventManager _em;
        string _methodName;
        AbstractEventInstanceManager? _eim;

        public EventCallMethodCommand(EventManager em, string methodName, AbstractEventInstanceManager? eim)
        {
            _em = em;
            _methodName = methodName;
            _eim = eim;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _em.InvokeMethod(_methodName, _eim);
        }
    }

    public class EventInstanceCallMethodCommand : IWorldChannelCommand
    {
        string _methodName;
        AbstractEventInstanceManager _eim;

        public EventInstanceCallMethodCommand(AbstractEventInstanceManager eim, string methodName)
        {
            _methodName = methodName;
            _eim = eim;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _eim.invokeScriptFunction(_methodName, _eim);
        }
    }

    public class EventDisposeCommand: IWorldChannelCommand
    {
        AbstractInstancedEventManager _em;
        string _eimName;

        public EventDisposeCommand(AbstractInstancedEventManager em, string eimName)
        {
            _em = em;
            _eimName = eimName;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _em.ProcessDisposeInstanceInternal(_eimName);
        }
    }
}
