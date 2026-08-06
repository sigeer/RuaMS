using Mapster;

namespace Application.Module.Duey.Channel.Models
{
    internal class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ProtoModel.DueyPackageProto, DueyPackageObject>();
        }
    }
}
