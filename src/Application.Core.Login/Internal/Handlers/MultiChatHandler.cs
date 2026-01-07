using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using MessageProto;

namespace Application.Core.Login.Internal.Handlers
{
    internal class MultiChatHandler : InternalSessionMasterHandler<MultiChatMessage>
    {
        public MultiChatHandler(MasterServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelSendCode.MultiChat;

        protected override async Task HandleAsync(MultiChatMessage data, CancellationToken cancellationToken = default)
        {
            if (data.Type == 0)
                await _server.BuddyManager.SendBuddyChatAsync(data.FromName, data.Text, data.Receivers.ToArray());
            else if (data.Type == 1)
                await _server.TeamManager.SendTeamChatAsync(data.FromName, data.Text);
            else if (data.Type == 2)
                await _server.GuildManager.SendGuildChatAsync(data.FromName, data.Text);
            else if (data.Type == 3)
                await _server.GuildManager.SendAllianceChatAsync(data.FromName, data.Text);
        }

        protected override MultiChatMessage Parse(ByteString content) => MultiChatMessage.Parser.ParseFrom(content);
    }
}
