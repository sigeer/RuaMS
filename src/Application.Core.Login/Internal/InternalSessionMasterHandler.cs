using Application.Shared.Internal;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
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

    public abstract class InternalSessionMasterEmptyHandler : InternalSessionMasterHandler<Empty>
    {
        static Empty Empty = new Empty();
        protected InternalSessionMasterEmptyHandler(MasterServer server) : base(server)
        {
        }

        protected override Empty Parse(ByteString data) => Empty;
    }

    public interface IInternalSessionMasterHandler : IInternalSessionHandler
    {

    }
}
