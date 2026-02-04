using Application.Core.Login.Internal;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Login.Commands
{
    internal class HandleChannelPacketCommand : IMasterCommand
    {
        IInternalSessionMasterHandler _handler;
        ByteString _content;

        public HandleChannelPacketCommand(IInternalSessionMasterHandler handler, ByteString content)
        {
            _handler = handler;
            _content = content;
        }

        public void Execute(MasterCommandContext ctx)
        {
            _handler.Handle(_content);
        }
    }
}
