using Mapster;
using System.Drawing;

namespace Application.Module.PlayerNPC.Channel.Models
{
    internal class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PlayerNPCProto.PlayerNPCEquip, PlayerNpcEquipObject>()
                .TwoWays()
                .Map(dest => dest.EquipId, x => x.ItemId)
                .Map(dest => dest.EquipPos, x => x.Position);

            config.NewConfig<PlayerNPCProto.PlayerNPCDto, PlayerNpc>()
                .AfterMapping((src, dest) =>
                {
                    dest.setObjectId(dest.Id);
                    dest.setPosition(new Point(dest.X, dest.Cy));
                });

        }
    }
}
