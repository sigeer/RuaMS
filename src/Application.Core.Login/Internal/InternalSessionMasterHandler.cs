using Application.Shared.Internal;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Login.Internal
{
    public abstract class InternalSessionMasterHandler<TMessage> : InternalSessionHandler<MasterServer, TMessage>, IInternalSessionMasterHandler where TMessage : IMessage
    {
        protected InternalSessionMasterHandler(MasterServer server) : base(server)
        {
        }
    }

    public interface IInternalSessionMasterHandler : IInternalSessionHandler
    {

    }
}
