using Application.Core.Game.Skills;
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
                        await client.OnlinedCharacter.SendPacket(PacketCreator.giveForeignBuff(mapChr.Id, new BuffStatValue(BuffStat.DARKSIGHT, 0)));
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
