using System.Net;
using tools;

namespace Application.Core.Channel.Commands
{
    public class PlayerPreEnterChannelCommand : IWorldChannelAsyncCommand
    {
        public string Name => nameof(PlayerPreEnterChannelCommand);
        int _chrId;
        IPEndPoint _channelSocket;
        bool fromChannel;

        public PlayerPreEnterChannelCommand(int chrId, IPEndPoint channelSocket, bool fromChannel)
        {
            _chrId = chrId;
            _channelSocket = channelSocket;
            this.fromChannel = fromChannel;
        }

        public async Task Execute(WorldChannel w)
        {
            if (fromChannel)
            {
                var chr = w.getPlayerStorage().getCharacterById(_chrId);
                if (chr == null)
                {
                    return;
                }
                chr.Client.CurrentServer.NodeService.DataService.SaveBuff(chr);

                chr.Client.SetCharacterOnSessionTransitionState(_chrId);
                await chr.SendPacket(PacketCreator.getChannelChange(_channelSocket));
            }
            else
            {
                var client = w.PlayersAway.GetValueOrDefault(_chrId);
                if (client == null)
                {
                    return;
                }
                client.SetCharacterOnSessionTransitionState(_chrId);
                await client.SendPacket(PacketCreator.getChannelChange(_channelSocket));
            }
        }
    }
}
