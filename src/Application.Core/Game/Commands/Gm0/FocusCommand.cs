using Application.Core.Channel.Net.Packets;
using tools;

namespace Application.Core.Game.Commands.Gm0
{
    internal class FocusCommand : CommandBase
    {
        public FocusCommand() : base(0, ["focus"])
        {
        }

        public override async Task Execute(IChannelClient client, string[] values)
        {
            if (values.Length == 0)
            {
                foreach (var mapChr in client.OnlinedCharacter.MapModel.getAllPlayers())
                {
                    if (mapChr.Id != client.OnlinedCharacter.Id)
                    {
                        await client.OnlinedCharacter.SendPacket(BuffPackets.GiveRemoteHiddenBuff(mapChr.Id));
                    }
                }
            }
            else if (values[0].Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                List<BuffStat> cancelStat = [BuffStat.DARKSIGHT];
                foreach (var mapChr in client.OnlinedCharacter.MapModel.getAllPlayers())
                {
                    if (mapChr.Id != client.OnlinedCharacter.Id && !mapChr.HasBuff(BuffStat.DARKSIGHT))
                    {
                        await client.OnlinedCharacter.SendPacket(PacketCreator.cancelForeignBuff(mapChr.Id, cancelStat));
                    }
                }
            }

        }
    }
}
