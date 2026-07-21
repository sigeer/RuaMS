using Application.Core.Login.Models;
using Application.EF.Entities;

namespace Application.Core.Login.Mappers
{
    internal class PlayerNPCMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PlayerNpcEquipModel, LifeProto.PlayerNPCEquip>()
                .Map(dest => dest.ItemId, src => src.Equipid)
                .Map(dest => dest.Position, src => src.Equippos);
            config.NewConfig<LifeProto.PlayerNPCEquip, PlayerNpcEquipModel>()
                .Map(dest => dest.Equipid, src => src.ItemId)
                .Map(dest => dest.Equippos, src => src.Position);

            config.NewConfig<PlayerNpcModel, LifeProto.PlayerNPCDto>()
                .Map(dest => dest.MapId, src => src.Map)
                .Map(dest => dest.ScriptId, src => src.NpcId);
            config.NewConfig<LifeProto.PlayerNPCDto, PlayerNpcModel>()
                .Map(dest => dest.Map, src => src.MapId)
                .Map(dest => dest.NpcId, src => src.ScriptId);

            config.NewConfig<PlayerNpcEntity, PlayerNpcModel>()
                .Map(dest => dest.NpcId, src => src.Scriptid);
            config.NewConfig<PlayerNpcModel, PlayerNpcEntity>()
                .Map(dest => dest.Scriptid, src => src.NpcId);

            config.NewConfig<PlayerNpcsEquipEntity, PlayerNpcEquipModel>();
            config.NewConfig<PlayerNpcEquipModel, PlayerNpcsEquipEntity>();
        }
    }
}
