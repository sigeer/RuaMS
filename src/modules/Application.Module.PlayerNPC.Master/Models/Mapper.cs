using Application.EF.Entities;
using Mapster;

namespace Application.Module.PlayerNPC.Master.Models
{
    internal class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PlayerNpcEquipModel, PlayerNPCProto.PlayerNPCEquip>()
                .TwoWays()
                .Map(dest => dest.ItemId, x => x.Equipid)
                .Map(dest => dest.Position, x => x.Equippos)
                ;
            config.NewConfig<PlayerNpcModel, PlayerNPCProto.PlayerNPCDto>()
                .TwoWays()
                .Map(dest => dest.MapId, x => x.Map);

            config.NewConfig<PlayerNpcEntity, PlayerNpcModel>()
                .TwoWays();
            config.NewConfig<PlayerNpcsEquipEntity, PlayerNpcEquipModel>()
                .TwoWays();
        }
    }
}
