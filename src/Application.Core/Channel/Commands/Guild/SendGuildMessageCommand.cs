namespace Application.Core.Channel.Commands
{
    internal class SendGuildMessageCommand : IWorldChannelCommand
    {
        int guildId;
        int type;
        string message;

        public SendGuildMessageCommand(int guildId, int type, string message)
        {
            this.guildId = guildId;
            this.type = type;
            this.message = message;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _ = ctx.WorldChannel.Node.Transport.BroadcastGuildMessage(guildId, type, message);
        }
    }
}
