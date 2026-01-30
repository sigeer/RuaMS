using System;
using System.Collections.Generic;
using System.Text;
using tools;

namespace Application.Core.Channel.Commands
{

    internal class SendWorldBroadcastMessageCommand : IChannelCommand
    {
        int type;
        string message;
        bool onlyGM;

        public SendWorldBroadcastMessageCommand(int type, string message) : this(type, message, false)
        {
        }

        public SendWorldBroadcastMessageCommand(int type, string message, bool gm)
        {
            this.type = type;
            this.message = message;
            this.onlyGM = gm;
        }

        public void Execute(ChannelNodeCommandContext ctx)
        {
            ctx.Server.SendDropMessage(type, message, onlyGM);
        }
    }
}
