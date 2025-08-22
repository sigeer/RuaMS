using Application.Core.Login.Datas;
using Application.EF.Entities;
using Application.Utility.Compatible.Atomics;
using Mapster;

namespace Application.Module.Duey.Master.Models
{
    internal class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<DueyPackageEntity, DueyPackageModel>()
                .Map(dest => dest.Id, x => x.PackageId);

            config.NewConfig<DueyPackageModel, DueyDto.DueyPackageDto>()
                .Map(dest => dest.PackageId, x => x.Id);
        }
    }
}
