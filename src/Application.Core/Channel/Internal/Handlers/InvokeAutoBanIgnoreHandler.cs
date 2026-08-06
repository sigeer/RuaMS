using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class InvokeAutoBanIgnoreHandler : InternalSessionChannelHandler<ProtoService.ToggleAutoBanIgnoreResponse>
    {
        public InvokeAutoBanIgnoreHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.InvokeAutoBanIgnore;

        protected override Task HandleMessage(ProtoService.ToggleAutoBanIgnoreResponse res)
        {
            if (res.Code == 0)
                return _server.PushChannelCommandAsync(new InvokeDropMessageAsyncCommand(res.Request.MasterId, -1, res.Request.TargetName + " is " + (res.IsIgnored ? "now being ignored." : "no longer being ignored.")));
            else
                return _server.PushChannelCommandAsync(new InvokeDropMessageAsyncCommand(res.Request.MasterId, 5, $"未找到玩家：{res.Request.TargetName}"));
        }

        protected override ProtoService.ToggleAutoBanIgnoreResponse Parse(ByteString data) => ProtoService.ToggleAutoBanIgnoreResponse.Parser.ParseFrom(data);
    }
}
