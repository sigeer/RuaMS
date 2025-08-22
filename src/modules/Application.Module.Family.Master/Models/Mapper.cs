using Application.EF.Entities;
using Mapster;

namespace Application.Module.Family.Master.Models
{
    internal class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<FamilyCharacterEntity, FamilyCharacterModel>()
                .TwoWays()
                .Map(dest => dest.Id, x => x.Cid);

            config.NewConfig<FamilyEntitlementEntity, FamilyEntitlementUseRecord>()
                .TwoWays()
                .Map(dest => dest.Id, x => x.Entitlementid)
                .Map(dest => dest.Time, x => x.Timestamp);
        }
    }
}
