using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Config;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class InvokeAutoBanIgnoreHandler : InternalSessionChannelHandler<Config.ToggleAutoBanIgnoreResponse>
    {
        public InvokeAutoBanIgnoreHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.InvokeAutoBanIgnore;

        protected override void HandleMessage(ToggleAutoBanIgnoreResponse res)
        {
            if (res.Code == 0)
                _server.PushChannelCommand(new InvokeDropMessageCommand(res.Request.MasterId, -1, res.Request.TargetName + " is " + (res.IsIgnored ? "now being ignored." : "no longer being ignored.")));
            else
                _server.PushChannelCommand(new InvokeDropMessageCommand(res.Request.MasterId, 5, $"未找到玩家：{res.Request.TargetName}"));

        }

        protected override ToggleAutoBanIgnoreResponse Parse(ByteString data) => ToggleAutoBanIgnoreResponse.Parser.ParseFrom(data);
    }
}
