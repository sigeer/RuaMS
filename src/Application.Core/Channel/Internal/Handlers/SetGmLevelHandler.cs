using Application.Core.Channel.Commands;
using Application.Resources.Messages;
using Application.Shared.Message;
using Google.Protobuf;
using System.Runtime.ConstrainedExecution;
using SystemProto;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class SetGmLevelHandler : InternalSessionChannelHandler<SystemProto.SetGmLevelResponse>
    {
        public SetGmLevelHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.InvokeSetGmLevel;

        protected override void HandleMessage(SetGmLevelResponse res)
        {
            _server.Broadcast(w =>
            {
                w.getPlayerStorage().GetCharacterActor(res.Request.OperatorId)?
                    .Send(m =>
                    {
                        var masterChr = m.getCharacterById(res.Request.OperatorId);
                        if (masterChr != null)
                        {
                            if (res.Code != 0)
                                masterChr.Yellow(nameof(ClientMessage.PlayerNotFound), res.Request.TargetName);
                            else
                            {
                                masterChr.Yellow(nameof(ClientMessage.SetGmLevelCommand_Result), res.Request.TargetName, res.Request.Level.ToString());
                            }
                        }
                    });

                if (res.Code == 0)
                {
                    w.getPlayerStorage().GetCharacterActor(res.TargetId)?
                        .Send(m =>
                        {
                            var chr = m.getCharacterById(res.TargetId);
                            if (chr != null)
                            {
                                chr.Client.AccountEntity!.GMLevel = (sbyte)res.Request.Level;
                                chr.Notice(nameof(ClientMessage.Notice_GmLevelChanged), res.Request.Level.ToString());
                            }
                        });
                }
            });
        }

        protected override SetGmLevelResponse Parse(ByteString content) => SetGmLevelResponse.Parser.ParseFrom(content);
    }
}
