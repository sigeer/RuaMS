using Application.Shared.Internal;
using Application.Shared.Message;
using Config;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Xml.Linq;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class InvokeAutoBanIgnoreHandler : InternalSessionChannelHandler<Config.ToggleAutoBanIgnoreResponse>
    {
        public InvokeAutoBanIgnoreHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.InvokeAutoBanIgnore;

        protected override Task HandleAsync(ToggleAutoBanIgnoreResponse res, CancellationToken cancellationToken = default)
        {
            var chr = _server.FindPlayerById(res.Request.MasterId);
            if (chr != null)
            {
                if (res.Code == 0)
                {
                    chr.yellowMessage(res.Request.TargetName + " is " + (res.IsIgnored ? "now being ignored." : "no longer being ignored."));
                }
                else
                {
                    chr.dropMessage($"未找到玩家：{res.Request.TargetName}");
                }
            }
            return Task.CompletedTask;

        }

        protected override ToggleAutoBanIgnoreResponse Parse(ByteString data) => ToggleAutoBanIgnoreResponse.Parser.ParseFrom(data);
    }
}
