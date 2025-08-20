using Application.Core.Login;
using Application.Core.Login.Models.Items;
using Application.EF;
using Application.EF.Entities;
using AutoMapper;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace ServiceTest.Infrastructure
{
    internal class MapperTest
    {
        AutoMapper.IMapper _mapper;
        IDbContextFactory<DBContext> _dbContextFactory;
        private TypeAdapterConfig _mapsterConfig;
        MapsterMapper.IMapper _mapper2;

        static Expression<Func<FredrickStoreModel, bool>> expression = x => x.Meso > 0;
        public MapperTest()
        {
            var services = new ServiceCollection();

            services.AddLoginServerDI("Sqlite", "Data Source=benchmark.db");

            var provider = services.BuildServiceProvider();
            _mapper = provider.GetRequiredService<IMapper>();

            _dbContextFactory = provider.GetRequiredService<IDbContextFactory<DBContext>>();

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

            using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Database.Migrate();
        }

        [Test]
        public void MapsterMapTest()
        {
            var model = new FredrickStoreModel { Meso = 1 };
            Assert.That(model.Adapt<FredstorageEntity>().Meso == model.Meso);
        }

        [Test]
        public void UseAutoMapper()
        {
            var model = new FredrickStoreModel { Meso = 1 };
            Assert.That(_mapper.Map<FredstorageEntity>(model).Meso == model.Meso);
        }

        /// <summary>
        /// 性能更好但是不支持类似AutoMapper的MapExpression
        /// https://github.com/MapsterMapper/Mapster/issues/517
        /// </summary>
        //[Test]
        //public void MapsterMapExpressionTest()
        //{
        //    using var dbContext = _dbContextFactory.CreateDbContext();

        //    var entityExpression = expression.Adapt<Expression<Func<FredstorageEntity, bool>>>(_mapsterConfig);
        //    var dataFromDB = dbContext.Fredstorages.Where(entityExpression).AsNoTracking().ToList();
        //    Assert.Pass();
        //}
    }
}
