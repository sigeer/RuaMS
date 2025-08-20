using Application.Core.Login;
using Application.Core.Login.Models.Items;
using Application.EF.Entities;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Benchmark
{
    [MemoryDiagnoser]
    public class MapperExpressionBenchrmark
    {
        AutoMapper.IMapper _mapper;
        private TypeAdapterConfig _mapsterConfig;

        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();

            services.AddLoginServerDI("Sqlite", "Data Source=benchmark.db");

            var provider = services.BuildServiceProvider();
            _mapper = provider.GetRequiredService<IMapper>();

            _mapsterConfig = new TypeAdapterConfig();
            _mapsterConfig.Default.PreserveReference(true);
            _mapsterConfig.NewConfig<FredrickStoreModel, FredstorageEntity>();
            _mapsterConfig.NewConfig<FredstorageEntity, FredrickStoreModel>();
        }
        [Benchmark()]
        public void UseAutoMapper()
        {
            var model = new FredrickStoreModel { Meso = 1 };
            _mapper.Map<FredstorageEntity>(model);
        }

        [Benchmark()]
        public void UseMapster()
        {
            var model = new FredrickStoreModel { Meso = 1 };
            model.Adapt<FredstorageEntity>();
        }
    }
}
