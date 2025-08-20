using Application.Core.Login;
using Application.Core.Login.Models.Items;
using Application.EF.Entities;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using Facet;
using FastExpressionCompiler;
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

            TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileFast();
            _mapsterConfig = new TypeAdapterConfig();
            _mapsterConfig.Default.PreserveReference(true);
            _mapsterConfig.NewConfig<FredrickStoreModel, FredstorageEntity>()
                                    .MapWith(src => new FredstorageEntity(src.Id, src.Cid, src.Daynotes, src.Meso, DateTimeOffset.FromUnixTimeMilliseconds(src.UpdateTime))); ;
            _mapsterConfig.NewConfig<FredstorageEntity, FredrickStoreModel>()
                    .MapWith(src => new FredrickStoreModel
                    {
                        Id = src.Id,
                        Cid = src.Cid,
                        Daynotes = src.Daynotes,
                        Meso = src.Meso,
                        ItemMeso = src.ItemMeso,
                        UpdateTime = src.Timestamp.ToUnixTimeMilliseconds()
                    }); ;
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

        /// <summary>
        /// 性能最佳，但是不支持双向映射。更倾向于创建复制类型而不是类型映射
        /// </summary>
        [Benchmark()]
        public void UseFacet()
        {
            var model = new FredrickStoreModel { Meso = 1 };
            new FredstorageEntityLocal(model);
        }
    }


    [Facet(typeof(FredrickStoreModel), nameof(FredrickStoreModel.Items))]
    public partial class FredstorageEntityLocal { }

    //public class FredstorageMapper : IFacetMapConfiguration<FredrickStoreModel, FredstorageEntity>
    //{
    //    public static void Map(FredrickStoreModel source, FredstorageEntity target)
    //    {
    //        target.Daynotes = source.Daynotes;
    //        target.Cid = source.Cid;
    //        target.Id = source.Id;
    //        target.Meso = source.Meso;
    //        target.ItemMeso = (int)source.ItemMeso;
    //    }
    //}

}
