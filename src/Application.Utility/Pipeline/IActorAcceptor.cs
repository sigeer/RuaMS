using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Utility.Pipeline
{
    public interface IActorAcceptor<TContext>
    {
        void Send(ICommand command);

        void Send(Func<TContext, Task> action);
        void Send(Action<TContext> action);
    }
}
