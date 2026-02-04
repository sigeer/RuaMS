using System.Net;
using tools;

namespace Application.Core.Channel.Commands
{
    public class PlayerPreEnterChannelCommand : IWorldChannelCommand
    {
        int _chrId;
        IPEndPoint _channelSocket;
        bool fromChannel;

        public PlayerPreEnterChannelCommand(int chrId, IPEndPoint channelSocket, bool fromChannel)
        {
            _chrId = chrId;
            _channelSocket = channelSocket;
            this.fromChannel = fromChannel;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = fromChannel 
                ? ctx.WorldChannel.getPlayerStorage().getCharacterById(_chrId)
                : ctx.WorldChannel.PlayersAway.GetValueOrDefault(_chrId);
            if (chr != null)
            {
                if (fromChannel)
                    chr.Client.CurrentServer.NodeService.DataService.SaveBuff(chr);

                chr.Client.SetCharacterOnSessionTransitionState(_chrId);
                chr.sendPacket(PacketCreator.getChannelChange(_channelSocket));
            }
        }
    }
}
