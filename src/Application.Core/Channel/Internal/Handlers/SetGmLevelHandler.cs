using Application.Resources.Messages;
using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class SetGmLevelHandler : InternalSessionChannelHandler<ProtoService.SetGmLevelResponse>
    {
        public SetGmLevelHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.InvokeSetGmLevel;

        protected override async Task HandleMessage(ProtoService.SetGmLevelResponse res)
        {
            if (res.Code == 0)
            {
                await _server.SendToPlayersAsync([res.Request.OperatorId, res.TargetId], async chr =>
                {
                    if (chr.Id == res.Request.OperatorId)
                    {
                        await chr.Yellow(nameof(ClientMessage.SetGmLevelCommand_Result), res.Request.TargetName, res.Request.Level.ToString());
                    }
                    else if (chr.Id == res.TargetId)
                    {
                        chr.Client.AccountEntity!.GMLevel = (sbyte)res.Request.Level;
                        await chr.Notice(nameof(ClientMessage.Notice_GmLevelChanged), res.Request.Level.ToString());
                    }
                });
            }
            else
            {
                await _server.SendToPlayerAsync(res.Request.OperatorId, chr =>
                {
                    return chr.Yellow(nameof(ClientMessage.PlayerNotFound), res.Request.TargetName);
                });

            }
        }

        protected override ProtoService.SetGmLevelResponse Parse(ByteString content) => ProtoService.SetGmLevelResponse.Parser.ParseFrom(content);
    }
}
