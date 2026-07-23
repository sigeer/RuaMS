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

        protected override Task HandleMessage(MultiChatMessage data)
        {
            if (data.Type == 0)
                return _server.BuddyManager.SendBuddyChatAsync(data.FromName, data.Text, data.Receivers.ToArray());
            else if (data.Type == 1)
                return _server.TeamManager.SendTeamChatAsync(data.FromName, data.Text);
            else if (data.Type == 2)
                return _server.GuildManager.SendGuildChatAsync(data.FromName, data.Text);
            else if (data.Type == 3)
                return _server.GuildManager.SendAllianceChatAsync(data.FromName, data.Text);
            return Task.CompletedTask;
        }

        protected override MultiChatMessage Parse(ByteString content) => MultiChatMessage.Parser.ParseFrom(content);
    }
}
