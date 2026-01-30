using System.Net;
using tools;

namespace Application.Core.Channel.Commands
{
    public class PlayerPreEnterChannelCommand : IWorldChannelCommand
    {
        int _chrId;
        IPEndPoint _channelSocket;
        bool saveBuff;

        public PlayerPreEnterChannelCommand(int chrId, IPEndPoint channelSocket, bool saveBuff = false)
        {
            _chrId = chrId;
            _channelSocket = channelSocket;
            this.saveBuff = saveBuff;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(_chrId);
            if (chr != null)
            {
                if (saveBuff)
                    chr.Client.CurrentServer.NodeService.DataService.SaveBuff(chr);

                chr.Client.SetCharacterOnSessionTransitionState(_chrId);
                chr.sendPacket(PacketCreator.getChannelChange(_channelSocket));
            }
        }
    }
}
