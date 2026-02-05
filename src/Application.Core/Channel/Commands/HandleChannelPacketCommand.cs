using Application.Utility.Performance;

namespace Application.Core.Channel.Commands
{
    internal class HandleChannelPacketCommand : IWorldChannelCommand
    {
        IChannelClient _client;
        InPacket _inPacket;

        public HandleChannelPacketCommand(IChannelClient client, InPacket inPacket)
        {
            _client = client;
            _inPacket = inPacket;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            using var activity = GameMetrics.ActivitySource.StartActivity(nameof(HandleChannelPacketCommand));

            activity?.SetTag("Channel", _client.CurrentServer.InstanceName);
            activity?.SetTag("AccountId", _client.AccountId);
            if (_client.Character != null)
            {
                activity?.SetTag("PlayerId", _client.Character.Id);
                activity?.SetTag("PlayerName", _client.Character.Name);
            }

            _client.ProcessPacket(_inPacket);
        }
    }
}
