using Application.EF.Entities;
using Mapster;

namespace Application.Module.Family.Master.Models
{
    internal class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<FamilyCharacterEntity, FamilyCharacterModel>()
                .Map(dest => dest.Id, src => src.Cid);

            config.NewConfig<FamilyCharacterModel, FamilyCharacterEntity>()
                .Map(dest => dest.Cid, src => src.Id);

            config.NewConfig<FamilyEntitlementEntity, FamilyEntitlementUseRecord>()
                .Map(dest => dest.Id, src => src.Entitlementid)
                .Map(dest => dest.Time, src => src.Timestamp);
        }
    }
}
