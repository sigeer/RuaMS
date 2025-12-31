using Application.Resources.Messages;
using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using SystemProto;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class SetGmLevelHandler : InternalSessionChannelHandler<SystemProto.SetGmLevelResponse>
    {
        public SetGmLevelHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => ChannelRecvCode.InvokeSetGmLevel;

        protected override Task HandleAsync(SetGmLevelResponse res, CancellationToken cancellationToken = default)
        {
            var masterChr = _server.FindPlayerById(res.Request.OperatorId);
            if (masterChr != null)
            {
                if (res.Code == 0)
                {
                    masterChr.Yellow(nameof(ClientMessage.SetGmLevelCommand_Result), res.Request.TargetName, res.Request.Level.ToString());
                }
                else
                {
                    masterChr.Yellow(nameof(ClientMessage.PlayerNotFoundInChannel), res.Request.TargetName);
                }
            }

            var chr = _server.FindPlayerById(res.TargetId);
            if (chr != null)
            {
                chr.Client.AccountEntity!.GMLevel = (sbyte)res.Request.Level;
                chr.Notice(nameof(ClientMessage.Notice_GmLevelChanged), res.Request.Level.ToString());
            }

            return Task.CompletedTask;
        }

        protected override SetGmLevelResponse Parse(ByteString content) => SetGmLevelResponse.Parser.ParseFrom(content);
    }
}
